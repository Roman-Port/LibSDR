using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework.Components.Filters
{
    public unsafe class FloatCicFilter
    {
        private int R;
        private int N;
        private int M;
        private UnsafeBuffer bufferIntegrator;
        private float* bufferIntegratorPtr;
        private UnsafeBuffer bufferComb;
        private float* bufferCombPtr;
        private UnsafeBuffer bufferOffsetComb;
        private int* bufferOffsetCombPtr;

        private UnsafeBuffer bufferLeftover;
        private float* bufferLeftoverPtr;
        private int leftover;

        public FloatCicFilter(int decimationFactor, int numberOfSections = 1, int diferrencialDelay = 1)
        {
            R = decimationFactor;
            N = numberOfSections;
            M = diferrencialDelay;

            bufferIntegrator = UnsafeBuffer.Create(N, sizeof(float));
            bufferIntegratorPtr = (float*)bufferIntegrator;
            bufferComb = UnsafeBuffer.Create(N, sizeof(float) * N); //actually a 2D array
            bufferCombPtr = (float*)bufferComb;
            bufferOffsetComb = UnsafeBuffer.Create(N, sizeof(int));
            bufferOffsetCombPtr = (int*)bufferOffsetComb;
            bufferLeftover = UnsafeBuffer.Create(R, sizeof(float));
            bufferLeftoverPtr = (float*)bufferLeftover;
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
            //Allocate initial vars
            int written = 0;
            float* input = buffer;
            float* output = buffer;

            //If there is something in the leftovers, we want to get that scored away before we continue
            //Since this is a continuous stream, we need to 
            if (leftover > 0)
            {
                //"Move" the bytes that we can into this leftovers array by copying and changing our pointer/length
                while (leftover < R && length > 0)
                {
                    bufferLeftoverPtr[leftover] = input[0];
                    input += channelCount;
                    length--;
                    leftover++;
                }

                //IF we don't have enough to process STILL (very unlikely!), then just process nothing
                if (leftover < R)
                    return 0;

                //Process what is currently in the leftovers
                //It is safe to reuse the same buffer because we would've read the first entry (that we are overwriting) regardless, because at least one copy operation would've been done above
                output[0] = ProcessSingle(bufferLeftoverPtr, 1);
                output += channelCount;
                written++;

                //Reset leftovers
                leftover = 0;
            }

            //Process
            for (int c = 0; c < length / R; c++)
            {
                output[0] = ProcessSingle(input, channelCount);
                input += channelCount * R;
                output += channelCount;
                written++;
            }

            //Get leftover samples that we don't have enough of to compute
            int currentLeftover = length % R;

            //Copy this to the leftovers. We're guarenteed we will have space because leftovers will always = 0 by the time we get to this point
            while (currentLeftover > 0)
            {
                bufferLeftoverPtr[leftover] = input[0];
                input += channelCount;
                leftover++;
                currentLeftover--;
            }

            return written;
        }

        private float ProcessSingle(float* input, int channelCount)
        {
            float anttenuation = 1.0f;
            float b = 0;
            for (int i = 0; i < R; i++)
            {
                b = input[0];
                input += channelCount;

                for (int j = 0; j < N; j++)
                    b = bufferIntegratorPtr[j] = bufferIntegratorPtr[j] + b;
            }
            for (int i = 0; i < N; i++)
            {
                bufferOffsetCombPtr[i] = (bufferOffsetCombPtr[i] + 1) % M;
                float tmp = bufferCombPtr[(i * N) + bufferOffsetCombPtr[i]];
                bufferCombPtr[(i * N) + bufferOffsetCombPtr[i]] = b;
                b = b - tmp;
            }
            return anttenuation * b;
        }
    }
}
