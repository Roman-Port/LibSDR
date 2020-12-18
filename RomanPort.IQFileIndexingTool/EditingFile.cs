using RomanPort.LibSDR;
using RomanPort.LibSDR.Demodulators;
using RomanPort.LibSDR.Extras;
using RomanPort.LibSDR.Extras.RDS;
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
        public EditingFileMetadata metadata;
        public string id;
        public FileInfo fileInfo;
        public string fileSize;
        public ListViewItem item;

        private UserMetadataState userMetadata;
        private bool opened;
        private Form1 form;

        public EditingFile(string originalPathname)
        {
            this.originalPathname = originalPathname;

            //Get file metadata
            metadata = new EditingFileMetadata(originalPathname + ".meta", originalPathname);
            metadata.OnMadeReady += OnMetadataReady;

            userMetadata = new UserMetadataState();
            fileInfo = new FileInfo(originalPathname);
            userMetadata.time = fileInfo.LastWriteTime;
            fileSize = Math.Round((double)fileInfo.Length / 1024 / 1024, 1).ToString() + " MB";

            //Generate ID
            byte[] idBuffer = new byte[4];
            Program.rand.NextBytes(idBuffer);
            id = BitConverter.ToString(idBuffer).Replace("-", "");
        }

        private void OnMetadataReady()
        {
            //Do nothing if not opened
            if (!opened)
                return;
            
            //Load parts
            byte[] hash = metadata.GetSha256();
            float[] waveformData = metadata.GetWaveform();
            
            //Convert hash into a string
            string sha256 = BitConverter.ToString(hash).Replace("-", "");

            //Convert waveform into a bitmap, interpolating between data we might not have
            Bitmap waveformThumb = new Bitmap(form.waveformView.Width, form.waveformView.Height);
            int halfHeight = waveformThumb.Height / 2;
            Graphics gfx = Graphics.FromImage(waveformThumb);
            Pen pen = new Pen(Color.Red, 1);
            float interpScale = EditingFileMetadata.WAVEFORM_RESOLUTION / (float)waveformThumb.Width;
            for (int i = 0; i<waveformThumb.Width; i++)
            {
                float size = waveformData[(int)(i * interpScale)] * halfHeight;
                gfx.DrawLine(pen, i, halfHeight - size, i, halfHeight + size);
            }

            //Update in the interface
            form.Invoke((MethodInvoker)delegate
            {
                if (opened)
                {
                    form.metaHash.Text = sha256;
                    form.waveformView.Image = waveformThumb;
                    UpdateButtonStatus();
                }
            });
        }
        
        public unsafe void OpenEditor(Form1 form)
        {
            //Update state
            opened = true;
            this.form = form;

            //Open radio
            form.ChangeRadioFile(originalPathname);

            //Update track bar
            form.trackProgress.Value = 0;
            form.trackProgress.Maximum = (int)GetLengthSeconds();

            //Set user metadata
            form.userMetadata = userMetadata;
            userMetadata.radio = form.source.fileSampleRate == 900001 ? "RTL-SDR" : "AirSpy";
            form.LoadUserMetadata();

            //Set our metadata
            form.metaSampleRate.Text = form.source.fileSampleRate.ToString();
            form.metaSize.Text = fileSize;
            if(metadata.IsReady())
                OnMetadataReady();
            else
            {
                form.metaHash.Text = "Calculating...";
                form.waveformView.Image = null;
            }

            //Set buttons
            UpdateButtonStatus();
            form.timeUpdateTimer.Start();
        }

        public string GetOutputPath()
        {
            return form.outputDirectory + $"{userMetadata.stationValue} - {EscapePathPart(userMetadata.artistValue)} - {EscapePathPart(userMetadata.titleValue)} - {id}.wav";
        }

        private string EscapePathPart(string p)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (var c in invalidChars)
                p = p.Replace(c, '_');
            return p;
        }

        public void CloseEditor()
        {
            //Update state
            opened = false;
            form.timeUpdateTimer.Stop();

            //Close file
            form.CloseRadioFile();

            //Disable buttons
            form.btnDelete.Enabled = false;
            form.btnTrim.Enabled = false;
            form.btnSave.Enabled = false;
            form.alreadyExistsBadge.Visible = false;
            form.trimDialog?.Close();
        }

        public void Shutdown()
        {
            //Run close thread tasks
            CloseEditor();
        }

        public float GetLengthSeconds()
        {
            return form.source.GetLengthSeconds();
        }

        public float GetPositionSeconds()
        {
            return form.source.GetPositionSeconds();
        }

        public string GetTitle()
        {
            return fileInfo.Name;
        }

        public void JumpToPositionSeconds(float seconds)
        {
            form.source.SafeSkipToSeconds(seconds);
        }

        public void UpdateButtonStatus()
        {
            if (!opened)
                return;
            bool canConfirm = userMetadata.IsFilledOut() && metadata.IsReady();
            form.btnDelete.Enabled = true;
            form.btnSave.Enabled = canConfirm;
            form.btnTrim.Enabled = canConfirm;
            if (!canConfirm && form.trimDialog != null)
                form.trimDialog.Close();
        }

        public string GetCopyString()
        {
            return $"{userMetadata.stationValue}\t{userMetadata.artistValue}\t{userMetadata.titleValue}\t{userMetadata.time.ToShortDateString()}\tUnzipped (ID)\t{userMetadata.prefix}\t{userMetadata.suffix}\t{userMetadata.radio}\t{fileSize}\t{userMetadata.notes}\t{id}\t{BitConverter.ToString(metadata.GetSha256()).Replace("-", "")}";
        }
    }
}
