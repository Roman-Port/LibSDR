using RomanPort.LibSDR.Components.FFT;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RomanPort.LibSDR.UI
{
    public partial class FFTDisplay : UserControl
    {
        public FFTDisplay()
        {
            InitializeComponent();
        }

        public float FftMinDb
        {
            get => mainFft.FftMinDb;
            set
            {
                mainFft.FftMinDb = value;
                waterfall.FftMinDb = value;
            }
        }

        public float FftMaxDb
        {
            get => mainFft.FftMaxDb;
            set
            {
                mainFft.FftMaxDb = value;
                waterfall.FftMaxDb = value;
            }
        }

        public void ConfigureFFT(IFftMutatorSource spectrum, IFftMutatorSource waterfall)
        {
            this.mainFft.SetFFT(spectrum);
            this.waterfall.SetFFT(waterfall);
        }

        public void RefreshFFT()
        {
            this.mainFft.RefreshFFT();
            this.waterfall.RefreshFFT();
        }
    }
}
