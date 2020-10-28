using NAudio.Wave;
using RomanPort.LibSDR;
using RomanPort.LibSDR.Demodulators;
using RomanPort.LibSDR.Extras;
using RomanPort.LibSDR.Extras.RDS;
using RomanPort.LibSDR.Extras.RDS.Features;
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

        public EditingFile activeFile;
        public UserMetadataState userMetadata = new UserMetadataState();

        public string inputDirectory = @"C:\Users\Roman\Desktop\IQ\";
        public string outputDirectory = @"C:\Users\Roman\Desktop\IQ\Output\";
        
        public Form1()
        {
            InitializeComponent();
            sliderIfGain.Value = (int)(defaultIfGain * 100);
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
                var file = new EditingFile(f);
                var entry = new ListViewItem(new string[] { file.fileInfo.Name, file.fileSize });
                file.item = entry;
                entry.Tag = file;
                fileList.Items.Add(entry);
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
            timeUpdateTimer = new System.Timers.Timer(100);
            timeUpdateTimer.AutoReset = true;
            timeUpdateTimer.Elapsed += TimeUpdateTimer_Elapsed;

            //Open
            activeFile.OpenEditor(this);
        }

        private void TimeUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Get times
            TimeSpan length = new TimeSpan(0, 0, (int)activeFile.GetLengthSeconds());
            TimeSpan position = new TimeSpan(0, 0, (int)activeFile.GetPositionSeconds());

            //Set text
            Invoke((MethodInvoker)delegate
            {
                if(!suspendPlaybarUpdates)
                    trackProgress.Value = (int)activeFile.GetPositionSeconds();
                timerText.Text = $"{position.Minutes.ToString().PadLeft(2, '0')}:{position.Seconds.ToString().PadLeft(2, '0')} / {length.Minutes.ToString().PadLeft(2, '0')}:{length.Seconds.ToString().PadLeft(2, '0')}";
                rdsRadioText.Text = new string(activeFile.rds.featureRadioText.textBuffer);
                rdsPsName.Text = activeFile.rds.featureStationName.stationName;
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
            //Check if this exists
            if(File.Exists(activeFile.savedPathname))
            {
                if (MessageBox.Show("The file at \"" + activeFile.savedPathname + "\" already exists. Is it OK to overwrite it?", "Can't Save", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    File.Delete(activeFile.savedPathname);
                else
                    return;
            }
            
            //Shut down radio
            activeFile.Shutdown();

            //Copy database entry to clipboard
            Clipboard.SetText(activeFile.GetCopyString());

            //Move file
            File.Move(activeFile.originalPathname, activeFile.savedPathname);

            //Remove from list
            fileList.Items.Remove(activeFile.item);

            //Confirm
            MessageBox.Show("File moved, database entry copied to clipboard.", activeFile.GetTitle(), MessageBoxButtons.OK);

            //Close if needed
            if(fileList.Items.Count == 0)
            {
                MessageBox.Show("No more input files. Closing...", "Finished", MessageBoxButtons.OK);
                Close();
                return;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            //Prompt
            if (MessageBox.Show("Is it okay to delete this file?", activeFile.GetTitle(), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            //Shut down radio
            activeFile.Shutdown();

            //Delete
            File.Delete(activeFile.originalPathname);

            //Remove from list
            fileList.Items.Remove(activeFile.item);

            //Close if needed
            if (fileList.Items.Count == 0)
            {
                MessageBox.Show("No more input files. Closing...", "Finished", MessageBoxButtons.OK);
                Close();
                return;
            }
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
            activeFile.demodulator.ifGain = ((float)sliderIfGain.Value) / 100;
            defaultIfGain = ((float)sliderIfGain.Value) / 100;
        }
    }
}
