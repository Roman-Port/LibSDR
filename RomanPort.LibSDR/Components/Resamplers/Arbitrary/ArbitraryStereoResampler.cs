using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Resamplers.Arbitrary
{
    public unsafe class ArbitraryStereoResampler : IDisposable
    {
        public ArbitraryStereoResampler(double inSampleRate, double outSampleRate, int bufferSize)
        {
            resamplerA = new ArbitraryFloatResampler(inSampleRate, outSampleRate, bufferSize);
            resamplerB = new ArbitraryFloatResampler(inSampleRate, outSampleRate, bufferSize);
        }

        private ArbitraryFloatResampler resamplerA;
        private ArbitraryFloatResampler resamplerB;

        public void Input(float* audioL, float* audioR, int count)
        {
            resamplerA.Input(audioL, count, 1);
            resamplerB.Input(audioR, count, 1);
        }

        public int Output(float* audioL, float* audioR, int maxCount)
        {
            resamplerA.Output(audioL, maxCount, 1);
            return resamplerB.Output(audioR, maxCount, 1);
        }

        public int Output(float* audio, int maxCount)
        {
            resamplerA.Output(audio, maxCount, 2);
            return resamplerB.Output(audio + 1, maxCount, 2);
        }

        public void Dispose()
        {
            resamplerA.Dispose();
            resamplerB.Dispose();
        }
    }
}
