using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.FFTX.Kiss
{
    public unsafe class KissFFTComplex : KissFFT
    {
        public KissFFTComplex(int nfft, bool inverse) : base(nfft, inverse)
        {

        }

        public void Process(Complex* input, Complex* output, int stride = 1)
        {
            kf_work(output, input, 1, stride, factors);
        }
    }
}
