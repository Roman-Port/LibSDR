using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework
{
    public unsafe class CircularBuffer<T> : IDisposable where T : unmanaged
    {
        private int bufferSize;
        private UnsafeBuffer buffer;
        private T* bufferPtr;

        private int readIndex;
        private int writeIndex;
        private int itemsWaiting; //Items written that have not yet been read

        public CircularBuffer(int bufferElementCount)
        {
            //Open buffer
            bufferSize = bufferElementCount;
            buffer = UnsafeBuffer.Create(bufferElementCount, sizeof(T));
            bufferPtr = (T*)buffer;
        }

        public int Write(T* data, int count, bool force = false)
        {
            //Determine the MAX number of items we can write without overwriting data and then find the max we'll actually use
            int maxBytesWritable;
            if (force)
                maxBytesWritable = count; //We're going to write everything, even if it overwrites data
            else
                maxBytesWritable = Math.Min(bufferSize - itemsWaiting, count); //Write just what is safe to write

            //Write blocks until the data wraps
            int remaining = maxBytesWritable;
            int inputOffset = 0;
            while(remaining > 0)
            {
                //Determine how many of this block we can read, up until the wrap
                int preWrapBytes = Math.Min(remaining, bufferSize - writeIndex);

                //Copy
                Utils.Memcpy(bufferPtr + writeIndex, data + inputOffset, preWrapBytes * sizeof(T));

                //Update
                remaining -= preWrapBytes;
                inputOffset += preWrapBytes;
                writeIndex = (writeIndex + preWrapBytes) % bufferSize;
                itemsWaiting += preWrapBytes;
            }

            return inputOffset;
        }

        public int Read(T* output, int maxCount)
        {
            //Determine the number of bytes we're able to read
            int maxBytesReadable = Math.Min(itemsWaiting, maxCount);

            //Read blocks up until the data wraps
            int remaining = maxBytesReadable;
            int outputOffset = 0;
            while(remaining > 0)
            {
                //Determine how many of this block we can read, up until the wrap
                int preWrapBytes = Math.Min(remaining, bufferSize - readIndex);

                //Copy
                Utils.Memcpy(output + outputOffset, bufferPtr + readIndex, preWrapBytes * sizeof(T));

                //Update
                remaining -= preWrapBytes;
                outputOffset += preWrapBytes;
                readIndex = (readIndex + preWrapBytes) % bufferSize;
                itemsWaiting -= preWrapBytes;
            }
            
            return outputOffset;
        }

        public int GetAvailable()
        {
            return itemsWaiting;
        }

        public int GetSpaceRemaining()
        {
            return bufferSize - itemsWaiting;
        }

        public void Dispose()
        {
            buffer.Dispose();
        }
    }
}
