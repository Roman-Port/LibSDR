using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters
{
    public interface IFilterBuilder
    {
        float SampleRate { get; set; }
        void ValidateDecimation(int decimation);
        int GetDecimation(out float outputSampleRate);
    }
}
