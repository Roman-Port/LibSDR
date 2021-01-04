using RomanPort.LibSDR.NRSC5.Framework.Layer1.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.Layer1.Frames
{
    public class FramePids
    {
        internal FramePids(byte[] bits)
        {
            this.bits = bits;
        }

        public byte[] bits;

        public PidsReader GetReader()
        {
            return new PidsReader(this);
        }
    }
}
