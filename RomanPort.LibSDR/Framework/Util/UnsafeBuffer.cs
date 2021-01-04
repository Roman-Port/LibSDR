using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RomanPort.LibSDR.Framework.Util
{
    public unsafe sealed class UnsafeBuffer : IDisposable
    {
        private readonly GCHandle _handle;
        private void* _ptr;
        private int _length;
        public Array _buffer;

        private UnsafeBuffer(Array buffer, int realLength, bool aligned)
        {
            _buffer = buffer;
            _handle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
            _ptr = (void*)_handle.AddrOfPinnedObject();
            if (aligned)
            {
                _ptr = (void*)(((long)_ptr + 15) & ~15);
            }
            _length = realLength;
        }

        ~UnsafeBuffer()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_handle.IsAllocated)
            {
                _handle.Free();
            }
            _buffer = null;
            _ptr = null;
            _length = 0;
            GC.SuppressFinalize(this);
        }

        public GCHandle GcHandle
        {
            get => _handle;
        }

        public void* Address
        {
            get { return _ptr; }
        }

        public int Length
        {
            get { return _length; }
        }

        public static implicit operator void*(UnsafeBuffer unsafeBuffer)
        {
            return unsafeBuffer.Address;
        }

        public static UnsafeBuffer Create(int size)
        {
            return Create(1, size, true);
        }

        public static UnsafeBuffer Create(int length, int sizeOfElement)
        {
            return Create(length, sizeOfElement, true);
        }

        public static UnsafeBuffer Create(int length, int sizeOfElement, bool aligned)
        {
            var buffer = new byte[length * sizeOfElement + (aligned ? 16 : 0)];
            return new UnsafeBuffer(buffer, length, aligned);
        }

        public static UnsafeBuffer Create(Array buffer)
        {
            return new UnsafeBuffer(buffer, buffer.Length, false);
        }

        //Allows for a oneliner for creating UnsafeBuffers
        public static UnsafeBuffer Create<T>(int length, int sizeOfElement, out T* ptr) where T : unmanaged
        {
            UnsafeBuffer buf = UnsafeBuffer.Create(length, sizeOfElement);
            ptr = (T*)buf;
            return buf;
        }

        public static UnsafeBuffer Create<T>(int length, out T* ptr) where T : unmanaged
        {
            UnsafeBuffer buf = UnsafeBuffer.Create(length, sizeof(T));
            ptr = (T*)buf;
            return buf;
        }

        public static UnsafeBuffer Create2D<T>(int height, int width, int sizeOfElement, out T*[] ptr) where T : unmanaged
        {
            UnsafeBuffer buf = UnsafeBuffer.Create(width * height, sizeOfElement);
            ptr = new T*[height];
            for (int i = 0; i < height; i++)
                ptr[i] = ((T*)buf) + (width * i);
            return buf;
        }

        public static UnsafeBuffer Create2D<T>(int height, int width, out T*[] ptr) where T : unmanaged
        {
            UnsafeBuffer buf = UnsafeBuffer.Create(width * height, sizeof(T));
            ptr = new T*[height];
            for (int i = 0; i < height; i++)
                ptr[i] = ((T*)buf) + (width * i);
            return buf;
        }
    }
}
