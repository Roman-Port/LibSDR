using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;
using RomanPort.LibSDR.Framework;

namespace RomanPort.LibSDR.Components.FFT.Processors
{
    public unsafe class FFTProcessorComplex : FFTProcessorBase<Complex>
    {
        public FFTProcessorComplex(int fftBins) : base(fftBins, 1)
        {

        }

        protected override unsafe void CopyIncomingSamples(Complex* ptr, Complex* dest)
        {
            Utils.Memcpy(dest, ptr, FftBins * sizeof(Complex));
        }
    }
}
