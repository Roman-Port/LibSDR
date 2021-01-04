using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Components.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Radio.Mutators.Chain.ComplexMutators
{
    public class ComplexDecimateMutator : BaseMutatorChained<Complex>
    {
        public ComplexDecimateMutator(int decimationFactor)
        {
            this.decimationFactor = decimationFactor;
            cic = new ComplexCicFilter(decimationFactor);
        }

        public int DecimationFactor { get => decimationFactor; }

        private int decimationFactor;
        private ComplexCicFilter cic;

        public override float OutputSampleRate => InputSampleRate / decimationFactor;

        public override void DisposeInternal()
        {
            cic.Dispose();
        }

        protected override void ConfigureInternal(float inputSampleRate)
        {

        }

        protected override unsafe int ProcessInternal(Complex* ptr, int count)
        {
            return cic.Process(ptr, count);
        }
    }
}
