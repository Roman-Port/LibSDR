using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters.FIR.Real.Implementations
{
    public unsafe class RealFirFilterGeneric : IRealFirFilter
    {
        private float* coeffsBufferPtr;
        private float* insampBufferPtr;
        private float* insampBufferPtrOffset;

        private UnsafeBuffer coeffsBuffer;
        private UnsafeBuffer insampBuffer;

        private int taps;
        private int decimation;
        private int decimationIndex;
        private int offset;

        internal RealFirFilterGeneric(float[] coeffs, int decimation)
        {
            VolkApi.WarnVolk();
            coeffsBuffer = UnsafeBuffer.Create(coeffs.Length, out coeffsBufferPtr);
            insampBuffer = UnsafeBuffer.Create(coeffs.Length * 2, out insampBufferPtr);
            insampBufferPtrOffset = insampBufferPtr + coeffs.Length;
            taps = coeffs.Length;
            this.decimation = decimation;
            decimationIndex = 0;
            offset = 0;
            for (int i = 0; i < coeffs.Length; i++)
                coeffsBufferPtr[i] = coeffs[i];
        }

        public int Process(float* input, float* output, int count, int channels = 1)
        {
            int read = 0;
            float result;
            for (int i = 0; i < count; i++)
            {
                //Write to both the real position as well as an offset value
                this.insampBufferPtr[this.offset] = *input;
                this.insampBufferPtrOffset[this.offset++] = *input;
                input += channels;
                this.offset %= this.taps;

                //Process (if needed)
                if (this.decimationIndex++ == 0)
                {
                    result = 0;
                    float* aPtr = &this.insampBufferPtr[this.offset];
                    float* bPtr = this.coeffsBufferPtr;
                    for (int j = 0; j < this.taps; j++)
                    {
                        result += ((*aPtr++) * (*bPtr++));
                    }
                    output[read++ * channels] = result;
                }
                this.decimationIndex %= this.decimation;
            }
            return read;
        }

        public int Process(float* ptr, int count, int channels = 1)
        {
            return Process(ptr, ptr, count, channels);
        }

        public void Dispose()
        {
            coeffsBuffer.Dispose();
            insampBuffer.Dispose();
        }
    }
}
