using RomanPort.LibSDR.Components.Filters.FIR.ComplexFilter.Implementations;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters.FIR.ComplexFilter
{
    public static class ComplexFirFilter
    {
        public static IComplexFirFilter CreateFirFilter(IFilterBuilderComplex builder, int decimation = 1)
        {
            builder.ValidateDecimation(decimation);
            return CreateFirFilter(builder.BuildFilterComplex(), decimation);
        }

        public static IComplexFirFilter CreateFirFilter(Complex[] coeffs, int decimation = 1)
        {
            if (VolkApi.volkSupported)
                return new ComplexFirFilterVOLK(coeffs, decimation);
            else
                return new ComplexFirFilterGeneric(coeffs, decimation);
        }
    }
}
