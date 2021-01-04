using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework.Components.Resamplers
{
    public class FloatInterpolator
    {
        public FloatInterpolator(int interpolationRate, int channelCount)
        {
            this.interpolationRate = interpolationRate;
            this.channelCount = channelCount;
        }

        private int interpolationRate;
        private int channelCount;

        public static int CalculateInterpolationRate(float inputSampleRate, float desiredSampleRate, out float actualSampleRate)
        {
            int rate = 1;
            while (inputSampleRate * rate < desiredSampleRate)
                rate++;
            actualSampleRate = inputSampleRate * rate;
            return rate;
        }

        public unsafe int Process(float* input, float* output, int count)
        {
            if (input == output)
                throw new Exception("Input and output buffers cannot match!");
            for(int i = 0; i<count; i++)
            {
                for(int j = 0; j<interpolationRate; j++)
                {
                    output[0] = input[0];
                    output += channelCount;
                }
                input += channelCount;
            }
            return count * interpolationRate;
        }
    }
}
