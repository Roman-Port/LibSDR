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
            converter = new Oscillator();
            converter.Frequency = offsetFreq;
        }

        private Oscillator converter;

        public override float OutputSampleRate => throw new NotImplementedException();

        public override void DisposeInternal()
        {
            
        }

        protected override void ConfigureInternal(float inputSampleRate)
        {
            converter.SampleRate = inputSampleRate;
        }

        protected override unsafe int ProcessInternal(Complex* ptr, int count)
        {
            converter.Mix(ptr, count);
            return count;
        }
    }
}
