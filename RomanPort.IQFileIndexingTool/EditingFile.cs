using RomanPort.LibSDR;
using RomanPort.LibSDR.Demodulators;
using RomanPort.LibSDR.Extras;
using RomanPort.LibSDR.Extras.RDS;
using RomanPort.LibSDR.Extras.RDS.Features;
using RomanPort.LibSDR.Framework.Util;
using RomanPort.LibSDR.Receivers;
using RomanPort.LibSDR.Sources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RomanPort.IQFileIndexingTool
{
    public class EditingFile
    {
        public string originalPathname;
        public string savedPathname;
        public string id;
        public FileInfo fileInfo;
        public string fileSize;
        public ListViewItem item;

        private SDRRadio radio;
        public WbFmDemodulator demodulator;
        private WavStreamSource source;
        private FFPlayReceiver audio;
        public RDSClient rds;

        private UserMetadataState userMetadata;
        private string sha256;
        private Bitmap waveformThumb;
        private bool opened;
        private Form1 form;
        private bool thumbnailGenerationCompleted;

        private Thread shaGenerationThread;
        private Thread thumbGenerationThread;

        public EditingFile(string originalPathname)
        {
            this.originalPathname = originalPathname;
            userMetadata = new UserMetadataState();
            fileInfo = new FileInfo(originalPathname);
            userMetadata.time = fileInfo.LastWriteTime;
            fileSize = Math.Round((double)fileInfo.Length / 1024 / 1024, 1).ToString() + " MB";

            //Generate ID
            byte[] idBuffer = new byte[4];
            Program.rand.NextBytes(idBuffer);
            id = BitConverter.ToString(idBuffer).Replace("-", "");
        }
        
        public unsafe void OpenEditor(Form1 form)
        {
            //Update state
            opened = true;
            this.form = form;
            

            //Get output pathname
            UpdateSavedPathname();
            
            //Open radio source
            source = new WavStreamSource(new FileStream(originalPathname, FileMode.Open, FileAccess.Read), false, 0);

            //Open radio demodulator
            demodulator = new WbFmDemodulator();
            demodulator.ifGain = Form1.defaultIfGain;
            demodulator.EnableMpxFFT().OnFFTWindowAvailable += EditingFile_OnFFTWindowAvailable1;
            rds = demodulator.UseRds();
            rds.OnPiCodeUpdated += EditingFile_OnPiCodeUpdated;
            rds.featureRadioText.RDSFeatureRadioText_RadioTextUpdatedEvent += FeatureRadioText_RDSFeatureRadioText_RadioTextUpdatedEvent;

            //Open radio audio
            audio = new FFPlayReceiver();

            //Open radio
            radio = new SDRRadio(new SDRRadioConfig
            {
                realtime = true,
                bufferSize = 32768
            });
            radio.OpenRadio(source);
            radio.AddDemodReceiver(audio);
            radio.SetDemodulator(demodulator, 250000);
            radio.EnableFFT().OnFFTWindowAvailable += EditingFile_OnFFTWindowAvailable;

            //Update track bar
            form.trackProgress.Value = 0;
            form.trackProgress.Maximum = (int)GetLengthSeconds();

            //Set user metadata
            form.userMetadata = userMetadata;
            userMetadata.radio = radio.iqSampleRate == 900001 ? "RTL-SDR" : "AirSpy";
            form.LoadUserMetadata();

            //Set our metadata
            form.metaSampleRate.Text = radio.iqSampleRate.ToString();
            form.metaSize.Text = fileSize;

            //Create and set waveform thumb
            if (waveformThumb == null)
            {
                waveformThumb = new Bitmap(form.waveformView.Width, form.waveformView.Height);
                GenerateWaveformPreviewThread();
            }
            form.waveformView.Image = waveformThumb;

            //Create and set hash
            if (sha256 == null)
            {
                form.metaHash.Text = "Calculating...";
                GenerateSha256HashThread();
            } else
            {
                form.metaHash.Text = sha256;
            }

            //Set buttons
            UpdateButtonStatus();
            form.timeUpdateTimer.Start();
        }

        private unsafe void EditingFile_OnFFTWindowAvailable1(float* window, int width)
        {
            form.mpxSpectrumView.UpdateFFT(window, width);
        }

        private unsafe void EditingFile_OnFFTWindowAvailable(float* window, int width)
        {
            form.fftSpectrumView1.UpdateFFT(window, width);
        }

        private void FeatureRadioText_RDSFeatureRadioText_RadioTextUpdatedEvent(string text)
        {
            if(RDSFeatureRadioText.TryGetTrackInfo(text, out string trackTitle, out string trackArtist, out string stationName))
            {
                form.Invoke((MethodInvoker)delegate
                {
                    form.metaArtist.Text = trackArtist;
                    form.metaTitle.Text = trackTitle;
                });
            }
        }

        private void EditingFile_OnPiCodeUpdated(LibSDR.Extras.RDS.RDSClient session, ushort piCode)
        {
            form.Invoke((MethodInvoker)delegate
            {
                if (RDSClient.TryGetCallsign(piCode, out string callsign))
                    form.metaCall.Text = callsign + "-FM";
            });
        }

        public void UpdateSavedPathname()
        {
            savedPathname = form.outputDirectory + $"{userMetadata.stationValue} - {userMetadata.artistValue} - {userMetadata.titleValue} - {id}.wav";
            form.alreadyExistsBadge.Visible = File.Exists(savedPathname);
        }

        public void CloseEditor()
        {
            //Update state
            opened = false;
            form.timeUpdateTimer.Stop();

            //Disable buttons
            form.btnDelete.Enabled = false;
            form.btnSave.Enabled = false;
            form.alreadyExistsBadge.Visible = false;

            //Stop radio
            radio.StopRadio();
            audio.Dispose();
        }

        public void Shutdown()
        {
            //Run close thread tasks
            CloseEditor();
        }

        private void GenerateSha256HashThread()
        {
            shaGenerationThread = new Thread(() =>
            {
                //Oppn file and begin hashing
                byte[] hash;
                using (FileStream fs = new FileStream(originalPathname, FileMode.Open, FileAccess.Read))
                using (SHA256 a = SHA256.Create())
                {
                    hash = a.ComputeHash(fs);
                }

                //Convert this to a hex string
                sha256 = BitConverter.ToString(hash).Replace("-", "");

                //Update in the interface
                form.Invoke((System.Windows.Forms.MethodInvoker)delegate
                {
                    if(opened)
                        form.metaHash.Text = sha256;
                    UpdateButtonStatus();
                });
            });
            shaGenerationThread.IsBackground = true;
            shaGenerationThread.Start();
        }

        private unsafe void GenerateWaveformPreviewThread()
        {
            //Get parameters and create Bitmap
            int halfHeight = waveformThumb.Height / 2;
            Graphics gfx = Graphics.FromImage(waveformThumb);
            Pen pen = new Pen(Color.Red, 1);

            //Open buffer
            UnsafeBuffer dataBuffer = UnsafeBuffer.Create(waveformThumb.Width, sizeof(float));
            float* dataBufferPtr = (float*)dataBuffer;

            //Generate
            thumbGenerationThread = new Thread(() =>
            {
                //Generate
                using(WaveformGenerator g = new WaveformGenerator(originalPathname, new FmDemodulator(), 250000, 19000, 16384))
                {
                    g.OnWaveformChunkProgress += (int x, WaveformGenerator sender, object context) =>
                    {
                        //Draw this
                        float size = dataBufferPtr[x - 1] * halfHeight * 1.25f;
                        gfx.DrawLine(pen, x - 1, halfHeight - size, x - 1, halfHeight + size);

                        //Render
                        if (opened)
                            form.waveformView.Invalidate();
                    };
                    g.RequestFull(dataBufferPtr, waveformThumb.Width);
                }

                //Clean up
                dataBuffer.Dispose();
                thumbnailGenerationCompleted = true;

                //Update
                form.Invoke((System.Windows.Forms.MethodInvoker)delegate
                {
                    UpdateButtonStatus();
                });
            });
            thumbGenerationThread.IsBackground = true;
            thumbGenerationThread.Start();
        }

        public float GetLengthSeconds()
        {
            return source.GetLengthSeconds();
        }

        public float GetPositionSeconds()
        {
            return source.GetPositionSeconds();
        }

        public string GetTitle()
        {
            return fileInfo.Name;
        }

        public void JumpToPositionSeconds(float seconds)
        {
            source.SafeSkipToSeconds(seconds);
        }

        public void UpdateButtonStatus()
        {
            if (!opened)
                return;
            bool backgroundTasksCompleted = sha256 != null && thumbnailGenerationCompleted;
            bool metadataFilled = userMetadata.IsFilledOut();
            form.btnDelete.Enabled = backgroundTasksCompleted;
            form.btnSave.Enabled = backgroundTasksCompleted && metadataFilled;
            UpdateSavedPathname();
        }

        public string GetCopyString()
        {
            return $"{userMetadata.stationValue}\t{userMetadata.artistValue}\t{userMetadata.titleValue}\t{userMetadata.time.ToShortDateString()}\tUnzipped (ID)\t{userMetadata.prefix}\t{userMetadata.suffix}\t{userMetadata.radio}\t{fileSize}\t{userMetadata.notes}\t{id}\t{sha256}";
        }
    }
}
