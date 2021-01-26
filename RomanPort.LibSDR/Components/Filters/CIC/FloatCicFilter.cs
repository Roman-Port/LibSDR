using RomanPort.LibSDR.Components.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters.CIC
{
    public unsafe class FloatCicFilter : IDisposable
    {
        private int R;
        private int N;
        private int M;

        private float[] buffer_integrator;
        private float[][] buffer_comb;
        private int[] offset_comb;

        public FloatCicFilter(int decimationFactor, int numberOfSections = 1, int diferrencialDelay = 1)
        {
            R = decimationFactor;
            N = numberOfSections;
            M = diferrencialDelay;

            buffer_integrator = new float[N];
            buffer_comb = new float[N][];
            offset_comb = new int[N];
            for (int i = 0; i < N; i++)
                buffer_comb[i] = new float[M];
        }

        public void Dispose()
        {
            
        }

        /// <summary>
        /// Processes a block of data of any size.
        /// </summary>
        /// <param name="buffer">The input and the output buffer.</param>
        /// <param name="length">The number of samples to read.</param>
        /// <param name="channelCount">The spacing between each written output. However, will only run on one channel.</param>
        /// <returns>The number of bytes written to the output.</returns>
        public int Process(float* buffer, int length, int channelCount)
        {
            return Process(buffer, buffer, length, channelCount);
        }

        public int Process(float* input, float* output, int length, int channelCount)
        {
            int processed = 0;
            for(int i = 0; i<length; i += R)
            {
                *output = ProcessSingle(input, channelCount);
                input += R * channelCount;
                output += channelCount;
                processed++;
            }
            return processed;
        }

        private float ProcessSingle(float* input, int channelCount)
        {
            float anttenuation = 0.5f;  // the amplitude anttenuation factor, default as 1
            float tmp_out = 0;

            // Integrator part
            for (int i = 0; i < R; i++)
            {
                tmp_out = input[i * channelCount];

                for (int j = 0; j < N; j++)
                    tmp_out = this.buffer_integrator[j] = this.buffer_integrator[j] + tmp_out;
            }

            // Comb part
            for (int i = 0; i < N; i++)
            {
                this.offset_comb[i] = (this.offset_comb[i] + 1) % M;
                float tmp = this.buffer_comb[i][this.offset_comb[i]];
                this.buffer_comb[i][this.offset_comb[i]] = tmp_out;
                tmp_out = tmp_out - tmp;
            }

            return anttenuation * tmp_out;
        }
    }
}