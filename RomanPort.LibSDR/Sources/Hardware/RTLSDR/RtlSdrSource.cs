using RomanPort.LibSDR.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RomanPort.LibSDR.Sources.Hardware.RTLSDR
{
    public unsafe class RtlSdrSource : IHardwareSource
    {
        public readonly uint deviceIndex;
        public readonly uint sampleRate;
        public int droppedSamples;

        private RtlSdrDevice device;
        private CircularBuffer<Complex> circularBuffer;

        public RtlSdrSource(uint deviceIndex, uint sampleRate)
        {
            this.deviceIndex = deviceIndex;
            this.sampleRate = sampleRate;
        }
        
        public override float Open(int bufferLength)
        {
            //Open device
            device = new RtlSdrDevice(deviceIndex);
            device.SetSampleRate(sampleRate);
            device.SetCenterFreq(92500000);
            device.SetManualGain(0);

            //Open circular buffer
            circularBuffer = new CircularBuffer<Complex>(bufferLength * 4);

            //Begin streaming
            device.SamplesAvailable += Device_SamplesAvailable;
            device.BeginStreaming((uint)bufferLength);

            return sampleRate;
        }

        private void Device_SamplesAvailable(Complex* buffer, int length)
        {
            int written = circularBuffer.Write(buffer, length);
            droppedSamples += (length - written);
            if (written != length)
                Console.WriteLine("DROPPED " + droppedSamples);
        }

        public override unsafe int Read(Complex* iq, int bufferLength)
        {
            //Wait for data to become avalible
            while (circularBuffer == null || circularBuffer.GetAvailable() == 0) ;

            //Read
            return circularBuffer.Read(iq, bufferLength);
        }

        public override void Close()
        {
            //Close device
            device.Dispose();
            device = null;
        }

        public override void Dispose()
        {
            circularBuffer.Dispose();
        }
    }
}
