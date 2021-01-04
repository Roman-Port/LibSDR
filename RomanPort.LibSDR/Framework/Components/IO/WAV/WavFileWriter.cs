using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RomanPort.LibSDR.Framework.Components.IO.WAV
{
    public class WavFileWriter
    {
        private Stream underlyingStream;

        public WavFileWriter(Stream underlyingStream)
        {
            this.underlyingStream = underlyingStream;
        }
    }
}
