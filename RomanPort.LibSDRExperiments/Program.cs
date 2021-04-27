using RomanPort.LibSDR.Components;
using RomanPort.LibSDR.Components.Analog.Primitive;
using RomanPort.LibSDR.Components.Digital;
using RomanPort.LibSDR.Components.Filters.Builders;
using RomanPort.LibSDR.Components.Filters.FIR;
using RomanPort.LibSDR.Components.Filters.IIR;
using RomanPort.LibSDR.Components.General;
using RomanPort.LibSDR.Components.IO.WAV;
using RomanPort.LibSDR.Demodulators.Analog.Video;
using RomanPort.LibSDR.Hardware.AirSpy;
using RomanPort.LibSDR.IO.USB.LibUSB;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace RomanPort.LibSDRExperiments
{
    unsafe class Program
    {
        static void Main(string[] args)
        {
            LibUSBProvider provider = new LibUSBProvider();
            var testd = provider.FindDevices(0x1d50, 0x60a1)[0];
            testd.OpenDevice();

            var b = new LibSDR.Components.IO.USB.UsbBuffer(512);
            int testr = testd.BulkTransfer(LibSDR.Components.IO.USB.UsbTransferDirection.READ, 0x01, b, 1000);


            AirSpyDevice device = AirSpyDevice.OpenDevice(provider);
            device.StartRx();
            device.SampleRate = 10000000;
            //device.SampleRate = 3000000;
            
            if(true)
            {
                device.CenterFrequency = 93700000;
                device.SetLinearGain(6 / 21f);
            } else
            {
                device.CenterFrequency = 144430000;
                device.SetLinearGain(2 / 21f);
            }

            test = new WavFileWriter(new FileStream("F:\\test.wav", FileMode.Create), (int)device.SampleRate, 2, LibSDR.Components.IO.SampleFormat.Short16, 1 << 16);

            UnsafeBuffer buffer = UnsafeBuffer.Create(1 << 16, out Complex* bufferPtr);
            while(true)
            {
                int read = device.Read(bufferPtr, 1 << 16, 1000);
                test.Write(bufferPtr, read);
                test.FinalizeFile();
            }
        }

        private static void A_OnTransferFailed(LibSDR.Components.IO.USB.IUsbAsyncTransfer transfer)
        {
            throw new NotImplementedException();
        }

        private static void A_OnTransferCompleted(LibSDR.Components.IO.USB.IUsbAsyncTransfer transfer, byte* bufferPtr, int count)
        {
            throw new NotImplementedException();
        }

        static WavFileWriter test;

        private static void Device_OnSamples(LibSDR.Components.Interfaces.IRadioDevice device, Complex* ptr, int count)
        {
            test.Write(ptr, count);
            test.FinalizeFile();
        }
    }
}
