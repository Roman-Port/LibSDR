using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RomanPort.SDRTools.FmFileDemodulator
{
    public abstract partial class LoadingForm : Form
    {
        public LoadingForm(string topic)
        {
            InitializeComponent();
            infoText.Text = topic;
            start = DateTime.UtcNow;
        }

        private Thread worker;
        private DateTime start;
        public bool cancelRequested;

        public abstract object WorkThread();

        public void StatusUpdate(double status)
        {
            Invoke((MethodInvoker)delegate
            {
                status = Math.Max(0, Math.Min(1, status));
                progressBar.Value = (int)(status * 100);

                if (status != 0)
                {
                    TimeSpan remainingTime = new TimeSpan(0, 0, (int)((DateTime.UtcNow - start).TotalSeconds * (1f / status)));
                    remainingCounter.Text = ((remainingTime.Days * 24) + remainingTime.Hours).ToString().PadLeft(2, '0') + ":" + remainingTime.Minutes.ToString().PadLeft(2, '0') + ":" + remainingTime.Seconds.ToString().PadLeft(2, '0') + " remaining";
                }
            });
        }

        public void RunWork(WorkingFormResponse callback)
        {
            //Begin thread
            worker = new Thread(() =>
            {
                //Work
                object response = WorkThread();

                //Respond
                Invoke((MethodInvoker)delegate
                {
                    progressBar.Value = 100;
                    callback(response);
                    Close();
                });
            });
            worker.IsBackground = true;
            worker.Start();

            //Show
            ShowDialog();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            cancelRequested = true;
            btnCancel.Enabled = false;
        }
    }

    public delegate void WorkingFormResponse(object response);
}
