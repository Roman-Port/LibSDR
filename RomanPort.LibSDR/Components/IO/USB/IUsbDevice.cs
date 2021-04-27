using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.IO.USB
{
    public interface IUsbDevice
    {
        bool OpenDevice();
        int ControlTransferRead(byte fieldRequest, ushort fieldValue, ushort fieldIndex, UsbBuffer buffer, uint timeout);
        int ControlTransferWrite(byte fieldRequest, ushort fieldValue, ushort fieldIndex, UsbBuffer buffer, uint timeout);
        int BulkTransfer(UsbTransferDirection direction, byte endpoint, UsbBuffer buffer, int timeout);
        IUsbAsyncTransfer OpenBulkTransfer(byte endpoint, int bufferSize);
        string DescriptorManufacturer { get; }
        string DescriptorProduct { get; }
        string DescriptorSerialNumber { get; }
    }
}
