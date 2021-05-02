using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters.FIR.Real
{
    public interface IRealFirFilter : IDisposable
    {
        unsafe int Process(float* input, float* output, int count, int channels = 1);
        unsafe int Process(float* ptr, int count, int channels = 1);
    }
}
