using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.IO.USB.LibUSB
{
    public class LibUSBException : Exception
    {
        public int UsbErrorCode { get => error; }
        public unsafe string UsbError { get => GetUsbError(error); }
        public unsafe string UsbErrorText { get => GetUsbErrorText(error); }

        private int error;

        public LibUSBException(int error) : base($"LibUSB encountered error: {GetUsbError(error)} ({error}) - {GetUsbErrorText(error)}")
        {
            this.error = error;
        }

        private unsafe static string GetUsbError(int code)
        {
            return LibUSBNative.UtilReadNullTerminatedString(LibUSBNative.libusb_error_name(code));
        }

        private unsafe static string GetUsbErrorText(int code)
        {
            return LibUSBNative.UtilReadNullTerminatedString(LibUSBNative.libusb_strerror(code));
        }
    }
}
