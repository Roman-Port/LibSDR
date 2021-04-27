using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RomanPort.LibSDR.Components.IO.Buffers
{
    public unsafe class CircularBuffer<T> : IDisposable where T : unmanaged
    {
        private int bufferSize;
        private UnsafeBuffer buffer;
        private T* bufferPtr;

        private volatile int read;
        private volatile int write;

        public CircularBuffer(int bufferElementCount)
        {
            bufferSize = bufferElementCount + 1;
            buffer = UnsafeBuffer.Create(bufferElementCount + 1, sizeof(T));
            bufferPtr = (T*)buffer;
        }

        public bool IsEmpty { get => read == write; }
        public bool IsFull { get => Free == 0; }
        public int Waiting { get => (((write - read) % (bufferSize + 1)) + bufferSize) % bufferSize; }
        public int Free { get => bufferSize - Waiting - 1; }


        public bool WriteOne(T data, bool force = false)
        {
            //Check if full
            if (IsFull)
                return false;

            //Write
            bufferPtr[write] = data;

            //Update state
            write = (write + 1) % (bufferSize + 1);

            return true;
        }

        public bool ReadOne(T* output)
        {
            //Check if empty
            if (IsEmpty)
                return false;

            //Read
            *output = bufferPtr[read];

            //Update state
            read = (read + 1) % (bufferSize + 1);

            return true;
        }

        public int Write(T* data, int count, bool force = false)
        {
            int written = 0;
            while (written < count && WriteOne(data[written], force))
                written++;
            return written;
        }

        public int Read(T* output, int count)
        {
            int read = 0;
            while (read < count && ReadOne(output + read))
                read++;
            return read;
        }

        public void Dispose()
        {
            buffer.Dispose();
        }
    }
}
