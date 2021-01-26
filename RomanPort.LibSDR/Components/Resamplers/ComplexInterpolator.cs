using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Resamplers
{
    public class ComplexInterpolator
    {
        public ComplexInterpolator(int interpolationRate)
        {
            this.interpolationRate = interpolationRate;
            interpA = new FloatInterpolator(interpolationRate, 2);
            interpB = new FloatInterpolator(interpolationRate, 2);
        }

        private int interpolationRate;
        private FloatInterpolator interpA;
        private FloatInterpolator interpB;

        public unsafe int Process(Complex* input, Complex* output, int count)
        {
            float* inputPtr = (float*)input;
            float* outputPtr = (float*)output;
            interpA.Process(inputPtr, outputPtr, count);
            return interpB.Process(inputPtr + 1, outputPtr + 1, count);
        }
    }
}
