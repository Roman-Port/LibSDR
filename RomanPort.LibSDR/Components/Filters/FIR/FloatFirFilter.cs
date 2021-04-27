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
        public FloatFirFilter(IFilterBuilderReal builder, int decimation = 1) : this(builder.BuildFilterReal(), decimation)
        {
            builder.ValidateDecimation(decimation);
        }

        public FloatFirFilter(float[] coeffs, int decimation = 1)
        {
            //Configure
            taps = coeffs.Length;
            this.decimation = decimation;
            bufferSize = taps;

            //Create buffers
            coeffsBuffer = UnsafeBuffer.Create(coeffs.Length, out coeffsBufferPtr);
            insampBuffer = UnsafeBuffer.Create(bufferSize, out insampBufferPtr);

            //Copy coeffs
            for (int i = 0; i < coeffs.Length; i++)
                coeffsBufferPtr[i] = coeffs[i];
        }

        private int taps;
        private int decimation;
        private int decimationIndex;
        private int bufferSize;
        private int offset;

        private UnsafeBuffer coeffsBuffer;
        private float* coeffsBufferPtr;
        private UnsafeBuffer insampBuffer;
        private float* insampBufferPtr;

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
            return FastFunctions.ApplyFilterFloat(inPtr, outPtr, count, channels, coeffsBufferPtr, taps, insampBufferPtr, ref offset, decimation, ref decimationIndex);
        }

        public void Dispose()
        {
            coeffsBuffer.Dispose();
            insampBuffer.Dispose();
        }
    }
}