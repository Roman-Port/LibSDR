using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework
{
    public class ManagedCircularBuffer<T> : IDisposable where T : unmanaged
    {
        private CircularBuffer<T> buffer;
        
        public ManagedCircularBuffer(int bufferElementCount)
        {
            buffer = new CircularBuffer<T>(bufferElementCount);
        }

        public unsafe int Write(T[] data, int count, bool force = false)
        {
            int o;
            fixed (T* ptr = data)
                o = buffer.Write(ptr, count, force);
            return o;
        }

        public unsafe int Read(T[] output, int maxCount)
        {
            int o;
            fixed (T* ptr = output)
                o = buffer.Read(ptr, maxCount);
            return o;
        }

        public int GetAvailable()
        {
            return buffer.GetAvailable();
        }

        public int GetSpaceRemaining()
        {
            return buffer.GetSpaceRemaining();
        }

        public void Dispose()
        {
            buffer.Dispose();
        }
    }
}
