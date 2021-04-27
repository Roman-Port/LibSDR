using RomanPort.LibSDR.IO.USB.LibUSB.Native;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RomanPort.LibSDR.IO.USB.LibUSB
{
    unsafe class LibUSBNative
    {
        private const string LIBUSB_DLL = "LibUSB_x64.dll";

        public delegate void libusb_transfer_cb_fn(LibUSBTransfer* transfer);

        [DllImport(LIBUSB_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte* libusb_error_name(int code);

        [DllImport(LIBUSB_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte* libusb_strerror(int code);

        [DllImport(LIBUSB_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int libusb_init(ref IntPtr ctx);

        [DllImport(LIBUSB_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int libusb_get_device_list(IntPtr ctx, ref IntPtr devices);
        
        [DllImport(LIBUSB_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int libusb_get_device_descriptor(IntPtr device, LibUSBDeviceDescriptor* descriptor);

        [DllImport(LIBUSB_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int libusb_open(IntPtr device, ref IntPtr handle);
        
        [DllImport(LIBUSB_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int libusb_get_string_descriptor_ascii(IntPtr handle, byte descriptor, byte* data, int length);
        
        [DllImport(LIBUSB_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int libusb_set_configuration(IntPtr handle, int option);
        
        [DllImport(LIBUSB_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int libusb_claim_interface(IntPtr handle, int option);
        
        [DllImport(LIBUSB_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int libusb_control_transfer(IntPtr handle, byte request_type, byte bRequest, ushort wValue, ushort wIndex, byte* data, ushort wLength, uint timeout);

        [DllImport(LIBUSB_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int libusb_bulk_transfer(IntPtr handle, byte endpoint, byte* data, int length, int* transferred, uint timeout);

        [DllImport(LIBUSB_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern LibUSBTransfer* libusb_alloc_transfer(int isoPackeets);
        
        [DllImport(LIBUSB_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int libusb_submit_transfer(LibUSBTransfer* transfer);
        
        [DllImport(LIBUSB_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int libusb_handle_events_timeout_completed(IntPtr ctx, LibUSBTimeval* interval, IntPtr completed);

        [DllImport(LIBUSB_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int libusb_handle_events(IntPtr ctx);

        public static void ThrowIfError(int code)
        {
            if (code != 0)
                throw new LibUSBException(code);
        }

        public static string UtilReadNullTerminatedString(byte* ptr)
        {
            int length = 0;
            while (ptr[length] != 0x00)
                length++;
            return Encoding.ASCII.GetString(ptr, length);
        }
    }
}
