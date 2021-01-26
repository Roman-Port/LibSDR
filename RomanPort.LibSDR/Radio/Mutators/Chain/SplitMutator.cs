using RomanPort.LibSDR.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Radio.Mutators.Chain
{
    /// <summary>
    /// This mutator will dispatch samples to multiple mutators.
    /// 
    /// Since this breaks the chain method a little bit, this will always claim to have processed zero samples. You'll need to request buffers from this
    /// </summary>
    public unsafe class SplitMutator<T> : BaseMutatorChained<T> where T : unmanaged
    {
        public SplitMutator(int bufferSize)
        {
            this.bufferSize = bufferSize;
        }

        private int bufferSize;

        private T* lastSharedBufferPtr;
        private List<BaseMutatorChained<T>> outputs = new List<BaseMutatorChained<T>>();
        private List<SplitOutputBuffers> buffers = new List<SplitOutputBuffers>();
        private List<int> outputsConsumed = new List<int>();

        public override float OutputSampleRate => InputSampleRate;

        public override void DisposeInternal()
        {
            //Dispose of each output
            foreach (var o in outputs)
                o.Dispose();
            
            //Dispose of each buffer
            foreach (var b in buffers)
                b.buffer.Dispose();
        }

        protected override void ConfigureInternal(float inputSampleRate)
        {
            foreach (var o in outputs)
                o.Configure(inputSampleRate);
        }

        protected override unsafe int ProcessInternal(T* ptr, int count)
        {
            //Loop through all but the first output because these will need their own buffer
            for(int i = 1; i < outputs.Count; i++)
            {
                //Get or create a buffer for this
                SplitOutputBuffers b;
                if (buffers.Count >= i)
                    b = buffers[i - 1];
                else
                    b = CreateNewBuffer();

                //Copy to buffer
                Utils.Memcpy(b.bufferPtr, ptr, count * sizeof(T));

                //Process
                outputsConsumed[i] = outputs[i].Process(b.bufferPtr, count);
            }

            //Process the first output on our shared buffer now that we've copied everything out
            lastSharedBufferPtr = ptr;
            if (outputs.Count > 0)
                outputsConsumed[0] = outputs[0].Process(ptr, count);

            //Return 0 so that we don't attempt to use this as the normal chain object
            return 0;
        }

        public override BaseMutatorChained<T> Then(BaseMutatorChained<T> mutator)
        {
            outputs.Add(mutator);
            outputsConsumed.Add(0);
            return mutator;
        }

        private SplitOutputBuffers CreateNewBuffer()
        {
            //Make buffer
            UnsafeBuffer buffer = UnsafeBuffer.Create(bufferSize, sizeof(T));
            T* bufferPtr = (T*)buffer;

            //Make buffers object
            SplitOutputBuffers o = new SplitOutputBuffers
            {
                buffer = buffer,
                bufferPtr = bufferPtr
            };

            //Add to list
            buffers.Add(o);

            return o;
        }

        /// <summary>
        /// Adds the mutator to the list of multiple outputs.
        /// </summary>
        /// <param name="mutator"></param>
        /// <returns></returns>
        public BaseMutatorChained<T> AddOutput(BaseMutatorChained<T> mutator)
        {
            return Then(mutator);
        }

        /// <summary>
        /// Returns the pointer to the output of an attached child mutator, by index. Only call this after processing.
        /// </summary>
        /// <param name="index">The index of the attached child.</param>
        /// <param name="consumed">The output number of samples from that child.</param>
        /// <returns></returns>
        public T* GetOutputBuffer(int index, out int consumed)
        {
            //Set the consumed
            consumed = outputsConsumed[index];

            //If this is index 0, we use the shared buffer. Otherwise, use the buffer we created
            if (index == 0)
                return lastSharedBufferPtr;
            else
                return buffers[index - 1].bufferPtr;
        }

        /// <summary>
        /// Returns the pointer to the output of an attached child mutator. Only call this after processing.
        /// </summary>
        /// <param name="index">The attached child.</param>
        /// <param name="consumed">The output number of samples from that child.</param>
        /// <returns></returns>
        public T* GetOutputBuffer(BaseMutatorChained<T> child, out int consumed)
        {
            //Find the child
            int index = outputs.IndexOf(child);
            if (index == -1)
                throw new Exception("This is not a child of this mutator.");

            return GetOutputBuffer(index, out consumed);
        }

        class SplitOutputBuffers
        {
            public UnsafeBuffer buffer;
            public T* bufferPtr;
        }
    }
}
