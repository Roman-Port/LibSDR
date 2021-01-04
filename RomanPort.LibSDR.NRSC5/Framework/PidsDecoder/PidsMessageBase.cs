using RomanPort.LibSDR.NRSC5.Framework.Layer1.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.PidsDecoder
{
    public abstract class PidsMessageBase
    {
        public abstract void Decode(PidsReader reader);
        public abstract void ProcessParser(PidsParser parser, PidsMessageBaseContext context);
    }
}
