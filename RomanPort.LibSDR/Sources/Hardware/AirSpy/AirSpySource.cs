using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RomanPort.LibSDR.Sources.Hardware.AirSpy
{
    public unsafe class AirSpySource : IHardwareSource
    {
        public override long CenterFrequency { get => device.CenterFrequency; set => device.CenterFrequency = (uint)value; }
        public override bool AutoGainEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override int ManualGainLevel { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override event HardwareSourceSamplesDroppedArgs OnSamplesDropped;

        private readonly ulong? radioSerialNumber;

        private AirSpyDevice device;
        private CircularBuffer<Complex> buffer;
        private ManualResetEvent readSignal = new ManualResetEvent(false);

        public uint[] CompatibleSampleRates { get; private set; }

        public AirSpySource()
        {
            radioSerialNumber = null;
        }

        public AirSpySource(ulong serialNumber)
        {
            radioSerialNumber = serialNumber;
        }

        public override void Close()
        {
            //Stop streaming
            device.StopStreaming();

            //Dispose buffers and device
            buffer.Dispose();
            device.Dispose();
        }

        public override void Dispose()
        {
            
        }

        public override float Open(int bufferLength)
        {
            //Find and open radio
            if (radioSerialNumber == null)
                device = AirSpyDevice.OpenFromDefault();
            else
                device = AirSpyDevice.OpenFromSerialNumber(radioSerialNumber.Value);

            //Get sample rates
            CompatibleSampleRates = device.GetSampleRates();

            //Configure
            device.SampleType = airspy_sample_type.AIRSPY_SAMPLE_FLOAT32_IQ;
            device.RfBias = false;
            device.Packing = false;
            device.SampleRate = CompatibleSampleRates[0];

            //Create buffer
            buffer = new CircularBuffer<Complex>(65536 * 2);

            //Subscribe to streaming event and begin streaming
            device.OnSamplesAvailable += Device_OnSamplesAvailable;
            device.BeginStreaming();

            return device.SampleRate;
        }

        private void Device_OnSamplesAvailable(Complex* samples, int count, ulong dropped)
        {
            //Add as many samples as we can to the buffer
            int written = buffer.Write(samples, count);

            //Signal to the reader thread
            readSignal.Set();

            //Update dropped count
            dropped += (uint)(count - written);

            //Send event if we dropped any
            if (dropped > 0)
                OnSamplesDropped?.Invoke((long)dropped);
        }

        public override unsafe int Read(Complex* iq, int bufferLength)
        {
            //Wait and reset
            //readSignal.WaitOne();
            //readSignal.Reset();

            //Read
            return buffer.Read(iq, bufferLength);
        }
    }
}
