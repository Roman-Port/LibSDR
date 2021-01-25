using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Util;
using RomanPort.LibSDR.Sources.Hardware.AirSpy.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RomanPort.LibSDR.Sources.Hardware.AirSpy
{
    public unsafe class AirSpySource : IHardwareSource
    {
        public long CenterFrequency { get => device.CenterFrequency; set => device.CenterFrequency = (uint)value; }
        public bool AutoGainEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int ManualGainLevel { get => device.LinearGain; set => device.LinearGain = (byte)value; }
        public float SampleRate
        {
            get => sampleRate;
            set
            {
                device.SampleRate = (uint)value;
                sampleRate = (uint)value;
                OnSampleRateChanged?.Invoke(value);
            }
        }
        public long TotalDroppedSamples { get => droppedSamples; }

        public event SamplesAvailableEventArgs OnSamplesAvailable;
        public event SampleRateChangedEventArgs OnSampleRateChanged;
        public event HardwareSourceSamplesDroppedArgs OnSamplesDropped;

        //Initial stuff
        private ulong? initialRequestedSerialNumber;
        private uint initialRequestedSampleRate;

        //Misc
        private AirSpyDevice device;
        private bool isStreaming;
        private long droppedSamples;
        private uint sampleRate;

        public AirSpySource(uint sampleRate)
        {
            initialRequestedSerialNumber = null;
            initialRequestedSampleRate = sampleRate;
        }

        public AirSpySource(uint sampleRate, ulong serialNumber)
        {
            initialRequestedSerialNumber = serialNumber;
            initialRequestedSampleRate = sampleRate;
        }

        public void Open(int bufferSize)
        {
            //Open device from either the serial or the default
            if (initialRequestedSerialNumber.HasValue)
                device = AirSpyDevice.OpenFromSerialNumber(initialRequestedSerialNumber.Value);
            else
                device = AirSpyDevice.OpenFromDefault();

            //Configure
            device.SampleType = airspy_sample_type.AIRSPY_SAMPLE_FLOAT32_IQ;
            device.OnSamplesAvailable += Device_OnSamplesAvailable;
            SampleRate = initialRequestedSampleRate;
        }

        private void Device_OnSamplesAvailable(Complex* samples, int count, ulong dropped)
        {
            OnSamplesAvailable?.Invoke(samples, count);
            droppedSamples += (long)dropped;
            if (dropped > 0)
                OnSamplesDropped?.Invoke((long)dropped);
        }

        public void Close()
        {
            //Stop streaming
            EndStreaming();

            //Dispose of device
            device.Dispose();
        }

        public void BeginStreaming()
        {
            //Check flag
            if (isStreaming)
                return;
            
            //Start
            device.BeginStreaming();

            //Update
            isStreaming = true;
        }

        public void EndStreaming()
        {
            //Check flag
            if (!isStreaming)
                return;

            //Start
            device.StopStreaming();

            //Update
            isStreaming = false;
        }
    }
}
