using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework.Components.Filters
{
    public class ComplexCicFilter
    {
        private FloatCicFilter cicA;
        private FloatCicFilter cicB;

        public ComplexCicFilter(int decimationFactor, int numberOfSections = 1, int diferrencialDelay = 1)
        {
            cicA = new FloatCicFilter(decimationFactor, numberOfSections, diferrencialDelay);
            cicB = new FloatCicFilter(decimationFactor, numberOfSections, diferrencialDelay);
        }

        public unsafe int Process(Complex* buffer, int length)
        {
            //Get pointers as floats
            float* bufferFloat = (float*)buffer;

            //Process
            cicA.Process(bufferFloat, length, 2);
            return cicB.Process(bufferFloat + 1, length, 2);
        }
    }
}
