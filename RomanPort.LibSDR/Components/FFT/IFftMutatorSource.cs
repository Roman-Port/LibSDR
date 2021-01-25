using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.FFT
{
    public interface IFftMutatorSource
    {
        unsafe float* ProcessFFT(out int fftBins);
    }
}
