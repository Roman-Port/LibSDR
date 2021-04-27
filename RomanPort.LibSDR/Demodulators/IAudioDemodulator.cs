using RomanPort.LibSDR.Components;
using RomanPort.LibSDR.Components.Misc;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Demodulators
{
    public unsafe interface IAudioDemodulator
    {
        /// <summary>
        /// Sets up the demodulator. MUST be called before doing anything else. Returns the output sample rate, which will be greater than or equal to the target rate.
        /// </summary>
        /// <param name="bufferSize"></param>
        /// <param name="sampleRate"></param>
        /// <param name="targetOutputRate"></param>
        /// <returns></returns>
        float Configure(int bufferSize, float sampleRate, float targetOutputRate);
        int Demodulate(Complex* iq, float* audio, int count);
        int DemodulateStereo(Complex* iq, float* left, float* right, int count);
    }
}
