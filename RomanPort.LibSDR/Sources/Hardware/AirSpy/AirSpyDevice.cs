using RomanPort.LibSDR.Components;
using RomanPort.LibSDR.Sources.Hardware.AirSpy.Internal;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RomanPort.LibSDR.Sources.Hardware.AirSpy
{
    public class AirSpyDevice : IDisposable
    {
        public static AirSpyDevice OpenFromSerialNumber(ulong serial)
        {
            airspy_error error = NativeMethods.airspy_open_sn(out IntPtr device, serial);
            if (error != airspy_error.AIRSPY_SUCCESS)
                throw new HardwareNotFoundException();
            return new AirSpyDevice(device);
        }

        public static AirSpyDevice OpenFromDefault()
        {
            airspy_error error = NativeMethods.airspy_open(out IntPtr device);
            if (error != airspy_error.AIRSPY_SUCCESS)
                throw new HardwareNotFoundException();
            return new AirSpyDevice(device);
        }

        private unsafe static airspy_sample_block_cb_fn sampleHandler = new airspy_sample_block_cb_fn(StreamTransferCompleted);

        private readonly IntPtr device;

        private GCHandle gcHandle;

        public event AirSpySamplesAvailable OnSamplesAvailable;

        private AirSpyDevice(IntPtr device)
        {
            this.device = device;
            gcHandle = GCHandle.Alloc(this);
        }

        public byte LinearGain
        {
            get => _linearGain;
            set
            {
                airspy_error error = NativeMethods.airspy_set_linearity_gain(device, value);
                if (error != airspy_error.AIRSPY_SUCCESS)
                    throw new AirSpyException(error, "Error setting the gain.");
                _linearGain = value;
            }
        }
        private byte _linearGain;

        public uint CenterFrequency
        {
            get => _centerFreqeuncy;
            set
            {
                airspy_error error = NativeMethods.airspy_set_freq(device, value);
                if (error != airspy_error.AIRSPY_SUCCESS)
                    throw new AirSpyException(error, "Error setting the freqency.");
                _centerFreqeuncy = value;
            }
        }
        private uint _centerFreqeuncy;

        public uint SampleRate
        {
            get => _sampleRate;
            set {
                airspy_error error = NativeMethods.airspy_set_samplerate(device, value);
                if (error != airspy_error.AIRSPY_SUCCESS)
                    throw new AirSpyException(error, "Error setting the sample rate.");
                _sampleRate = value;
            }
        }
        private uint _sampleRate;

        public bool RfBias
        {
            get => _rfBias;
            set
            {
                airspy_error error = NativeMethods.airspy_set_rf_bias(device, value);
                if (error != airspy_error.AIRSPY_SUCCESS)
                    throw new AirSpyException(error, "Error setting the RF bias.");
                _rfBias = value;
            }
        }
        private bool _rfBias;

        public bool Packing
        {
            get => _packing;
            set
            {
                airspy_error error = NativeMethods.airspy_set_packing(device, value);
                if (error != airspy_error.AIRSPY_SUCCESS)
                    throw new AirSpyException(error, "Error setting the packing.");
                _packing = value;
            }
        }
        private bool _packing;

        public airspy_sample_type SampleType
        {
            get => _sampleType;
            set
            {
                airspy_error error = NativeMethods.airspy_set_sample_type(device, value);
                if (error != airspy_error.AIRSPY_SUCCESS)
                    throw new AirSpyException(error, "Error while setting sample rate type.");
                _sampleType = value;
            }
        }
        private airspy_sample_type _sampleType;

        public unsafe ulong SerialNumber
        {
            get
            {
                uint* buffer = stackalloc uint[6];
                airspy_error error = NativeMethods.airspy_board_partid_serialno_read(device, buffer);
                if (error != airspy_error.AIRSPY_SUCCESS)
                    throw new AirSpyException(error, "Error reading the serial number.");
                return (ulong)buffer[4] << 32 | (ulong)buffer[5];
            }
        }

        public unsafe uint[] GetSampleRates()
        {
            //Query for the number of samplerates
            uint len;
            airspy_error error = NativeMethods.airspy_get_samplerates(device, &len, 0);
            if (error != airspy_error.AIRSPY_SUCCESS)
                throw new AirSpyException(error, "Error while getting the number of sample rates.");

            //Query for the actual sample rates
            uint[] sampleRates = new uint[len];
            fixed(uint* sampleRatesPtr = sampleRates)
            {
                error = NativeMethods.airspy_get_samplerates(device, sampleRatesPtr, len);
                if (error != airspy_error.AIRSPY_SUCCESS)
                    throw new AirSpyException(error, "Error while getting the sample rates.");
            }

            return sampleRates;
        }

        public unsafe void BeginStreaming()
        {
            airspy_error error = NativeMethods.airspy_start_rx(device, sampleHandler, (IntPtr)gcHandle);
            if (error != airspy_error.AIRSPY_SUCCESS)
                throw new AirSpyException(error, "Failed to begin streaming.");
        }

        public void StopStreaming()
        {
            airspy_error error = NativeMethods.airspy_stop_rx(device);
            if (error != airspy_error.AIRSPY_SUCCESS)
                throw new AirSpyException(error, "Failed to stop streaming.");
        }

        private static unsafe int StreamTransferCompleted(airspy_transfer* data)
        {
            //Get our instance back by getting the handle from the context
            GCHandle gcHandle = GCHandle.FromIntPtr(data->ctx);
            AirSpyDevice ctx = (AirSpyDevice)gcHandle.Target;

            //Read parameters
            Complex* samples = (Complex*)data->samples;
            int count = data->sample_count;
            ulong dropped = data->dropped_samples;

            //Make sure this is valid
            if (data->sample_type != airspy_sample_type.AIRSPY_SAMPLE_FLOAT32_IQ)
                throw new Exception("Only Float32 IQ is supported currently.");

            //Handle
            ctx.OnSamplesAvailable?.Invoke(samples, count, dropped);

            //Return 0 to indicate a success
            return 0;
        }

        public void Dispose()
        {
            //Dispose device
            airspy_error error = NativeMethods.airspy_close(device);
            if (error != airspy_error.AIRSPY_SUCCESS)
                throw new AirSpyException(error, "Failed close device.");

            //Dispose GC handle
            gcHandle.Free();
        }
    }

    public unsafe delegate void AirSpySamplesAvailable(Complex* samples, int count, ulong dropped);
}
