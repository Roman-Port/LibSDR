using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework
{
    public unsafe class FloatMultichannelArbResampler
    {
        public readonly double resamplingRate;
        public readonly int incomingChannels;

        private FloatArbResampler[] channels;

        public FloatMultichannelArbResampler(double resamplingRate, int incomingChannels)
        {
            this.resamplingRate = resamplingRate;
            this.incomingChannels = incomingChannels;
            Init();
        }

        public FloatMultichannelArbResampler(double inSampleRate, double outSampleRate, int incomingChannels)
        {
            resamplingRate = outSampleRate / inSampleRate;
            this.incomingChannels = incomingChannels;
            Init();
        }

        private void Init()
        {
            //Create channels
            channels = new FloatArbResampler[incomingChannels];
            for (int i = 0; i < incomingChannels; i++)
            {
                channels[i] = new FloatArbResampler(resamplingRate, incomingChannels, i);
            }
        }

        public int Process(float* inBuffer, int inBufferLen, float* outBuffer, int outBufferLen, bool lastBatch)
        {
            int total = 0;
            int computedEach = -1;
            for (int i = 0; i < incomingChannels; i++)
            {
                //Process
                int computed = channels[i].Process(inBuffer, inBufferLen, outBuffer, outBufferLen, lastBatch);

                //Make sure we output the same number of samples for each
                if (computedEach == -1)
                    computedEach = computed;
                else if (computed != computedEach)
                    throw new Exception($"Channel {i} did not output the same number of samples as previous channels did while resampling. Got {computed} samples, expected {computedEach}!");

                //Add to total
                total += computed;
            }
            return total;
        }
    }
}
