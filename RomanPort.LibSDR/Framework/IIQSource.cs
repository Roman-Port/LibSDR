using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework
{
    public unsafe abstract class IIQSource : IDisposable
    {
        /// <summary>
        /// Requests that the source be opened. Response is the sample rate
        /// </summary>
        /// <param name="bufferLength">The maximum number of IQ samples to read.</param>
        public abstract float Open(int bufferLength);

        /// <summary>
        /// Called continuously on a loop and requests IQ samples. Response should be the number of IQ samples read
        /// </summary>
        /// <param name="iq"></param>
        /// <returns></returns>
        public abstract int Read(Complex* iq, int bufferLength);

        /// <summary>
        /// Closes the source, stops calling Read
        /// </summary>
        public abstract void Close();
        public abstract void Dispose();
    }
}
