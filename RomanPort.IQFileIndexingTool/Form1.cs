using NAudio.Wave;
using RomanPort.IQFileIndexingTool.Commands;
using RomanPort.LibSDR;
using RomanPort.LibSDR.Demodulators;
using RomanPort.LibSDR.Extras;
using RomanPort.LibSDR.Extras.RDS;
using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Extras;
using RomanPort.LibSDR.Framework.FFT;
using RomanPort.LibSDR.Framework.Resamplers.Decimators;
using RomanPort.LibSDR.Framework.Util;
using RomanPort.LibSDR.Sources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace RomanPort.IQFileIndexingTool
{
    public unsafe partial class Form1 : Form
    {
        public float audioGain = 1;
        public System.Timers.Timer timeUpdateTimer;

        public static float defaultIfGain = FmDemodulator.FM_GAIN;

        private bool suspendPlaybarUpdates;
        private Queue<IWorkerCommand> commandQueue = new Queue<IWorkerCommand>();
        private List<EditingFile> files = new List<EditingFile>();

        public EditingFile activeFile;
        public UserMetadataState userMetadata = new UserMetadataState();
        public FileStream copyTextFile;
        public TrimForm trimDialog;

        public string inputDirectory = @"C:\Users\Roman\Desktop\IQ\";
        public string outputDirectory = @"C:\Users\Roman\Desktop\IQ\Output\";

        public Thread radioWorker;
        public Thread backgroundWorker;

        //Radio

        public WavStreamSource source;
        public WbFmDemodulator demodulator;
        public SdrComplexDecimator decimator;
        public IQFirFilter filter;
        public FloatArbResampler resamplerA;
        public FloatArbResampler resamplerB;
        public FFPlayAudio audio;
        public ComplexFftView basebandFft;
        public HalfFloatFftView mpxFft;
        public Throttle throttle;
        public volatile bool radioPlayingRequested = false;
        public volatile bool radioPlaying = false;

        public const int BUFFER_SIZE = 16384;
        public const int FFT_SIZE = 2048;

        public UnsafeBuffer iqBuffer;
        public Complex* iqBufferPtr;
        public UnsafeBuffer audioBufferA;
        public float* audioBufferAPtr;
        public UnsafeBuffer audioBufferB;
        public float* audioBufferBPtr;
        public UnsafeBuffer audioBufferC;
        public float* audioBufferCPtr;
        public UnsafeBuffer fftBuffer;
        public float* fftBufferPtr;

        public Form1()
        {
            InitializeComponent();
            sliderIfGain.Value = (int)(defaultIfGain * 100);

            //Create radio buffers
            int size = BUFFER_SIZE;
            iqBuffer = UnsafeBuffer.Create(size, sizeof(Complex));
            iqBufferPtr = (Complex*)iqBuffer;
            audioBufferA = UnsafeBuffer.Create(size, sizeof(float));
            audioBufferAPtr = (float*)audioBufferA;
            audioBufferB = UnsafeBuffer.Create(size, sizeof(float));
            audioBufferBPtr = (float*)audioBufferB;
            audioBufferC = UnsafeBuffer.Create(size, sizeof(float));
            audioBufferCPtr = (float*)audioBufferC;
            fftBuffer = UnsafeBuffer.Create(FFT_SIZE, sizeof(float));
            fftBufferPtr = (float*)fftBuffer;

            //Create demodulator
            demodulator = new WbFmDemodulator();
            demodulator.OnAttached(BUFFER_SIZE);
            demodulator.UseRds().OnPiCodeUpdated += Form1_OnPiCodeUpdated;
            demodulator.UseRds().OnRtTextUpdated += Form1_OnRtTextUpdated;

            //Create filter
            filter = new IQFirFilter(new float[0]);

            //Make audio
            audio = new FFPlayAudio(48000, 2);

            //Make FFTs
            basebandFft = new ComplexFftView(FFT_SIZE, 20);
            mpxFft = demodulator.EnableMpxFFT(FFT_SIZE, 20);

            //Open the copy text file. This contains all the entries to be copied in later
            copyTextFile = new FileStream(outputDirectory + "index.txt", FileMode.OpenOrCreate);
            copyTextFile.Position = copyTextFile.Length;

            //Open radio worker
            radioWorker = new Thread(RadioWorkThread);
            radioWorker.Name = "Radio Worker";
            radioWorker.IsBackground = true;
            radioWorker.Priority = ThreadPriority.Highest;
            radioWorker.Start();

            //Open background worker
            backgroundWorker = new Thread(BackgroundWorkThread);
            backgroundWorker.Name = "Background Worker";
            backgroundWorker.IsBackground = true;
            backgroundWorker.Start();
        }

        public void ChangeRadioFile(string path)
        {
            //Request stop
            radioPlayingRequested = false;
            while (radioPlaying) ;

            //Create new source
            source = new WavStreamSource(new FileStream(path, FileMode.Open, FileAccess.Read));
            float inputAudioRate = source.Open(BUFFER_SIZE);

            //Create decimator
            int decimationFactor = SdrFloatDecimator.CalculateDecimationRate(inputAudioRate, 250000, out float demodulationSampleRate);
            decimator = new SdrComplexDecimator(decimationFactor);

            //Update demodulator
            decimationFactor = SdrFloatDecimator.CalculateDecimationRate(demodulationSampleRate, 48000, out float audioDecimatedRate);
            demodulator.OnOutputDecimationChanged(decimationFactor);
            demodulator.OnInputSampleRateChanged(demodulationSampleRate);
            demodulator.UseRds().Reset();

            //Update filter
            filter.SetCoefficients(FilterBuilder.MakeBandPassKernel(inputAudioRate, 250, 0, (int)(250000 / 2), WindowType.BlackmanHarris4));

            //Create resamplers
            resamplerA = new FloatArbResampler(audioDecimatedRate, 48000, 1, 0);
            resamplerB = new FloatArbResampler(audioDecimatedRate, 48000, 1, 0);

            //Create throttle
            throttle = new Throttle(inputAudioRate, (-0.20f * inputAudioRate));

            //Allow player to begin
            radioPlayingRequested = true;
        }

        public void CloseRadioFile()
        {
            //Request stop
            radioPlayingRequested = false;
            while (radioPlaying) ;

            //Close file
            source.Close();
            source.Dispose();

            //Close trimmer
            trimDialog?.Close();
        }

        private void BackgroundWorkThread()
        {
            int metadataProcessingIndex = 0;
            while(true)
            {
                //Attempt to dequeue a command
                if (commandQueue.Count > 0)
                    commandQueue.Dequeue().ThreadedWork();

                //Process metadata
                if(metadataProcessingIndex < files.Count)
                {
                    //Make sure that this file is still around
                    if (fileList.Items.Contains(files[metadataProcessingIndex].item))
                    {
                        //Process
                        if (!files[metadataProcessingIndex].metadata.IsReady())
                            files[metadataProcessingIndex].metadata.GenerateAndSave();

                        //Update
                        Invoke((MethodInvoker)delegate
                        {
                            files[metadataProcessingIndex].item.BackColor = fileList.BackColor;
                        });
                    }

                    //Update index
                    metadataProcessingIndex++;
                }

                //Wait a moment
                Thread.Sleep(50);
            }
        }

        private void RadioWorkThread()
        {
            //Run
            int read;
            while(true)
            {
                //Wait
                while (!radioPlayingRequested)
                    radioPlaying = radioPlayingRequested;

                //Read next
                read = source.Read(iqBufferPtr, BUFFER_SIZE);

                //Write FFT
                for(int i = 0; i < 3; i++)
                    basebandFft.ProcessSamples(iqBufferPtr + (i * FFT_SIZE));

                //Filter
                filter.Process(iqBufferPtr, read);

                //Decimate
                int decimatedRead = decimator.Process(iqBufferPtr, read, iqBufferPtr, BUFFER_SIZE);

                //Demodulate
                int audioRead = demodulator.DemodulateStereo(iqBufferPtr, audioBufferAPtr, audioBufferBPtr, decimatedRead);

                //Resample A -> C (left)
                int resampledRead = resamplerA.Process(audioBufferAPtr, audioRead, audioBufferCPtr, BUFFER_SIZE, read != BUFFER_SIZE);

                //Resample B -> A (right)
                resamplerB.Process(audioBufferBPtr, audioRead, audioBufferAPtr, BUFFER_SIZE, read != BUFFER_SIZE);

                //Write
                for(int i = 0; i<resampledRead; i++)
                {
                    audio.WriteAudioSample(audioBufferCPtr[i]);
                    audio.WriteAudioSample(audioBufferAPtr[i]);
                }

                //Wait
                throttle.Work(read);
            }
        }

        public void LoadUserMetadata()
        {
            metaCall.Text = userMetadata.stationValue;
            metaArtist.Text = userMetadata.artistValue;
            metaTitle.Text = userMetadata.titleValue;
            metaRadio.Text = userMetadata.radio;
            metaPrefix.Text = userMetadata.prefix;
            metaSuffix.Text = userMetadata.suffix;
            metaNotes.Text = userMetadata.notes;
            metaTime.Value = userMetadata.time;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Find input files
            string[] inputFiles = Directory.GetFiles(inputDirectory);
            fileList.BeginUpdate();
            foreach(var f in inputFiles)
            {
                if (f.EndsWith(".wav"))
                {
                    var file = new EditingFile(f);
                    var entry = new ListViewItem(new string[] { file.fileInfo.Name, file.fileSize });
                    if(!file.metadata.IsReady())
                        entry.BackColor = Color.Gray;
                    file.item = entry;
                    entry.Tag = file;
                    fileList.Items.Add(entry);
                    files.Add(file);
                }
            }
            fileList.EndUpdate();

            //Make sure we have a file
            if(fileList.Items.Count == 0)
            {
                MessageBox.Show("There are currently no input files. Please add some files and restart the program.", "Can't Start", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            } else
            {
                fileList.Items[0].Selected = true;
                activeFile = (EditingFile)fileList.Items[0].Tag;
            }

            //Make UI update timer
            timeUpdateTimer = new System.Timers.Timer(30);
            timeUpdateTimer.AutoReset = true;
            timeUpdateTimer.Elapsed += TimeUpdateTimer_Elapsed;

            //Open
            activeFile.OpenEditor(this);
        }

        private void TimeUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Skip if no player
            if (!radioPlayingRequested)
                return;
            
            //Get times
            TimeSpan length = new TimeSpan(0, 0, (int)activeFile.GetLengthSeconds());
            TimeSpan position = new TimeSpan(0, 0, (int)activeFile.GetPositionSeconds());

            //Set text
            Invoke((MethodInvoker)delegate
            {
                if(!suspendPlaybarUpdates)
                    trackProgress.Value = (int)activeFile.GetPositionSeconds();
                timerText.Text = $"{position.Minutes.ToString().PadLeft(2, '0')}:{position.Seconds.ToString().PadLeft(2, '0')} / {length.Minutes.ToString().PadLeft(2, '0')}:{length.Seconds.ToString().PadLeft(2, '0')}";
                rdsRadioText.Text = new string(demodulator.UseRds().rtBuffer);
                rdsPsName.Text = demodulator.UseRds().psName;
            });

            //Do FFTs
            basebandFft.GetFFTSnapshot(fftBufferPtr);
            fftSpectrumView1.UpdateFFT(fftBufferPtr, FFT_SIZE);
            mpxFft.GetFFTSnapshot(fftBufferPtr);
            mpxSpectrumView.UpdateFFT(fftBufferPtr, FFT_SIZE);
        }

        private void Form1_OnRtTextUpdated(RDSClient client, string text)
        {
            if (RDSClient.TryGetTrackInfo(text, out string trackTitle, out string trackArtist, out string stationName))
            {
                Invoke((MethodInvoker)delegate
                {
                    metaArtist.Text = trackArtist;
                    metaTitle.Text = trackTitle;
                });
            }
        }

        private void Form1_OnPiCodeUpdated(RDSClient client, ushort pi)
        {
            Invoke((MethodInvoker)delegate
            {
                if (RDSClient.TryGetCallsign(pi, out string callsign))
                    metaCall.Text = callsign + "-FM";
            });
        }

        private void trackProgress_Scroll(object sender, EventArgs e)
        {
            activeFile.JumpToPositionSeconds(trackProgress.Value);
        }

        private void trackProgress_MouseDown(object sender, MouseEventArgs e)
        {
            suspendPlaybarUpdates = true;
        }

        private void trackProgress_MouseUp(object sender, MouseEventArgs e)
        {
            suspendPlaybarUpdates = false;
        }

        private void metaCall_TextChanged(object sender, EventArgs e)
        {
            userMetadata.stationEdited = true;
            userMetadata.stationValue = metaCall.Text;
            activeFile.UpdateButtonStatus();
        }

        private void metaArtist_TextChanged(object sender, EventArgs e)
        {
            userMetadata.artistEdited = true;
            userMetadata.artistValue = metaArtist.Text;
            activeFile.UpdateButtonStatus();
        }

        private void metaTitle_TextChanged(object sender, EventArgs e)
        {
            userMetadata.titleEdited = true;
            userMetadata.titleValue = metaTitle.Text;
            activeFile.UpdateButtonStatus();
        }

        private void metaPrefix_SelectedIndexChanged(object sender, EventArgs e)
        {
            userMetadata.prefix = metaPrefix.Text;
            activeFile.UpdateButtonStatus();
        }

        private void metaSuffix_SelectedIndexChanged(object sender, EventArgs e)
        {
            userMetadata.suffix = metaSuffix.Text;
            activeFile.UpdateButtonStatus();
        }

        private void metaNotes_TextChanged(object sender, EventArgs e)
        {
            userMetadata.notes = metaNotes.Text;
            activeFile.UpdateButtonStatus();
        }

        private void metaRadio_SelectedIndexChanged(object sender, EventArgs e)
        {
            userMetadata.radio = metaRadio.Text;
            activeFile.UpdateButtonStatus();
        }

        private void metaTime_ValueChanged(object sender, EventArgs e)
        {
            userMetadata.time = metaTime.Value;
            activeFile.UpdateButtonStatus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //Get output pathname and copy
            string outputPath = activeFile.GetOutputPath();
            string outputCopy = activeFile.GetCopyString();

            //Check if this exists
            if (File.Exists(outputPath))
            {
                if (MessageBox.Show("The file at \"" + outputPath + "\" already exists. Is it OK to overwrite it?", "Can't Save", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    File.Delete(outputPath);
                else
                    return;
            }
            
            //Shut down radio
            activeFile.Shutdown();
            activeFile.metadata.Delete();

            //Add command
            commandQueue.Enqueue(new SaveFileCommand(this, activeFile.originalPathname, outputPath, outputCopy));

            //Remove from list
            fileList.Items.Remove(activeFile.item);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            //Prompt
            if (MessageBox.Show("Is it okay to delete this file?", activeFile.GetTitle(), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            //Shut down radio
            activeFile.Shutdown();
            activeFile.metadata.Delete();

            //Add command
            commandQueue.Enqueue(new DeleteFileCommand(activeFile.originalPathname));

            //Remove from list
            fileList.Items.Remove(activeFile.item);
        }

        private void btnTrim_Click(object sender, EventArgs e)
        {
            if (trimDialog == null)
            {
                trimDialog = new TrimForm(source.fileSampleRate, source.totalSamples, this);
            }
            trimDialog.Show();
        }

        public void ConfirmTrim(long startSample, long endSample)
        {
            //Get output pathname and copy
            string outputPath = activeFile.GetOutputPath();
            string outputCopy = activeFile.GetCopyString();

            //Check if this exists
            if (File.Exists(outputPath))
            {
                if (MessageBox.Show("The file at \"" + outputPath + "\" already exists. Is it OK to overwrite it?", "Can't Save", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    File.Delete(outputPath);
                else
                    return;
            }

            //Shut down radio
            activeFile.Shutdown();
            activeFile.metadata.Delete();

            //Add command
            commandQueue.Enqueue(new SaveTrimFileCommand(this, activeFile.originalPathname, outputPath, outputCopy, startSample, endSample));

            //Remove from list
            fileList.Items.Remove(activeFile.item);
        }

        private void fileList_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Ensure we're ready
            if (activeFile == null || fileList.SelectedIndices.Count != 1)
                return;
            
            //Deactivate other
            activeFile.CloseEditor();

            //Switch
            activeFile = (EditingFile)fileList.Items[fileList.SelectedIndices[0]].Tag;
            activeFile.OpenEditor(this);
        }

        private void sliderPlaybackVol_Scroll(object sender, EventArgs e)
        {
            audioGain = ((float)sliderPlaybackVol.Value) / 100;
        }

        private void sliderIfGain_Scroll(object sender, EventArgs e)
        {
            demodulator.ifGain = ((float)sliderIfGain.Value) / 100;
            defaultIfGain = ((float)sliderIfGain.Value) / 100;
        }
    }
}
