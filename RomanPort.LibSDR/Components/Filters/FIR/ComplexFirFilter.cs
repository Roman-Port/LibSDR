using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters.FIR
{
    public class ComplexFirFilter
    {
        public ComplexFirFilter(IFilterBuilderReal builder, int decimation = 1) : this(builder.BuildFilterReal(), decimation)
        {
            builder.ValidateDecimation(decimation);
        }

        public ComplexFirFilter(IFilterBuilderComplex builder, int decimation = 1) : this(builder.BuildFilterComplex(), decimation)
        {
            builder.ValidateDecimation(decimation);
        }

        public ComplexFirFilter(Complex[] coeffs, int decimation = 1) : this(DeInterleaveComplexReal(coeffs), DeInterleaveComplexImag(coeffs), decimation)
        {
        }

        public ComplexFirFilter(float[] coeffs, int decimation = 1) : this(coeffs, coeffs, decimation)
        {
        }

        public ComplexFirFilter(float[] coeffsA, float[] coeffsB, int decimation = 1)
        {
            a = new FloatFirFilter(coeffsA, decimation);
            b = new FloatFirFilter(coeffsB, decimation);
        }

        protected FloatFirFilter a;
        protected FloatFirFilter b;

        private static float[] DeInterleaveComplexReal(Complex[] coeffs)
        {
            float[] output = new float[coeffs.Length];
            for (int i = 0; i < coeffs.Length; i++)
                output[i] = coeffs[i].Real;
            return output;
        }

        private static float[] DeInterleaveComplexImag(Complex[] coeffs)
        {
            float[] output = new float[coeffs.Length];
            for (int i = 0; i < coeffs.Length; i++)
                output[i] = coeffs[i].Imag;
            return output;
        }

        public unsafe int Process(Complex* ptr, int count)
        {
            return Process(ptr, ptr, count);
        }

        public virtual unsafe int Process(Complex* input, Complex* output, int count)
        {
            a.Process(((float*)input), ((float*)output), count, 2);
            return b.Process(((float*)input) + 1, ((float*)output) + 1, count, 2);
        }
    }
}
