using RomanPort.LibSDR.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.FFT.Processors
{
    public unsafe class FFTProcessorFloat : FFTProcessorBase<float>
    {
        public FFTProcessorFloat(int fftBins) : base(fftBins, 1)
        {
        }

        protected FFTProcessorFloat(int fftBins, int bufferSizeMultiplier) : base(fftBins, bufferSizeMultiplier)
        {
        }

        protected override unsafe void CopyIncomingSamples(float* ptr, Complex* dest)
        {
            for (int i = 0; i < FftBufferSize; i++)
                dest[i] = new Complex(ptr[i], 0);
        }
    }
}
