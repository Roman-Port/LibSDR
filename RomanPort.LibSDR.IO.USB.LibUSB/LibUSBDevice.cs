using RomanPort.LibSDR.Components.IO.USB;
using RomanPort.LibSDR.IO.USB.LibUSB.Native;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.IO.USB.LibUSB
{
    public unsafe class LibUSBDevice : IUsbDevice
    {
        internal LibUSBDevice(IntPtr device, LibUSBDeviceDescriptor info)
        {
            this.device = device;
            this.info = info;
        }

        private IntPtr device;
        private LibUSBDeviceDescriptor info;
        private bool isOpened;

        private IntPtr handle;

        private const byte LIBUSB_ENDPOINT_OUT = 0x00;
        private const byte LIBUSB_ENDPOINT_IN = 0x80;

        //libusb_request_type
        private const byte LIBUSB_REQUEST_TYPE_STANDARD = (0x00 << 5);
        private const byte LIBUSB_REQUEST_TYPE_CLASS = (0x01 << 5);
        private const byte LIBUSB_REQUEST_TYPE_VENDOR = (0x02 << 5);
        private const byte LIBUSB_REQUEST_TYPE_RESERVED = (0x03 << 5);

        //libusb_request_recipient 
        private const byte LIBUSB_RECIPIENT_DEVICE = 0x00;
        private const byte LIBUSB_RECIPIENT_INTERFACE = 0x01;
        private const byte LIBUSB_RECIPIENT_ENDPOINT = 0x02;
        private const byte LIBUSB_RECIPIENT_OTHER = 0x03;

        public string DescriptorManufacturer => ReadStringDescriptor(info.iManufacturer);

        public string DescriptorProduct => ReadStringDescriptor(info.iProduct);

        public string DescriptorSerialNumber => ReadStringDescriptor(info.iSerialNumber);

        public bool OpenDevice()
        {
            //If it's already opened, return true
            if (isOpened)
                return true;

            //Request the device to be opened
            handle = IntPtr.Zero;
            isOpened = LibUSBNative.libusb_open(device, ref handle) == 0;
            if (!isOpened)
                return false;

            //Set config
            LibUSBNative.ThrowIfError(LibUSBNative.libusb_set_configuration(handle, 1));
            LibUSBNative.ThrowIfError(LibUSBNative.libusb_claim_interface(handle, 0));

            return true;
        }

        public int ControlTransferRead(byte fieldRequest, ushort fieldValue, ushort fieldIndex, UsbBuffer buffer, uint timeout)
        {
            byte flags = LIBUSB_ENDPOINT_IN | LIBUSB_REQUEST_TYPE_VENDOR | LIBUSB_RECIPIENT_DEVICE;
            return ControlTransfer(flags, fieldRequest, fieldValue, fieldIndex, buffer, timeout);
        }

        public int ControlTransferWrite(byte fieldRequest, ushort fieldValue, ushort fieldIndex, UsbBuffer buffer, uint timeout)
        {
            byte flags = LIBUSB_ENDPOINT_OUT | LIBUSB_REQUEST_TYPE_VENDOR | LIBUSB_RECIPIENT_DEVICE;
            return ControlTransfer(flags, fieldRequest, fieldValue, fieldIndex, buffer, timeout);
        }

        public int BulkTransfer(UsbTransferDirection direction, byte endpoint, UsbBuffer buffer, int timeout)
        {
            int transferred;
            LibUSBNative.ThrowIfError(LibUSBNative.libusb_bulk_transfer(handle, (byte)(endpoint | (byte)direction), buffer.AsPtr(), buffer.ByteLength, &transferred, (uint)timeout));
            return transferred;
        }

        public IUsbAsyncTransfer OpenBulkTransfer(byte endpoint, int bufferSize)
        {
            return new LibUSBAsyncTransfer(handle, endpoint, bufferSize);
        }

        private int ControlTransfer(byte flags, byte fieldRequest, ushort fieldValue, ushort fieldIndex, UsbBuffer buffer, uint timeout)
        {
            //Apply
            int code;
            if(buffer != null)
                code = LibUSBNative.libusb_control_transfer(handle, flags, fieldRequest, fieldValue, fieldIndex, buffer.AsPtr(), (ushort)buffer.ByteLength, timeout);
            else
                code = LibUSBNative.libusb_control_transfer(handle, flags, fieldRequest, fieldValue, fieldIndex, null, 0, timeout);

            //Valiate
            if (code < 0)
                throw new Exception("Unknown USB transfer write error.");

            return code;
        }

        private void ThrowIfUnopened()
        {
            if (!isOpened)
                throw new Exception("Device must be opened before calling this function.");
        }

        private string ReadStringDescriptor(byte index, int length = 255)
        {
            //Make sure it's open
            ThrowIfUnopened();
            
            //Create buffer
            byte[] buffer = new byte[length];

            //Get data
            fixed (byte* bufferPtr = buffer)
                length = LibUSBNative.libusb_get_string_descriptor_ascii(handle, index, bufferPtr, length);

            //Read
            return Encoding.ASCII.GetString(buffer, 0, length);
        }
    }
}
