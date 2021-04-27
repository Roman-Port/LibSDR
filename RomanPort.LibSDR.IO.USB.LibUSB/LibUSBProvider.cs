using RomanPort.LibSDR.Components.IO.USB;
using RomanPort.LibSDR.IO.USB.LibUSB.Native;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RomanPort.LibSDR.IO.USB.LibUSB
{
    public unsafe class LibUSBProvider : IUsbProvider
    {
        public LibUSBProvider()
        {
            //Create the LibUSB instance
            LibUSBNative.ThrowIfError(LibUSBNative.libusb_init(ref ctx));

            //Create event thread
            eventThread = new Thread(EventLoop);
            eventThread.Name = "LibUSB Provider Event Loop";
            eventThread.IsBackground = true;
            eventThread.Start();
        }

        private IntPtr ctx;
        private Thread eventThread;
        
        public IUsbDevice[] FindDevices(ushort vid, ushort pid)
        {
            //Get USB devices
            IntPtr devicesRef = IntPtr.Zero;
            int count = LibUSBNative.libusb_get_device_list(ctx, ref devicesRef);
            IntPtr* devices = (IntPtr*)devicesRef.ToPointer();

            //Loop through devices
            List<LibUSBDevice> found = new List<LibUSBDevice>();
            for (int i = 0; i<count; i++)
            {
                //Get pointer to the device
                IntPtr device = devices[i];

                //Get info
                LibUSBDeviceDescriptor info;
                if (LibUSBNative.libusb_get_device_descriptor(device, &info) != 0)
                    continue;

                //Check if this is a match
                if (vid != info.idVendor || pid != info.idProduct)
                    continue;

                //Construct object
                found.Add(new LibUSBDevice(device, info));
            }

            return found.ToArray();
        }

        private void EventLoop()
        {
            while (true)
            {
                LibUSBNative.libusb_handle_events(ctx);
            }
        }
    }
}
