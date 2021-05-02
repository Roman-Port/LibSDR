using RomanPort.LibSDR.Components.Filters.FIR.Real.Implementations;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters.FIR.Real
{
    public static class RealFirFilter
    {
        public static IRealFirFilter CreateFirFilter(IFilterBuilderReal builder, int decimation = 1)
        {
            builder.ValidateDecimation(decimation);
            return CreateFirFilter(builder.BuildFilterReal(), decimation);
        }

        public static IRealFirFilter CreateFirFilter(float[] coeffs, int decimation = 1)
        {
            if (VolkApi.volkSupported)
                return new RealFirFilterVOLK(coeffs, decimation);
            else
                return new RealFirFilterGeneric(coeffs, decimation);
        }
    }
}
