using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.IO.USB.LibUSB.Native
{
    public enum LibUSBTransferStatus : byte
    {
        LIBUSB_TRANSFER_COMPLETED,
        LIBUSB_TRANSFER_ERROR,
        LIBUSB_TRANSFER_TIMED_OUT,
        LIBUSB_TRANSFER_CANCELLED,
        LIBUSB_TRANSFER_STALL,
        LIBUSB_TRANSFER_NO_DEVICE,
        LIBUSB_TRANSFER_OVERFLOW
    }
}
