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

        public int Write(T* data, int count)
        {
            //Determine the MAX number of items we can write without overwriting data and then find the max we'll actually use
            int maxBytesWritable = Math.Min(bufferSize - itemsWaiting, count);

            //Begin writing bytes
            for(int i = 0; i<maxBytesWritable; i++)
            {
                bufferPtr[writeIndex] = data[i];
                writeIndex = (writeIndex + 1) % bufferSize;
            }

            itemsWaiting += maxBytesWritable;

            return maxBytesWritable;
        }

        public int Read(T* output, int maxCount)
        {
            //Determine the number of bytes we're able to read
            int maxBytesReadable = Math.Min(itemsWaiting, maxCount);

            //Begin reading bytes
            for (int i = 0; i < maxBytesReadable; i++)
            {
                output[i] = bufferPtr[readIndex];
                readIndex = (readIndex + 1) % bufferSize;
            }

            itemsWaiting -= maxBytesReadable;

            return maxBytesReadable;
        }

        public int GetAvailable()
        {
            return itemsWaiting;
        }

        public void Dispose()
        {
            buffer.Dispose();
        }
    }
}
