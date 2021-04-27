using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters.FIR
{
    public unsafe class TrueComplexFirFilter
    {
        public TrueComplexFirFilter(IFilterBuilderComplex builder, int decimation = 1) : this(builder.BuildFilterComplex(), decimation)
        {

        }

        public TrueComplexFirFilter(Complex[] c, int decimation = 1)
        {
            //Configure
            this.decimation = decimation;
            taps = c.Length * 2;
            tapsComplex = c.Length;

            //Create buffers
            coeffsBuffer = UnsafeBuffer.Create(c.Length * TAPS_COPIES, out coeffs);
            bufferBuffer = UnsafeBuffer.Create(c.Length, out buffer);

            //Copy in coeffs...multiple times...so the buffer can loop around
            fixed (Complex* cp = c)
            {
                for (int i = 0; i < TAPS_COPIES; i++)
                    Utils.Memcpy(coeffs + (i * c.Length), cp, c.Length * sizeof(Complex));
            }

            //Offset pointer to middle
            coeffs += c.Length * (TAPS_COPIES / 2);
        }

        private const int TAPS_COPIES = 2;

        private int decimation;
        private int tapsComplex;
        private int taps;

        private int decimationIndex;
        private int offset;

        private UnsafeBuffer coeffsBuffer;
        private UnsafeBuffer bufferBuffer;

        private Complex* coeffs;
        private Complex* buffer;

        /// <summary>
        /// Processes the filter. Returns the number of samples processed. Returned count will never be larger than the input count, and will always match the input count if decimation == 1
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="count"></param>
        /// <param name="channels"></param>
        /// <returns></returns>
        public int Process(Complex* ptr, int count, int channels = 1)
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
        public int Process(Complex* inPtr, Complex* outPtr, int count, int channels = 1)
        {
            return FastFunctions.ApplyFilterComplex(inPtr, outPtr, count, channels, coeffs, tapsComplex, buffer, ref offset, decimation, ref decimationIndex);
        }
    }
}
