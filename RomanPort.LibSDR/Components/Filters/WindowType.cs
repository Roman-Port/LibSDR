using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters
{
    public enum WindowType
    {
        None,
        Hamming,
        Blackman,
        BlackmanHarris4,
        BlackmanHarris7,
        HannPoisson,
        Youssef
    }
}
