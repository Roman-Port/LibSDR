using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.FFT.Mutators
{
    public unsafe class FFTResizer : IFftMutatorSource
    {
        public FFTResizer(IFftMutatorSource source, int outputSize)
        {
            this.source = source;
            this.outputSize = outputSize;
            Configure();
        }

        private IFftMutatorSource source;
        private int outputSize;

        private UnsafeBuffer buffer;
        private float* bufferPtr;

        public int OutputSize
        {
            get => outputSize;
            set
            {
                outputSize = Math.Max(1, value);
                Configure();
            }
        }

        private void Configure()
        {
            //Dispose of old buffer
            buffer?.Dispose();

            //Make buffer
            buffer = UnsafeBuffer.Create(outputSize, out bufferPtr);
        }

        public float* ProcessFFT(out int fftBins)
        {
            //Get pointer from source
            float* src = source.ProcessFFT(out int srcFftBins);

            //Apply
            ResizeFFT(src, srcFftBins, bufferPtr, OutputSize);

            //Output
            fftBins = outputSize;
            return bufferPtr;
        }

        public static void ResizeFFT(float* input, int inputSize, float* output, int outputSize)
        {
            //Get the scaling factor
            float scale = (float)inputSize / outputSize;

            //Determine how to scale
            if (scale == 1)
            {
                //That was easy. These are the same dimensions
                Utils.Memcpy(output, input, outputSize * sizeof(float));
            }
            else if (scale > 1)
            {
                //More input bins than output bins. This is the most common one
                int lastIndex = -1;
                for (int i = 0; i < inputSize; i++)
                {
                    int outIndex = (int)(i / scale);
                    if (lastIndex < outIndex)
                        output[outIndex] = input[i]; //This is the first one, so just set it to avoid using old data
                    else
                        output[outIndex] = Math.Max(output[outIndex], input[i]);
                    lastIndex = outIndex;
                }
            }
            else
            {
                //More output bins than input bins. Interpolate
                for (int i = 0; i < outputSize; i++)
                    output[i] = input[(int)(i * scale)];
            }
        }
    }
}
