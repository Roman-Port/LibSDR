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
        private static Dictionary<uint, AnalogFilterConfig[]> analogFilters; //Key is sample rate
        private static readonly byte[] analogFilterNumbers1 = new byte[4]
        {
            (byte) 224,
            (byte) 128,
            (byte) 96,
            (byte) 0
        };
        private static readonly byte[] analogFilterNumbers2 = new byte[16]
        {
                (byte) 15,
                (byte) 14,
                (byte) 13,
                (byte) 12,
                (byte) 11,
                (byte) 10,
                (byte) 9,
                (byte) 8,
                (byte) 7,
                (byte) 6,
                (byte) 5,
                (byte) 4,
                (byte) 3,
                (byte) 2,
                (byte) 1,
                (byte) 0
        };

        private readonly IntPtr device;

        private GCHandle gcHandle;
        private int decimationIndex;

        public event AirSpySamplesAvailable OnSamplesAvailable;

        static AirSpyDevice()
        {
            //Create analog filters
            analogFilters = new Dictionary<uint, AnalogFilterConfig[]>();
            analogFilters.Add(10000000, new AnalogFilterConfig[]
            {
                new AnalogFilterConfig()
                {
                    lpf = 60,
                    hpf = 2,
                    shift = 0
                },
                new AnalogFilterConfig()
                {
                    lpf = 38,
                    hpf = 7,
                    shift = 1250000
                },
                new AnalogFilterConfig()
                {
                    lpf = 28,
                    hpf = 4,
                    shift = 2750000
                },
                new AnalogFilterConfig()
                {
                    lpf = 15,
                    hpf = 5,
                    shift = 3080000
                },
                new AnalogFilterConfig()
                {
                    lpf = 6,
                    hpf = 5,
                    shift = 3200000
                },
                new AnalogFilterConfig()
                {
                    lpf = 2,
                    hpf = 6,
                    shift = 3250000
                }
            });
            analogFilters.Add(2500000, new AnalogFilterConfig[]
            {
                new AnalogFilterConfig()
                {
                    lpf = 4,
                    hpf = 0,
                    shift = 0
                },
                new AnalogFilterConfig()
                {
                    lpf = 8,
                    hpf = 3,
                    shift = -280000
                },
                new AnalogFilterConfig()
                {
                    lpf = 5,
                    hpf = 5,
                    shift = -500000
                },
                new AnalogFilterConfig()
                {
                    lpf = 3,
                    hpf = 6,
                    shift = -550000
                }
            });
            analogFilters.Add(6000000, new AnalogFilterConfig[]
            {
                new AnalogFilterConfig()
                {
                    lpf = 33,
                    hpf = 2,
                    shift = 0
                },
                new AnalogFilterConfig()
                {
                    lpf = 26,
                    hpf = 2,
                    shift = 1000000
                },
                new AnalogFilterConfig()
                {
                    lpf = 17,
                    hpf = 4,
                    shift = 1100000
                },
                new AnalogFilterConfig()
                {
                    lpf = 7,
                    hpf = 5,
                    shift = 1200000
                },
                new AnalogFilterConfig()
                {
                    lpf = 2,
                    hpf = 6,
                    shift = 1250000
                }
            });
            analogFilters.Add(3000000, new AnalogFilterConfig[]
            {
                new AnalogFilterConfig()
                {
                    lpf = 8,
                    hpf = 0,
                    shift = 0
                },
                new AnalogFilterConfig()
                {
                    lpf = 7,
                    hpf = 4,
                    shift = -250000
                },
                new AnalogFilterConfig()
                {
                    lpf = 2,
                    hpf = 4,
                    shift = -300000
                }
            });
        }

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

        public uint DecimatedSampleRate { get => SampleRate >> decimationStages; }

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

        private int decimationStages = 0;

        public unsafe int DecimationStages
        {
            get => decimationStages;
            set
            {
                //Check range
                if (value < 0)
                    throw new ArgumentException("Decimation must be >= 0.");

                //Apply
                decimationStages = value;

                //Set conversion filter
                fixed (float* filter = ConversionFilters.FirKernels100dB[decimationStages])
                {
                    airspy_error error = NativeMethods.airspy_set_conversion_filter_float32(device, filter, ConversionFilters.FirKernels100dB[decimationStages].Length);
                    if (error != airspy_error.AIRSPY_SUCCESS)
                        throw new AirSpyException(error, "Error applying decimation filters to device.");
                }
            }
        }

        public int Decimation {
            get => 1 << decimationStages;
            set => DecimationStages = value >> 1;
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
            //Start
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

        private void ThrowIfOperationFailed(string text, airspy_error error)
        {
            if (error != airspy_error.AIRSPY_SUCCESS)
                throw new AirSpyException(error, text);
        }

        /// <summary>
        /// The AirSpy IF filter power, register 0x0A. Turning this off seems to kill the signal entirely.
        /// </summary>
        public bool RegFilterPower
        {
            get => ReadBoolFromRegister(0x0A, 7);
            set => WriteRegister10(value, RegFilterPowerLevel, RegFilterBandwidthFine);
        }

        /// <summary>
        /// The AirSpy IF filter power level, register 0x0A, high or low.
        /// </summary>
        public bool RegFilterPowerLevel
        {
            get => ReadBoolFromRegister(0x0A, 6);
            set => WriteRegister10(RegFilterPower, value, RegFilterBandwidthFine);
        }

        /// <summary>
        /// The AirSpy IF filter bandwidth (fine), register 0x0A, between 0-15. 15 is the narrowest, while 0 is the widest.
        /// </summary>
        public byte RegFilterBandwidthFine
        {
            get => ReadIntFromRegister(0x0A, 0, 0b1111);
            set => WriteRegister10(RegFilterPower, RegFilterPowerLevel, value);
        }

        /// <summary>
        /// The AirSpy IF filter bandwidth (coarse), register 0x0B.
        /// </summary>
        public AirSpyCoarseBandwidth RegFilterBandwidthCoarse
        {
            get => (AirSpyCoarseBandwidth)ReadIntFromRegister(0x0B, 5, 0b11);
            set => WriteRegister11(value, RegFilterHighPassCorner, RegUnknown);
        }

        /// <summary>
        /// The AirSpy IF filter high pass corner control, register 0x0B
        /// </summary>
        public int RegFilterHighPassCorner
        {
            get => ReadIntFromRegister(0x0B, 0, 0b1111);
            set => WriteRegister11(RegFilterBandwidthCoarse, value, RegUnknown);
        }

        /// <summary>
        /// An unknown, undocumented, value at 0x0B >> 7. Not sure what this does, but having it set to 0 seems to cause problems
        /// </summary>
        public bool RegUnknown
        {
            get => ReadBoolFromRegister(0x0B, 7);
            set => WriteRegister11(RegFilterBandwidthCoarse, RegFilterHighPassCorner, value);
        }

        /// <summary>
        /// Writes register 0x0A, filter control
        /// </summary>
        /// <param name="filterPower">On/off status. Turning this off seems to always kill the signal entirely.</param>
        /// <param name="filterHighPower">High/low power control.</param>
        /// <param name="filterBandwidthFine">Fine bandwidth control, 0-15</param>
        private void WriteRegister10(bool filterPower, bool filterHighPower, byte filterBandwidthFine)
        {
            //Validate
            if (filterBandwidthFine < 0 || filterBandwidthFine > 15)
                throw new ArgumentOutOfRangeException("BW out of range. Must be between 0-15.");

            //Create 
            int reg = 0;
            reg |= (filterPower ? 1 : 0) << 7;
            reg |= (filterHighPower ? 1 : 0) << 6;
            reg |= (filterHighPower ? 1 : 0) << 5;
            reg |= filterBandwidthFine << 0;

            //Write
            NativeMethods.airspy_r820t_write(device, 0x0A, (byte)reg);
        }

        /// <summary>
        /// Writes register 0x0B, filter control
        /// </summary>
        /// <param name="fineBandwidth">The fine bandwidth of the filter.</param>
        /// <param name="highPassCorner">High pass corner highest/lowest</param>
        /// <param name="unknown">Unknown, undocumented, at offset 7. Setting this to 0 causes problems though.</param>
        private void WriteRegister11(AirSpyCoarseBandwidth fineBandwidth, int highPassCorner, bool unknown)
        {
            //Validate
            if (highPassCorner < 0 || highPassCorner > 15)
                throw new ArgumentOutOfRangeException("highPassCorner out of range. Must be between 0-15.");

            //Create
            int reg = 0;
            reg |= (unknown ? 1 : 0) << 7;
            reg |= (int)fineBandwidth << 5;
            reg |= highPassCorner << 0;

            //Write
            NativeMethods.airspy_r820t_write(device, 0x0B, (byte)reg);
        }

        private bool ReadBoolFromRegister(byte register, byte shift)
        {
            ThrowIfOperationFailed("Failed to read register from device.", NativeMethods.airspy_r820t_read(device, register, out byte value));
            return ((value >> shift) & 1) == 1;
        }

        private byte ReadIntFromRegister(byte register, byte shift, byte mask)
        {
            ThrowIfOperationFailed("Failed to read register from device.", NativeMethods.airspy_r820t_read(device, register, out byte value));
            return (byte)((value >> shift) & mask);
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

            //We've already filtered, so go ahead and decimate ourselves
            int decimatedCount = 0;
            for(int i = 0; i<count; i++)
            {
                if (ctx.decimationIndex == 0)
                    samples[decimatedCount++] = samples[i];
                if (++ctx.decimationIndex == ctx.Decimation)
                    ctx.decimationIndex = 0;
            }

            //Handle
            ctx.OnSamplesAvailable?.Invoke(samples, decimatedCount, dropped);

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
