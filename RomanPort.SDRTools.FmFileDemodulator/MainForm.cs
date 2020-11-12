using RomanPort.LibSDR.Demodulators;
using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Util;
using RomanPort.LibSDR.Receivers;
using RomanPort.LibSDR.Sources;
using RomanPort.SDRTools.FmFileDemodulator.Workers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RomanPort.SDRTools.FmFileDemodulator
{
    public unsafe partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            buffers = new SdrAppBuffers(BUFFER_SIZE);
        }

        public const int BUFFER_SIZE = 65536*8;

        private SdrAppBuffers buffers;

        private string sourceFilename;
        private WavStreamSource source;
        private float sampleRate;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            //Close existing source, if any
            if (source != null)
                OnFileUnloaded();

            //Open file picker
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Choose IQ File";
            dialog.Filter = "IQ Files (*.wav)|*.wav";
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            //Work
            new OpenFileScanner(dialog.FileName, BUFFER_SIZE).RunWork((object r) =>
            {
                Tuple<WavStreamSource, float> data = (Tuple<WavStreamSource, float>)r;
                sourceFilename = dialog.FileName;
                source = data.Item1;
                sampleRate = data.Item2;
                OnFileLoaded();
            });
        }

        private void OnFileLoaded()
        {
            //Update UI
            inputFilepath.Text = sourceFilename;
            sampleRateStat.Text = sampleRate.ToString();
            btnStart.Enabled = true;
        }

        private void OnFileUnloaded()
        {
            source.Close();
            source.Dispose();
            inputFilepath.Text = "";
            sampleRateStat.Text = "";
            source = null;
            btnStart.Enabled = false;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            //Read settings
            int outSampRate = (int)settingsOutputSampRate.Value;
            float bandwidth = (float)settingDemodBandwidth.Value;

            //Open file picker
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "Choose Output File";
            dialog.Filter = "WAV Files (*.wav)|*.wav";
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            //Open source
            WavReceiver output = new WavReceiver(new FileStream(dialog.FileName, FileMode.Create), 16);
            output.Open(outSampRate, BUFFER_SIZE);

            //Start
            new DemodulatingWorker(source, buffers, output, sampleRate, outSampRate, bandwidth).RunWork((object s) =>
            {
                //Finished. Close output
                output.Dispose();

                //Confirm
                MessageBox.Show("The file has been successfully demodulated.", "Success", MessageBoxButtons.OK);
            });
        }
    }
}
