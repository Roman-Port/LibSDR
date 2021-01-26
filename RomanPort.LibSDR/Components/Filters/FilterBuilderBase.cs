using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters
{
    public abstract class FilterBuilderBase
    {
        public FilterBuilderBase(float sampleRate)
        {
            SampleRate = sampleRate;
        }

        public float SampleRate { get; set; }

        public abstract float[] BuildFilter();
    }
}
