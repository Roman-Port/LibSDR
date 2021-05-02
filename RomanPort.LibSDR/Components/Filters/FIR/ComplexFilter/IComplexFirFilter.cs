using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters.FIR.ComplexFilter
{
    public interface IComplexFirFilter : IDisposable
    {
        unsafe int Process(Complex* input, Complex* output, int count, int channels = 1);
        unsafe int Process(Complex* ptr, int count, int channels = 1);
    }
}
