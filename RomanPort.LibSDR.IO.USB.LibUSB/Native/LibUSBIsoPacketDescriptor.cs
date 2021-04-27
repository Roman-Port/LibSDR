using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.IO.USB.LibUSB.Native
{
    unsafe struct LibUSBIsoPacketDescriptor
    {
        public uint length;
        public uint actual_length;
        public LibUSBTransferStatus status;
    }
}
