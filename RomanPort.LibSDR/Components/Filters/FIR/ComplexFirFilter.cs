using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters.FIR
{
    public class ComplexFirFilter
    {
        public ComplexFirFilter(FilterPassBuilderBase builder, int decimation = 1) : this(builder.BuildFilter(), decimation)
        {

        }

        public ComplexFirFilter(float[] coeffs, int decimation = 1)
        {
            a = new FloatFirFilter(coeffs, decimation);
            b = new FloatFirFilter(coeffs, decimation);
        }

        private FloatFirFilter a;
        private FloatFirFilter b;

        public unsafe int Process(Complex* ptr, int count)
        {
            float* fPtr = (float*)ptr;
            a.Process(fPtr, count, 2);
            return b.Process(fPtr + 1, count, 2);
        }
    }
}
