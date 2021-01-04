using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.Layer1.Frames
{
    //https://www.nrscstandards.org/standards-and-guidelines/documents/standards/nrsc-5-d/reference-docs/1019s.pdf
    public class FrameAas
    {
        public ushort port; //distinguishes data packets from different services
        public ushort sequence; //maintains packet order, detects missed packets
        public byte[] payload;
    }
}
