using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.IO.USB
{
    public unsafe class UsbBuffer : IDisposable
    {
        public UsbBuffer(int length)
        {
            this.length = length;
            buffer = UnsafeBuffer.Create(length, out bytePtr);
        }

        public static UsbBuffer FromValue<T>(T value) where T : unmanaged
        {
            UsbBuffer buffer = new UsbBuffer(sizeof(T));
            Utils.Memcpy(buffer.AsPtr(), &value, sizeof(T));
            return buffer;
        }

        public static UsbBuffer FromValue<T>(T* value) where T : unmanaged
        {
            UsbBuffer buffer = new UsbBuffer(sizeof(T));
            Utils.Memcpy(buffer.AsPtr(), value, sizeof(T));
            return buffer;
        }

        private int length;
        private UnsafeBuffer buffer;
        private byte* bytePtr;

        public int ByteLength { get => length; }

        public byte* AsPtr()
        {
            return bytePtr;
        }

        public byte[] AsArray(out int offset)
        {
            offset = buffer.bufferAlignmentOffset;
            return buffer.buffer;
        }

        public void Dispose()
        {
            buffer.Dispose();
        }
    }
}
