using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Interfaces
{
    public unsafe delegate void IRadioDevice_ConnectionAborted(IRadioDevice device, IRadioDevice_ConnectionAbortedReason reason);

    public interface IRadioDevice
    {
        long CenterFrequency { get; set; }
        long SampleRate { get; set; }
        long[] SupportedSampleRates { get; }

        /// <summary>
        /// Called when the device is lost or otherwise stops streaming
        /// </summary>
        event IRadioDevice_ConnectionAborted OnAborted;

        /// <summary>
        /// Hangs while waiting for new samples. Transfers those samples into the buffer.
        /// </summary>
        /// <param name="ptr">The destination</param>
        /// <param name="count">Max to copy.</param>
        /// <param name="timeout">Timeout in milliseconds. If this is hit, 0 is returned.</param>
        /// <returns></returns>
        unsafe int Read(Complex* ptr, int count, int timeout);

        /// <summary>
        /// Sets the gain in a more abstracted value. Input is between 0-1, 1 being max gain.
        /// </summary>
        /// <param name="value">Range between 0-1 to use.</param>
        void SetLinearGain(float value);

        /// <summary>
        /// Starts up the radio recieving
        /// </summary>
        void StartRx();

        /// <summary>
        /// Stops reciving
        /// </summary>
        void StopRx();
    }

    public enum IRadioDevice_ConnectionAbortedReason
    {
        ERR_OTHER = -1,
        NORMAL = 0,
        ERR_LOST
    }
}
