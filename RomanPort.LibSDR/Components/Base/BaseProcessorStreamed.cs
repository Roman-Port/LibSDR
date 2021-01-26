using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Base
{
    /// <summary>
    /// This class allows functions that require a certain number of samples to be called. This behaves much like a stream
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public unsafe abstract class BaseProcessorStreamed<T> : IDisposable where T : unmanaged
    {
        public BaseProcessorStreamed(int blockSize)
        {
            this.blockSize = blockSize;
            buffer = UnsafeBuffer.Create(blockSize, out bufferPtr);
        }

        protected int blockSize;
        private UnsafeBuffer buffer;
        private T* bufferPtr;
        private int bufferUsage;

        protected abstract int ProcessBlock(T* input, T* output, int inputCount, int outputCount, out int inputConsumed);

        public int Process(T* input, T* output, int count, int outputSize)
        {
            int writtenTotal = 0;
            while(count > 0)
            {
                //If we have bytes to skip, process those
                if(bufferUsage < 0)
                {
                    bufferUsage++;
                    input++;
                    count--;
                    continue;
                }
                
                //Transfer all we can into the buffer
                int copyable = Math.Min(count, blockSize - bufferUsage);

                //Transfer and update state
                Utils.Memcpy(bufferPtr + bufferUsage, input, copyable * sizeof(T));
                bufferUsage += copyable;
                input += copyable;
                count -= copyable;

                //Process
                int written = ProcessBlock(bufferPtr, output, bufferUsage, outputSize, out int consumed);

                //Offset output
                writtenTotal += written;
                output += written;

                //Switch on how many bytes were consumed
                if(consumed == 0 && bufferUsage == blockSize)
                {
                    //Probably an infite loop where bytes won't be consumed
                    throw new Exception("Infinite loop detected!");
                } else if (consumed < bufferUsage)
                {
                    //Move unused bytes and update state
                    Utils.Memcpy(bufferPtr, bufferPtr + consumed, (bufferUsage - consumed) * sizeof(T));
                    bufferUsage -= consumed;
                } else if (consumed == bufferUsage)
                {
                    //Consumed all. Don't bother copying bytes, as there's nothing to copy!
                    bufferUsage = 0;
                } else if (consumed > bufferUsage)
                {
                    //We'll skip these number of bytes 
                    bufferUsage -= consumed;
                } else
                {
                    throw new Exception("Unknown state.");
                }
            }
            return writtenTotal;
        }

        public void Dispose()
        {
            buffer.Dispose();
        }
    }
}
