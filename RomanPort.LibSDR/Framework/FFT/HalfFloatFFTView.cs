using RomanPort.LibSDR.Framework.Extras;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework.FFT
{
    public unsafe class HalfFloatFftView : FftView
    {
        public HalfFloatFftView(int fftBins, int interval, int averageSampleCount) : base(fftBins, fftBins * 2, interval, averageSampleCount)
        {

        }

        public void ProcessSamples(float* _iqPtr)
        {
            //Copy
            for (int i = 0; i < fftBinsBufferSize; i++)
                _fftPtr[i] = new Complex(_iqPtr[i], 0);

            //Process
            BaseProcessSamples();

            //Send events
            BroadcastEvents(fftBins);
        }
    }
}
