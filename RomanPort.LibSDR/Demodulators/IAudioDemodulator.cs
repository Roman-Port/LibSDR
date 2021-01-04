using RomanPort.LibSDR.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Demodulators
{
    public unsafe interface IAudioDemodulator
    {
        void Configure(int bufferSize, float sampleRate);
        void Demodulate(Complex* iq, float* audio, int count);
        void DemodulateStereo(Complex* iq, float* left, float* right, int count);
    }
}
