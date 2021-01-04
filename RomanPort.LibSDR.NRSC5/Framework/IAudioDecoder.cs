using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework
{
    public interface IAudioDecoder
    {
        void OpenDecoder(long samplerate);
        unsafe int Process(byte* compressed, int compressedCount, short* outBuffer, int outBufferCount);
        void CloseDecoder();
    }
}
