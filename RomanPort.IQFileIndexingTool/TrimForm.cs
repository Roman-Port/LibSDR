using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RomanPort.IQFileIndexingTool
{
    public partial class TrimForm : Form
    {
        private int sampleRate;
        private long samplesCount;
        private Form1 context;
        private long startSample;
        private long endSample;
        
        public TrimForm(int sampleRate, long samplesCount, Form1 context)
        {
            InitializeComponent();
            this.sampleRate = sampleRate;
            this.samplesCount = samplesCount;
            this.context = context;
        }

        private void btnMarkStart_Click(object sender, EventArgs e)
        {
            startSample = context.source.SamplePosition;
            startTime.Text = GetTimestampFromSeconds((int)context.source.GetPositionSeconds());
            btnSave.Enabled = startSample < endSample;
        }

        private void btnMarkEnd_Click(object sender, EventArgs e)
        {
            endSample = context.source.SamplePosition;
            endTime.Text = GetTimestampFromSeconds((int)context.source.GetPositionSeconds());
            btnSave.Enabled = startSample < endSample;
        }

        private string GetTimestampFromSeconds(int totalSeconds)
        {
            int mins = totalSeconds / 60;
            int secs = totalSeconds % 60;
            return mins.ToString().PadLeft(2, '0') + ":" + secs.ToString().PadLeft(2, '0');
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Close();
            context.ConfirmTrim(startSample, endSample);
        }

        private void TrimForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            context.trimDialog = null;
        }
    }
}
