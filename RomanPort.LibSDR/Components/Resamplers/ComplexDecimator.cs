using RomanPort.LibSDR.Components.Filters.FIR;
using RomanPort.LibSDR.Components.Filters.FIR.ComplexFilter;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Resamplers
{
    public unsafe class ComplexDecimator
    {
        public ComplexDecimator(float sampleRate)
        {

        }

        private float sampleRate;
        private int decimationStages;
        private IComplexFirFilter[] stages;

        private void Configure()
        {
            //Create stages
            stages = new IComplexFirFilter[decimationStages];

        }
    }
}
