using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Radio.Mutators
{
    public abstract class BaseMutatorChained<T> : BaseMutator<T> where T: unmanaged
    {
        /// <summary>
        /// The input samplerate. Not set until Init is called
        /// </summary>
        public float InputSampleRate { get; internal set; }

        /// <summary>
        /// Called when this is attached. Set it up here.
        /// </summary>
        /// <param name="inputSampleRate"></param>
        protected abstract void ConfigureInternal(float inputSampleRate);

        /// <summary>
        /// Processes the requested data. 
        /// </summary>
        /// <param name="ptr">The buffer to use. May become modified.</param>
        /// <param name="count">The input count.</param>
        /// <returns>The output count.</returns>
        protected abstract unsafe int ProcessInternal(T* ptr, int count);

        internal BaseMutator<T> previous;

        /// <summary>
        /// Called when this is attached. Set it up here.
        /// </summary>
        /// <param name="inputSampleRate"></param>
        internal void Configure(float inputSampleRate)
        {
            //Apply to this
            this.ConfigureInternal(inputSampleRate);

            //Apply to children
            next?.Configure(inputSampleRate);
        }

        /// <summary>
        /// Processes the requested data. 
        /// </summary>
        /// <param name="ptr">The buffer to use. May become modified.</param>
        /// <param name="count">The input count.</param>
        /// <returns>The output count.</returns>
        public override unsafe int Process(T* ptr, int count)
        {
            //Process
            int outCount = ProcessInternal(ptr, count);

            //Send to next
            if (next != null)
                return next.Process(ptr, outCount);
            else
                return outCount;
        }

        /// <summary>
        /// Removes this item from the chain by transferring references
        /// </summary>
        public void RemoveFromChain()
        {
            //If there is nothing next, do nothing
            if (next == null)
                return;

            //Transfer ownership of children
            next.SetNewParent(previous);

            //Update parent
            previous.next = next;
        }

        /// <summary>
        /// Sets up this for a new parent to be set
        /// </summary>
        /// <param name="parent"></param>
        internal void SetNewParent(BaseMutator<T> parent)
        {
            InputSampleRate = parent.OutputSampleRate;
            ConfigureInternal(parent.OutputSampleRate);
            previous = parent;
        }
    }
}
