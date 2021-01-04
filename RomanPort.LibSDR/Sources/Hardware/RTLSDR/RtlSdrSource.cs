using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Util;
using RomanPort.LibSDR.Sources.Hardware.RTLSDR.Internal;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace RomanPort.LibSDR.Sources.Hardware.RTLSDR
{
    public unsafe class RtlSdrSource : IHardwareSource, ISource
    {
        private readonly uint deviceIndex;

        //Misc
        private IntPtr device;
        private GCHandle gcHandle;
        private Thread workerThread;
        private int bufferLength;

        //Buffer for converting outgoing data
        private UnsafeBuffer buffer;
        private Complex* bufferPtr;

        //DC removers
        private DcRemover dcRemoverI;
        private DcRemover dcRemoverQ;

        //Internal stuff, don't touch
        private bool isStreaming;
        private uint sampleRate;
        private uint centerFreq;
        private int gainLevel;
        private bool gainAuto;

        private const int RTL_SUCCESS_OPCODE = 0;
        public static readonly uint[] SUPPORTED_SAMPLE_RATES = new uint[]
        {
            3200000,
            2800000,
            2400000,
            2048000,
            1920000,
            1800000,
            1400000,
            1024000,
            900001,
            250000
        };

        public event HardwareSourceSamplesDroppedArgs OnSamplesDropped;
        public event SamplesAvailableEventArgs OnSamplesAvailable;
        public event SampleRateChangedEventArgs OnSampleRateChanged;

        public long CenterFrequency {
            get => centerFreq;
            set {
                //Set on device
                int opcode = NativeMethods.rtlsdr_set_center_freq(device, (uint)value);
                if (opcode != RTL_SUCCESS_OPCODE)
                    throw new RtlDeviceErrorException(opcode);

                //Set locally
                centerFreq = (uint)value;
            }
        }

        public bool AutoGainEnabled {
            get => gainAuto;
            set {
                //Set on device
                int opcode = NativeMethods.rtlsdr_set_tuner_gain_mode(device, value ? 0 : 1);
                if (opcode != RTL_SUCCESS_OPCODE)
                    throw new RtlDeviceErrorException(opcode);

                //Set locally
                gainAuto = value;
            }
        }

        public int ManualGainLevel {
            get => gainLevel;
            set
            {
                //Change gain mode
                AutoGainEnabled = false;

                //Set on device
                int opcode = NativeMethods.rtlsdr_set_tuner_gain(device, value);
                if (opcode != RTL_SUCCESS_OPCODE)
                    throw new RtlDeviceErrorException(opcode);

                //Set locally
                gainLevel = value;
            }
        }

        public float SampleRate
        {
            get => sampleRate;
            set
            {
                //Set on device
                int opcode = NativeMethods.rtlsdr_set_sample_rate(device, (uint)value);
                if (opcode != RTL_SUCCESS_OPCODE)
                    throw new RtlDeviceErrorException(opcode);

                //Set locally
                sampleRate = (uint)value;

                //Send events
                OnSampleRateChanged?.Invoke(value);
            }
        }

        public int[] TunerGains {
            get
            {
                //Check
                if (device == null)
                    throw new Exception("Device not yet opened.");

                //Fetch
                int supportedGainsCount = NativeMethods.rtlsdr_get_tuner_gains(device, null);
                int[] supportedGains = new int[supportedGainsCount];
                NativeMethods.rtlsdr_get_tuner_gains(device, supportedGains);
                return supportedGains;
            }
        }

        public string TunerName
        {
            get
            {
                return NativeMethods.rtlsdr_get_device_name(deviceIndex);
            }
        }

        public uint DeviceIndex { get => deviceIndex; }

        public long TotalDroppedSamples => 0;

        public RtlSdrSource(uint deviceIndex, uint sampleRate)
        {
            this.deviceIndex = deviceIndex;
            this.sampleRate = sampleRate;
        }

        public static string[] GetConnectedDevices()
        {
            //Get count
            uint count = NativeMethods.rtlsdr_get_device_count();

            //Get names
            string[] devices = new string[count];
            for (uint i = 0; i < count; i++)
                devices[i] = NativeMethods.rtlsdr_get_device_name(i);

            return devices;
        }

        public void Open(int bufferSize)
        {
            //Create buffers
            bufferLength = bufferSize;
            buffer = UnsafeBuffer.Create(bufferLength, sizeof(Complex));
            bufferPtr = (Complex*)buffer;

            //Create DC removers
            dcRemoverI = new DcRemover();
            dcRemoverI.Init();
            dcRemoverQ = new DcRemover();
            dcRemoverQ.Init();

            //Open
            if (NativeMethods.rtlsdr_open(out device, deviceIndex) != RTL_SUCCESS_OPCODE)
                throw new HardwareNotFoundException();

            //Create handle
            gcHandle = GCHandle.Alloc(this);

            //Initialize the device
            SampleRate = sampleRate;
            AutoGainEnabled = false;
            ManualGainLevel = 0;
        }

        public void BeginStreaming()
        {
            //Check flag
            if (isStreaming)
                return;
            
            //Clear buffer to prepare for reading
            ResetDeviceBuffer();

            //Open device for streaming
            workerThread = new Thread(() =>
            {
                NativeMethods.rtlsdr_read_async(device, RtlSdrSamplesAvailable, (IntPtr)gcHandle, 0, (uint)(bufferLength * 2));
                isStreaming = false;
            });
            workerThread.IsBackground = true;
            workerThread.Name = "RTL-SDR Streaming Thread";
            workerThread.Start();

            //Update state
            isStreaming = true;
        }

        public void EndStreaming()
        {
            //Check flag
            if (!isStreaming)
                return;

            //Stop device
            int opcode = NativeMethods.rtlsdr_cancel_async(device);
            if (opcode != RTL_SUCCESS_OPCODE)
                throw new RtlDeviceErrorException(opcode);

            //Update state
            isStreaming = false;
        }

        public void Close()
        {
            //Stop streaming if we are
            EndStreaming();

            //Close device
            int opcode = NativeMethods.rtlsdr_close(device);
            if (opcode != RTL_SUCCESS_OPCODE)
                throw new RtlDeviceErrorException(opcode);

            //Dispose of buffers
            buffer.Dispose();

            //Dispose of GC handle
            gcHandle.Free();
        }

        private void ResetDeviceBuffer()
        {
            //Reset device buffer
            if (NativeMethods.rtlsdr_reset_buffer(device) != RTL_SUCCESS_OPCODE)
                throw new Exception("Failed to access RTL device.");
        }

        private static void RtlSdrSamplesAvailable(byte* buf, uint len, IntPtr ctx)
        {
            //Get handle
            var gcHandle = GCHandle.FromIntPtr(ctx);
            if (!gcHandle.IsAllocated)
            {
                return;
            }

            //Get instance
            var instance = (RtlSdrSource)gcHandle.Target;

            //Write samples to instance
            instance.SamplesMadeAvailable(buf, len);
        }

        private void SamplesMadeAvailable(byte* buf, uint len)
        {
            //Convert
            int samplesRead = (int)(len / 2);
            for (var i = 0; i < samplesRead; i++)
            {
                bufferPtr[i].Imag = ConversionLookupTable.LOOKUP_INT8[buf[(i * 2) + 0]];
                bufferPtr[i].Real = ConversionLookupTable.LOOKUP_INT8[buf[(i * 2) + 1]];
            }

            //Clean up
            dcRemoverI.ProcessInterleaved((float*)bufferPtr, samplesRead);
            dcRemoverQ.ProcessInterleaved(((float*)bufferPtr) + 1, samplesRead);

            //Send events, but limit the amount sent to the buffer size
            int offset = 0;
            while (samplesRead > 0)
            {
                int writable = Math.Min(bufferLength, samplesRead);
                OnSamplesAvailable?.Invoke(bufferPtr + offset, writable);
                samplesRead -= writable;
                offset += writable;
            }
        }
    }
}
