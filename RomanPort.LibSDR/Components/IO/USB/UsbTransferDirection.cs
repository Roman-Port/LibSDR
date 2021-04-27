using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.IO.USB
{
    public enum UsbTransferDirection : byte
    {
        WRITE = 0,
        READ = 128
    }
}
