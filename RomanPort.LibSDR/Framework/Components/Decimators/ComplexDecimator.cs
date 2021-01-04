using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework.Components.Decimators
{
    public class ComplexDecimator : BaseDecimator<Complex>
    {
        public ComplexDecimator(float sampleRate, float bandwidth, int decimationRate) : base(decimationRate)
        {
            //Determine filter bandwidth
            bandwidth = Math.Min(bandwidth, sampleRate / decimationRate);

            //Build filter
            float[] coeffs = FilterBuilder.MakeLowPassKernel(sampleRate, 32, (int)(bandwidth / 2), WindowType.BlackmanHarris7);
            filter = new IQFirFilter(coeffs);
            filter.SetProcessEveryN(decimationRate);
        }

        private IQFirFilter filter;

        protected override unsafe void ProcessFilter(Complex* ptr, int count)
        {
            filter.Process(ptr, count);
        }
    }
}
