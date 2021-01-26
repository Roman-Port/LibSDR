using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Radio.Mutators
{
    public abstract class BaseMutator<T> : IDisposable where T : unmanaged
    {
        /// <summary>
        /// The output samplerate.
        /// </summary>
        public abstract float OutputSampleRate { get; }

        /// <summary>
        /// Processes the requested data. 
        /// </summary>
        /// <param name="ptr">The buffer to use. May become modified.</param>
        /// <param name="count">The input count.</param>
        /// <returns>The output count.</returns>
        public abstract unsafe int Process(T* ptr, int count);

        /// <summary>
        /// Clean up
        /// </summary>
        public abstract void DisposeInternal();

        /// <summary>
        /// The output sample rate of the last item in the chain
        /// </summary>
        public float ChainOutputSampleRate
        {
            get
            {
                if (next == null)
                    return OutputSampleRate;
                else
                    return next.ChainOutputSampleRate;
            }
        }

        internal BaseMutatorChained<T> next;

        /// <summary>
        /// Adds an object to the chain
        /// </summary>
        /// <param name="mutator"></param>
        /// <returns></returns>
        public virtual BaseMutatorChained<T> Then(BaseMutatorChained<T> mutator)
        {
            mutator.SetNewParent(this);
            next = mutator;
            return mutator;
        }

        public void Dispose()
        {
            //Dispose children
            next?.Dispose();

            //Dispose this
            DisposeInternal();
        }
    }
}
