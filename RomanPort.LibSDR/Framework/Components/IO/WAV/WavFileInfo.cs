using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework.Components.IO.WAV
{
    public struct WavFileInfo
    {
        public short channels;
        public int sampleRate;
        public short bitsPerSample;
    }
}
