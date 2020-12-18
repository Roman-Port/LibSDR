using RomanPort.LibSDR.Framework.Components.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework.Resamplers.Decimators
{
    public unsafe class SdrFloatDecimator : IDisposable
    {
        public SdrFloatDecimator(int decimation, int incomingChannels, int channelIndex)
        {
            this.decimation = decimation;
            this.incomingChannels = incomingChannels;
            this.channelIndex = channelIndex;
            cic = new FloatCicFilter(decimation);
        }

        public readonly int decimation;
        public readonly int incomingChannels; //The number of channels in the stream. While this can only compute one channel, this allows us to skip others
        public readonly int channelIndex;

        private FloatCicFilter cic;

        public int Process(float* inBuffer, int inCount, float* outBuffer, int outBufferLen)
        {
            return cic.Process(inBuffer + channelIndex, inCount, incomingChannels);
        }

        public static int CalculateDecimationRate(float inputSampleRate, float desiredOutputSampleRate, out float actualOutputSampleRate)
        {
            //Calculate the rate by finding the LOWEST we can go without it becoming a rate lower than the desired rate
            int decimationRate = 1;
            do
            {
                decimationRate++;
            } while (inputSampleRate / (decimationRate + 1) >= desiredOutputSampleRate);

            //Determine the actual output sample rate
            actualOutputSampleRate = inputSampleRate / decimationRate;

            return decimationRate;
        }

        public void Dispose()
        {
            
        }
    }
}
