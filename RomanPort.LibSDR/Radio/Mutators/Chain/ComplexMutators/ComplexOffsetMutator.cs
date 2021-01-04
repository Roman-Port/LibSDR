using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Radio.Mutators.Chain.ComplexMutators
{
    public class ComplexOffsetMutator : BaseMutatorChained<Complex>
    {
        public ComplexOffsetMutator(float offsetFreq)
        {
            converter = new DownConverter(1);
            converter.Frequency = offsetFreq;
        }

        private DownConverter converter;

        public override float OutputSampleRate => throw new NotImplementedException();

        public override void DisposeInternal()
        {
            converter.Dispose();
        }

        protected override void ConfigureInternal(float inputSampleRate)
        {
            converter.SampleRate = inputSampleRate;
        }

        protected override unsafe int ProcessInternal(Complex* ptr, int count)
        {
            converter.Process(ptr, count);
            return count;
        }
    }
}
