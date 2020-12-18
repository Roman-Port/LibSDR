using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework.Components.FFT.Processors
{
    public class FFTProcessorHalfFloat : FFTProcessorFloat
    {
        public FFTProcessorHalfFloat(int fftBins) : base(fftBins, 2)
        {
        }
    }
}
