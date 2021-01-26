using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Radio.Mutators
{
    public class MutatorSource<T> : BaseMutator<T> where T : unmanaged
    {
        public MutatorSource(float sampleRate)
        {
            this.sampleRate = sampleRate;
        }

        private float sampleRate;
        public override float OutputSampleRate => sampleRate;

        public override void DisposeInternal()
        {
            
        }

        public override unsafe int Process(T* ptr, int count)
        {
            if (next != null)
                return next.Process(ptr, count);
            else
                return count;
        }
    }
}
