using RomanPort.LibSDR.Framework.Components.Filters;
using RomanPort.LibSDR.Framework.Resamplers.Decimators;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework.Components.Resamplers
{
    public unsafe delegate void BaseResamplerPipelineAction<T>(T* input, int count) where T : unmanaged;

    public abstract unsafe class BaseResamplerPipeline<T> where T : unmanaged
    {
        public BaseResamplerPipeline(float inputSampleRate, float outputSampleRate, int bufferSize, BaseResamplerPipelineAction<T> processBlock)
        {
            this.bufferSize = bufferSize;
            this.inputSampleRate = inputSampleRate;
            this.outputSampleRate = outputSampleRate;
            this.processBlock = processBlock;

            //Create filter
            if(inputSampleRate > outputSampleRate)
            {
                InitFilter(inputSampleRate, (int)(outputSampleRate / 2));
            }

            //Initialize stuff
            float curSampleRate = inputSampleRate;
            if (inputSampleRate > outputSampleRate)
            {
                //Since we're downsampling, try to see if we can just decimate it
                int decimationRate = SdrFloatDecimator.CalculateDecimationRate(curSampleRate, outputSampleRate, out curSampleRate);
                if (decimationRate > 1)
                {
                    InitDecimator(decimationRate);
                    decimatorBuffer = new PipelineBufferInstance(bufferSize);
                }
            }
            if((int)curSampleRate != (int)outputSampleRate)
            {
                //Create arb resampler to bring the sample rate to the desired output
                InitArbResampler(curSampleRate, outputSampleRate);
                resamplerBuffer = new PipelineBufferInstance(bufferSize);
            }
        }

        private float inputSampleRate;
        private float outputSampleRate;
        private int bufferSize;
        private BaseResamplerPipelineAction<T> processBlock;

        private PipelineBuffer decimatorBuffer;
        private PipelineBuffer resamplerBuffer;

        protected abstract void InitFilter(float sampleRate, int filterBw);
        protected abstract void InitDecimator(int decimationFactor);
        protected abstract void InitArbResampler(float fromRate, float toRate);
        protected abstract void ProcessFilter(T* ptr, int count);
        protected abstract int ProcessDecimator(T* input, T* output, int inputCount, int outputCount, int oldSamples, out int inputConsumed);
        protected abstract int ProcessArbResampler(T* input, T* output, int inputCount, int outputCount, int oldSamples, out int inputConsumed);

        public void Process(T* ptr, int count)
        {
            //Filter
            if (inputSampleRate > outputSampleRate)
                ProcessFilter(ptr, count);

            //Down/up sample
            if(decimatorBuffer == null && resamplerBuffer == null)
            {
                //Passthrough
                processBlock(ptr, count);
            } else if (decimatorBuffer == null && resamplerBuffer != null)
            {
                //Resample only
                ProcessStep(ptr, resamplerBuffer, count, ProcessArbResampler, processBlock);
            } else if (decimatorBuffer != null && resamplerBuffer == null)
            {
                //Decimate only
                ProcessStep(ptr, decimatorBuffer, count, ProcessDecimator, processBlock);
            } else if (decimatorBuffer != null && resamplerBuffer != null)
            {
                //Decimate, then resample
                ProcessStep(ptr, decimatorBuffer, count, ProcessDecimator, (T* postDecimation, int postDecimationCount) =>
                {
                    ProcessStep(postDecimation, resamplerBuffer, postDecimationCount, ProcessArbResampler, processBlock);
                });
            } else
            {
                //Unknown
                throw new Exception("Unknown steps.");
            }
        }

        private void ProcessStep(T* ptr, PipelineBuffer toBuffer, int count, Step step, BaseResamplerPipelineAction<T> next)
        {
            while(count != 0)
            {
                //Transfer data into the working buffer
                int transferrable = Math.Min(bufferSize - toBuffer.used, count);
                Utils.Memcpy(toBuffer.ptr + toBuffer.used, ptr, transferrable * sizeof(T));
                toBuffer.used += transferrable;
                ptr += transferrable;
                count -= transferrable;

                //Resample
                int output = step(toBuffer.ptr, toBuffer.workingBufferPtr, toBuffer.used, bufferSize, toBuffer.used - transferrable, out int consumed);

                //Move samples that weren't consumed to the beginning of the buffer and update the state
                Utils.Memcpy(toBuffer.ptr, toBuffer.ptr + consumed, (toBuffer.used - consumed) * sizeof(T));
                toBuffer.used -= consumed;

                //Pass on to the next
                next(toBuffer.workingBufferPtr, output);
            }
        }

        delegate int Step(T* input, T* output, int inputCount, int outputCount, int oldSamples, out int inputConsumed);

        class PipelineBuffer
        {
            public T* ptr;
            public T* workingBufferPtr;
            public int used;

            private UnsafeBuffer workingBuffer;

            public PipelineBuffer(int bufferSize, T* ptr)
            {
                this.ptr = ptr;
                workingBuffer = UnsafeBuffer.Create<T>(bufferSize, out workingBufferPtr);
            }
        }

        class PipelineBufferInstance : PipelineBuffer
        {
            public UnsafeBuffer buffer;

            public PipelineBufferInstance(int bufferSize) : base(bufferSize, Create(bufferSize, out UnsafeBuffer buffer))
            {
                this.buffer = buffer;
            }

            private static T* Create(int bufferSize, out UnsafeBuffer buffer)
            {
                buffer = UnsafeBuffer.Create<T>(bufferSize, out T* p);
                return p;
            }
        }
    }
}
