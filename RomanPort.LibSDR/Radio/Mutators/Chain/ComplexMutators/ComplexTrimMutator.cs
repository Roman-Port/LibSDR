using RomanPort.LibSDR.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Radio.Mutators.Chain.ComplexMutators
{
    public class ComplexTrimMutator : BaseMutatorChained<Complex>
    {
        public ComplexTrimMutator(long startSample, long endSample)
        {
            this.startSample = startSample;
            this.endSample = endSample;
        }

        protected long startSample;
        protected long endSample;
        private long currentSample;

        public override float OutputSampleRate => InputSampleRate;

        public override void DisposeInternal()
        {
            
        }

        protected override void ConfigureInternal(float inputSampleRate)
        {
            
        }

        protected override unsafe int ProcessInternal(Complex* ptr, int count)
        {
            int written = 0;
            for(int i = 0; i<count; i++)
            {
                //Attempt to write
                if(currentSample >= startSample && currentSample < endSample)
                {
                    ptr[written] = ptr[i];
                    written++;
                }

                //Update state
                currentSample++;
            }
            return written;
        }
    }
}
