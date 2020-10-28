using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework
{
    public unsafe abstract class IDemodulator : IDisposable
    {
        /// <summary>
        /// Demodulates raw IQ into the stereo audio data. Returns the number of audio samples written to each channel
        /// </summary>
        /// <param name="iq">Pointer to the raw IQ data. This should not be written to.</param>
        /// <param name="audioL">The left channel.</param>
        /// <param name="audioR">The right channel.</param>
        /// <param name="count">The number of IQ samples to process.</param>
        public abstract int DemodulateStereo(Complex* iq, float* audioL, float* audioR, int count);

        /// <summary>
        /// Demodulates raw IQ into the mono audio data. Returns the number of audio samples written
        /// </summary>
        /// <param name="iq"></param>
        /// <param name="audio"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public abstract int DemodulateMono(Complex* iq, float* audio, int count);

        /// <summary>
        /// Called when this is attached to a radio. The bufferSize is the maximum number of IQ samples this class will be expected to handle at once.
        /// </summary>
        /// <param name="bufferSize">Maximum number of IQ samples this will be expected to handle at once.</param>
        public abstract void OnAttached(int bufferSize);

        /// <summary>
        /// Called when the sample rate of the IQ input is changed.
        /// </summary>
        /// <param name="sampleRate"></param>
        public abstract void OnInputSampleRateChanged(float sampleRate);
        public abstract void Dispose();
    }
}
