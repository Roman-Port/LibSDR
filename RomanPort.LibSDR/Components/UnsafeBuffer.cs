﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RomanPort.LibSDR.Components
{
    public unsafe class UnsafeBuffer : IDisposable
    {
        private readonly GCHandle handle;
        private readonly void* ptr;
        private readonly int length;
        private readonly int sizeOfElement;

        public readonly byte[] buffer;

        private UnsafeBuffer(int length, int sizeOfElement)
        {
            //Set
            this.length = length;
            this.sizeOfElement = sizeOfElement;
            
            //Create buffer
            buffer = new byte[length * sizeOfElement + 16];

            //Get handle and aligned pointer
            handle = GCHandle.Alloc(this.buffer, GCHandleType.Pinned);
            ptr = (void*)(((long)handle.AddrOfPinnedObject() + 15) & ~15);
        }

        ~UnsafeBuffer()
        {
            Dispose();
        }

        public void Dispose()
        {
            handle.Free();
            GC.SuppressFinalize(this);
        }

        public void* Address
        {
            get { return ptr; }
        }

        /// <summary>
        /// Object count, not total length
        /// </summary>
        public int Length
        {
            get { return length; }
        }

        public static implicit operator void*(UnsafeBuffer unsafeBuffer)
        {
            return unsafeBuffer.Address;
        }

        public static UnsafeBuffer Create(int length, int sizeOfElement)
        {
            return new UnsafeBuffer(length, sizeOfElement);
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
    }
}
