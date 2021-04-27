using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.IO.NetIQ
{
    public enum NetIQOpcode : ushort
    {
        SERVER_INFO = 0,
        OPEN_STREAM = 1
    }
}
