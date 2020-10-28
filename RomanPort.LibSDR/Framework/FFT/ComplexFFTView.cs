using RomanPort.LibSDR.Framework.Extras;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework.FFT
{
    public unsafe class ComplexFftView : FftView
    {
        public ComplexFftView(int fftBins, int interval, int averageSampleCount) : base(fftBins, fftBins, interval, averageSampleCount)
        {
            
        }

        public void ProcessSamples(Complex* _iqPtr)
        {
            //Copy
            Utils.Memcpy(_fftPtr, _iqPtr, fftBinsBufferSize * sizeof(Complex));

            //Process
            BaseProcessSamples();

            //Send events
            BroadcastEvents(fftBins);
        }
    }
}
