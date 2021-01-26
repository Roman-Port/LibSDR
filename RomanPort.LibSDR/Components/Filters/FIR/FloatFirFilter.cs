using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters.FIR
{
    /// <summary>
    /// This is based off of MIT-licensed fir1 at https://github.com/berndporr/fir1/
    /// </summary>
    public unsafe class FloatFirFilter : IDisposable
    {
        public FloatFirFilter(FilterBuilderBase builder, int decimation = 1) : this(builder.BuildFilter(), decimation)
        {

        }

        public FloatFirFilter(float[] coeffs, int decimation = 1)
        {
            //Configure
            this.decimation = decimation;
            
            //Create buffers
            coefficientsHandle = UnsafeBuffer.Create(coeffs.Length, out coefficients);
            coefficientsEnd = coefficients + coeffs.Length;
            bufferHandle = UnsafeBuffer.Create(coeffs.Length, out buffer);

            //Set coeffs
            fixed (float* coeffsPtr = coeffs)
                Utils.Memcpy(coefficients, coeffsPtr, coeffs.Length * sizeof(float));
            taps = coeffs.Length;
        }

        int decimation;
        int decimationIndex;

        UnsafeBuffer coefficientsHandle;
        UnsafeBuffer bufferHandle;
        float* coefficients;
        float* coefficientsEnd;
        float* buffer;
        int taps;
        int offset = 0;

        /// <summary>
        /// Processes the filter. Returns the number of samples processed. Returned count will never be larger than the input count, and will always match the input count if decimation == 1
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="count"></param>
        /// <param name="channels"></param>
        /// <returns></returns>
        public int Process(float* ptr, int count, int channels = 1)
        {
            return Process(ptr, ptr, count, channels);
        }

        /// <summary>
        /// Processes the filter. Returns the number of samples processed. Returned count will never be larger than the input count, and will always match the input count if decimation == 1
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="count"></param>
        /// <param name="channels"></param>
        /// <returns></returns>
        public int Process(float* inPtr, float* outPtr, int count, int channels = 1)
        {
            float* coeff;
            float* bufferOffset;
            int processed = 0;
            for (int i = 0; i < count; i++)
            {
                //Write current value to the buffer
                buffer[offset] = *inPtr;

                //Check if we should process this
                decimationIndex++;
                if (decimationIndex == decimation)
                {
                    //Perform the filtering...
                    coeff = coefficients;
                    bufferOffset = buffer + offset;
                    *outPtr = 0;
                    while (bufferOffset >= buffer)
                        *outPtr += *bufferOffset-- * *coeff++;
                    bufferOffset = buffer + taps - 1;
                    while (coeff < coefficientsEnd)
                        *outPtr += *bufferOffset-- * *coeff++;

                    //Update state
                    processed++;
                    outPtr += channels;
                    decimationIndex = 0;
                }

                //Reset buffer loop if we go over
                if (++offset >= taps)
                    offset = 0;

                //Update pointer
                inPtr += channels;
            }
            return processed;
        }

        public void Dispose()
        {
            coefficientsHandle.Dispose();
            bufferHandle.Dispose();
        }
    }
}
