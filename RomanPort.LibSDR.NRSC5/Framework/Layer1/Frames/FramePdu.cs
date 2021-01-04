using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.Layer1.Frames
{
    /// <summary>
    /// Contains audio
    /// </summary>
    public class FramePdu
    {
        public byte[] payload;
        public int program;
    }
}
