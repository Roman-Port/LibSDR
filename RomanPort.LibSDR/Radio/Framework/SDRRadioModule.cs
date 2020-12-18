using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Radio.Framework
{
    public unsafe abstract class SDRRadioModule : SDRMessageReceiver, IDisposable
    {
        /// <summary>
        /// If set to true, a new buffer will be cloned before we give this access to it
        /// </summary>
        public abstract bool IsDestructive { get; }

        /// <summary>
        /// Called when we're attached to a radio
        /// </summary>
        /// <param name="radio"></param>
        public abstract void OnAttached();

        /// <summary>
        /// Called when we should handle new samples
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="read"></param>
        public abstract void OnIncomingSamples(Complex* ptr, int read);

        //Public stuff
        public SDRRadio radio;

        //Internal misc
        private List<UnsafeBuffer> managedBuffers = new List<UnsafeBuffer>(); //Buffers from RequestBuffer

        /// <summary>
        /// Creates a new buffer of T, with the size of BufferSize, for this module
        /// </summary>
        /// <returns></returns>
        protected T* RequestBuffer<T>() where T : unmanaged
        {
            //Open
            UnsafeBuffer buffer = UnsafeBuffer.Create(radio.BufferSize, sizeof(T));

            //Add
            managedBuffers.Add(buffer);

            //Return pointer
            return (T*)buffer;
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
