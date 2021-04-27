using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RomanPort.LibSDR.IO.USB.LibUSB.Native
{
    [StructLayout(LayoutKind.Sequential)]
    unsafe struct LibUSBTransfer
    {
        public IntPtr dev_handle;
        public byte flags;
        public byte endpoint;
        public LibUSBTransferType type;
        public uint timeout;
        public LibUSBTransferStatus status;
        public int length;
        public int actual_length;
        public IntPtr callback; //LibUSBNative.libusb_transfer_cb_fn
        public IntPtr user_data;
        public byte* buffer;
        public int num_iso_packets;
        //public LibUSBIsoPacketDescriptor* iso_packet_desc;
    }
}
