using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework.Resamplers.Decimators
{
    public unsafe class SdrComplexDecimator
    {
        private SdrFloatDecimator decimatorA;
        private SdrFloatDecimator decimatorB;
        
        public SdrComplexDecimator(int decimation)
        {
            decimatorA = new SdrFloatDecimator(decimation, 2, 0);
            decimatorB = new SdrFloatDecimator(decimation, 2, 1);
        }

        public int Process(Complex* inBuffer, int inCount, Complex* outBuffer, int outBufferLen)
        {
            int readA = decimatorA.Process((float*)inBuffer, inCount, (float*)outBuffer, outBufferLen);
            int readB = decimatorB.Process((float*)inBuffer, inCount, (float*)outBuffer, outBufferLen);
            if (readA != readB)
                throw new Exception("The number of samples read for each channel did not match!");
            return readA;
        }
    }
}
