using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters.FIR.ComplexFilter.Implementations
{
    public unsafe class ComplexFirFilterGeneric : IComplexFirFilter
    {
        private Complex* coeffsBufferPtr;
        private Complex* insampBufferPtr;
        private Complex* insampBufferPtrOffset;

        private UnsafeBuffer coeffsBuffer;
        private UnsafeBuffer insampBuffer;

        private int taps;
        private int decimation;
        private int decimationIndex;
        private int offset;

        internal ComplexFirFilterGeneric(Complex[] coeffs, int decimation)
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

        public int Process(Complex* input, Complex* output, int count, int channels = 1)
        {
            int read = 0;
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
                    ApplyFilter(&output[read++ * channels], &insampBufferPtr[offset], coeffsBufferPtr, taps);
                }
                this.decimationIndex %= this.decimation;
            }
            return read;
        }

        private static void ApplyFilter(Complex* result, Complex* input, Complex* taps, int tapCount)
        {
            //Get pointers
            float* inp = (float*)input;
            float* tp = (float*)taps;
            int blocks = tapCount / 2;

            //Allocate
            float[] sum0 = { 0, 0 };
            float[] sum1 = { 0, 0 };
            int i = 0;

            //Calculate
            for (i = 0; i < blocks; ++i)
            {
                sum0[0] += inp[0] * tp[0] - inp[1] * tp[1];
                sum0[1] += inp[0] * tp[1] + inp[1] * tp[0];
                sum1[0] += inp[2] * tp[2] - inp[3] * tp[3];
                sum1[1] += inp[2] * tp[3] + inp[3] * tp[2];

                inp += 4;
                tp += 4;
            }

            //Apply
            result->Real = sum0[0] + sum1[0];
            result->Imag = sum0[1] + sum1[1];

            //If we have an odd number of taps, apply the remaining bits
            if ((tapCount & 1) != 0)
            {
                //*result += input[tapCount - 1] * taps[tapCount - 1];
            }
        }

        public int Process(Complex* ptr, int count, int channels = 1)
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
