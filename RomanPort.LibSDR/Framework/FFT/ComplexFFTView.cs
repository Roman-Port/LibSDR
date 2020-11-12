using RomanPort.LibSDR.Framework.Extras;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework.FFT
{
    public unsafe class ComplexFftView : FftView
    {
        public ComplexFftView(int fftBins, int averageSampleCount) : base(fftBins, fftBins, averageSampleCount)
        {
            
        }

        public void ProcessSamples(Complex* _iqPtr)
        {
            //Copy
            Utils.Memcpy(_fftPtr, _iqPtr, fftBinsBufferSize * sizeof(Complex));

            //Process
            BaseProcessSamples();
        }
    }
}
