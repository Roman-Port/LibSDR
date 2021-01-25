using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.FFT.Processors
{
    public class FFTProcessorHalfFloat : FFTProcessorFloat
    {
        public FFTProcessorHalfFloat(int fftBins) : base(fftBins, 2)
        {
        }
    }
}
