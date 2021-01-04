using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Radio.Mutators.Chain.ComplexMutators
{
    public class ComplexTrimSecondsMutator : ComplexTrimMutator
    {
        public ComplexTrimSecondsMutator(double startSeconds, double endSeconds) : base(0, 0)
        {
            this.startSeconds = startSeconds;
            this.endSeconds = endSeconds;
        }

        private double startSeconds;
        private double endSeconds;

        protected override void ConfigureInternal(float inputSampleRate)
        {
            //Transform to be in samples
            startSample = (long)(startSeconds * inputSampleRate);
            endSample = (long)(endSeconds * inputSampleRate);
        }
    }
}
