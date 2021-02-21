using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RomanPort.LibSDR.Components.General
{
    /// <summary>
    /// Allows for two feeds to process at once with a buffer being based around. UNTESTED!!
    /// </summary>
    public unsafe class ThreadPipe<T> : IDisposable where T : unmanaged
    {
        public ThreadPipe(int bufferSize)
        {
            threadABuffer = UnsafeBuffer.Create(bufferSize, out threadABufferPtr);
            threadBBuffer = UnsafeBuffer.Create(bufferSize, out threadBBufferPtr);

            threadASignal = new AutoResetEvent(true);
            threadBSignal = new AutoResetEvent(false);
        }

        private UnsafeBuffer threadABuffer;
        private T* threadABufferPtr;
        private UnsafeBuffer threadBBuffer;
        private T* threadBBufferPtr;

        private volatile int threadBufferUsage;
        private volatile bool inputBufferSwapped;
        private volatile bool outputBufferSwapped;

        private AutoResetEvent threadASignal;
        private AutoResetEvent threadBSignal;

        public T* BeginInput()
        {
            //Wait for us to be able to safely transfer to the other buffer. Only between after this and the end of this function are we guarenteed thread safety
            threadASignal.WaitOne();
            threadASignal.Reset();

            //Swap
            inputBufferSwapped = !inputBufferSwapped;

            //Return the now safe to use buffer
            if (!inputBufferSwapped)
                return threadBBufferPtr;
            else
                return threadABufferPtr;
        }

        public void EndInput(int count)
        {
            //Set usage so we can read it 
            threadBufferUsage = count;

            //Signal to the other thread that it can run
            threadBSignal.Set();
        }

        public T* BeginOutput(out int count)
        {
            //Wait for us to be able to safely transfer to the other buffer. Only between after this and the end of this function are we guarenteed thread safety
            threadBSignal.WaitOne();
            threadBSignal.Reset();

            //Set count from stored amount
            count = threadBufferUsage;

            //Swap
            outputBufferSwapped = !outputBufferSwapped;

            //Return the now safe to use buffer
            if (!outputBufferSwapped)
                return threadABufferPtr;
            else
                return threadBBufferPtr;
        }

        public void EndOutput()
        {
            //Signal to the other thread that it can run
            threadASignal.Set();
        }

        public void Dispose()
        {
            threadABuffer.Dispose();
            threadBBuffer.Dispose();
        }
    }
}
