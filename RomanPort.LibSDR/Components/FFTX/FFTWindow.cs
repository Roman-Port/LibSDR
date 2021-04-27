using RomanPort.LibSDR.Components.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.FFTX
{
    public class FFTWindow
    {
        public FFTWindow(int fftBins, WindowType type = WindowType.HannPoisson)
        {
            this.fftBins = fftBins;
            window = WindowUtil.MakeWindow(type, fftBins);
        }

        private float[] window;
        private int fftBins;

        public unsafe void Apply(Complex* block)
        {
            for(int i = 0; i<fftBins; i++)
            {
                block[i].Real *= window[i];
                block[i].Imag *= window[i];
            }
        }
    }
}
