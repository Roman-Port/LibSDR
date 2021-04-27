using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.IO.USB
{
    public interface IUsbProvider
    {
        IUsbDevice[] FindDevices(ushort vid, ushort pid);
    }
}
