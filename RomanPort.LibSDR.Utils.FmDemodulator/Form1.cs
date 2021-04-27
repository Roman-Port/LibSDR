using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RomanPort.LibSDR.Utils.FmDemodulator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public string statText;
        public double statProgress;

        private Timer timer;

        private void Form1_Load(object sender, EventArgs e)
        {
            //Create UI timer
            timer = new Timer();
            timer.Interval = 1000 / 60;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            statusText.Text = statText;
            statusBar.Value = (int)(statProgress * 100);
        }

        private void btnControl_Click(object sender, EventArgs e)
        {

        }
    }
}
