using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Util;
using RomanPort.LibSDR.Radio.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Radio
{
    public unsafe abstract class SDRRadioModule : SDRMessageReceiver, IDisposable
    {
        /// <summary>
        /// Called when we're attached to a radio
        /// </summary>
        /// <param name="radio"></param>
        public abstract void Configure(int bufferSize, float inputSampleRate);

        /// <summary>
        /// Called when we should handle new samples
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="read"></param>
        public abstract void ProcessSamples(Complex* ptr, int read);

        //Internal misc
        private List<UnsafeBuffer> managedBuffers = new List<UnsafeBuffer>(); //Buffers from RequestBuffer

        /// <summary>
        /// Creates a new buffer of T, with the size of BufferSize, for this module
        /// </summary>
        /// <returns></returns>
        protected T* RequestBuffer<T>(int bufferSize) where T : unmanaged
        {
            //Open
            UnsafeBuffer buffer = UnsafeBuffer.Create(bufferSize, sizeof(T));

            //Add
            managedBuffers.Add(buffer);

            //Return pointer
            return (T*)buffer;
        }

        /// <summary>
        /// Requests a new buffer. Looks for the old buffer, and if it exists and is a different size, replaces it. Otherwise, it either uses the old buffer or creates a brand new one
        /// </summary>
        /// <returns></returns>
        protected T* RequestBuffer<T>(int bufferSize, T* oldBuffer) where T : unmanaged
        {
            //Check if we should search for the old buffer
            if(oldBuffer != null)
            {
                for(int i = 0; i<managedBuffers.Count; i++)
                {
                    //Get buffer
                    var b = managedBuffers[i];
                    
                    //Check if this is the desired buffer
                    if (b != oldBuffer)
                        continue;

                    //This is the same buffer. Check if it's the same size
                    if (b.Length == bufferSize)
                        return (T*)b;

                    //We'll be recreating this. Destroy this buffer
                    b.Dispose();
                    managedBuffers.RemoveAt(i);
                    break;
                }
            }

            //Create buffer
            return RequestBuffer<T>(bufferSize);
        }

        /// <summary>
        /// Cleans up after this module
        /// </summary>
        public virtual void Dispose()
        {
            //Dispose of all allocated buffers
            foreach (var b in managedBuffers)
                b.Dispose();
            managedBuffers.Clear();
        }
    }
}
