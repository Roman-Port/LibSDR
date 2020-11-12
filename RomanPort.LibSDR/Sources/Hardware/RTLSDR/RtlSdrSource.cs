using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Exceptions;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace RomanPort.LibSDR.Sources.Hardware.RTLSDR
{
    public unsafe class RtlSdrSource : IHardwareSource
    {
        public readonly uint deviceIndex;
        public readonly uint sampleRate;
        public readonly int internalBufferMultiplier;

        public IntPtr device;
        public int[] supportedGains;
        public string deviceName;
        private GCHandle gcHandle;
        private Thread workerThread;
        private EventWaitHandle waitHandle;

        public int droppedSamples;

        private int incomingBufferLength;

        //Buffer for incoming data
        private UnsafeBuffer incomingBuffer;
        private byte* incomingBufferPtr;

        //Buffer for converting outgoing data
        private UnsafeBuffer outgoingBuffer;
        private byte* outgoingBufferPtr;

        //Circular buffer for threading
        private CircularBuffer<byte> buffer;

        //DC removers
        private DcRemover dcRemoverI;
        private DcRemover dcRemoverQ;

        //Buffer for converting from the byte input from the RTL-SDR to a float input the rest of this program uses
        private static readonly UnsafeBuffer _lutBuffer = UnsafeBuffer.Create(256, sizeof(float));
        private static readonly float* _lutPtr;

        private const int RTL_SUCCESS_OPCODE = 0;

        private uint centerFreq;
        private int gainLevel;

        public override long CenterFrequency {
            get => centerFreq;
            set {
                SetCenterFreq((uint)value);
                centerFreq = (uint)value;
            }
        }
        public override bool AutoGainEnabled {
            get => gainLevel == -1;
            set {
                gainLevel = -1;
                SetAutomaticGain();
            }
        }
        public override int ManualGainLevel {
            get => gainLevel;
            set
            {
                gainLevel = value;
                SetManualGain(gainLevel);
            }
        }

        public RtlSdrSource(uint deviceIndex, uint sampleRate, int internalBufferMultiplier = 2)
        {
            this.deviceIndex = deviceIndex;
            this.sampleRate = sampleRate;
            this.internalBufferMultiplier = internalBufferMultiplier;
        }

        static RtlSdrSource()
        {
            _lutPtr = (float*)_lutBuffer;

            const float scale = 1.0f / 127.0f;
            for (var i = 0; i < 256; i++)
            {
                _lutPtr[i] = (i - 128) * scale;
            }
        }

        public override float Open(int bufferLength)
        {
            //Create buffers
            incomingBufferLength = bufferLength;
            incomingBuffer = UnsafeBuffer.Create(bufferLength * 2, sizeof(byte));
            incomingBufferPtr = (byte*)incomingBuffer;
            outgoingBuffer = UnsafeBuffer.Create(bufferLength * 2, sizeof(byte));
            outgoingBufferPtr = (byte*)outgoingBuffer;
            buffer = new CircularBuffer<byte>(bufferLength * 4 * internalBufferMultiplier);

            //Create DC removers
            dcRemoverI = new DcRemover();
            dcRemoverI.Init();
            dcRemoverQ = new DcRemover();
            dcRemoverQ.Init();

            //Open
            if (NativeMethods.rtlsdr_open(out device, deviceIndex) != RTL_SUCCESS_OPCODE)
                throw new RadioNotFoundException();

            //Load device gains
            int supportedGainsCount = NativeMethods.rtlsdr_get_tuner_gains(device, null);
            supportedGains = new int[supportedGainsCount];
            NativeMethods.rtlsdr_get_tuner_gains(device, supportedGains);

            //Load device name
            deviceName = NativeMethods.rtlsdr_get_device_name(deviceIndex);

            //Create handle
            gcHandle = GCHandle.Alloc(this);

            //Open device
            SetSampleRate(sampleRate);
            CenterFrequency = 97100000;
            SetManualGain(5);

            //Clear buffer to prepare for reading
            ResetDeviceBuffer();

            //Open device for streaming
            waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            workerThread = new Thread(RunStreamWorker);
            workerThread.IsBackground = true;
            workerThread.Start();

            return sampleRate;
        }

        public void SetSampleRate(uint sampleRate)
        {
            int opcode = NativeMethods.rtlsdr_set_sample_rate(device, sampleRate);
            if (opcode != RTL_SUCCESS_OPCODE)
                throw new RtlDeviceErrorException(opcode);
        }

        public void SetCenterFreq(uint centerFreq)
        {
            int opcode = NativeMethods.rtlsdr_set_center_freq(device, centerFreq);
            if (opcode != RTL_SUCCESS_OPCODE)
                throw new RtlDeviceErrorException(opcode);
        }

        public void SetManualGain(int gain)
        {
            //Change mode
            int opcode = NativeMethods.rtlsdr_set_tuner_gain_mode(device, 1);
            if (opcode != RTL_SUCCESS_OPCODE)
                throw new RtlDeviceErrorException(opcode);

            //Change gain
            opcode = NativeMethods.rtlsdr_set_tuner_gain(device, gain);
            if (opcode != RTL_SUCCESS_OPCODE)
                throw new RtlDeviceErrorException(opcode);
        }

        public void SetAutomaticGain()
        {
            //Change mode
            if (NativeMethods.rtlsdr_set_tuner_gain_mode(device, 0) != RTL_SUCCESS_OPCODE)
                throw new Exception("Failed to access RTL device.");
        }

        public void ResetDeviceBuffer()
        {
            //Reset device buffer
            if (NativeMethods.rtlsdr_reset_buffer(device) != RTL_SUCCESS_OPCODE)
                throw new Exception("Failed to access RTL device.");
        }

        public override unsafe int Read(Complex* iq, int bufferLength)
        {
            //Wait
            while (buffer.GetAvailable() < bufferLength)
                Thread.Sleep(1);

            //Read from buffer
            int samplesRead = buffer.Read(outgoingBufferPtr, bufferLength * 2) / 2;

            //Convert
            for (var i = 0; i < samplesRead; i++)
            {
                iq[i].Imag = _lutPtr[outgoingBufferPtr[(i * 2) + 0]];
                iq[i].Real = _lutPtr[outgoingBufferPtr[(i * 2) + 1]];
            }

            //Clean up
            dcRemoverI.ProcessInterleaved((float*)iq, samplesRead);
            dcRemoverQ.ProcessInterleaved(((float*)iq) + 1, samplesRead);

            return samplesRead;
        }

        private void RunStreamWorker()
        {
            NativeMethods.rtlsdr_read_async(device, RtlSdrSamplesAvailable, (IntPtr)gcHandle, 0, (uint)(incomingBufferLength * 2));
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

            //Write samples to buffer
            int dropped = (int)len - instance.buffer.Write(buf, (int)len);

            //Deal with dropped samples
            instance.droppedSamples += dropped;
            if(dropped != 0)
                Console.WriteLine($"[RtlSdrSource] Can't keep up! Dropped {dropped} samples just now, {instance.droppedSamples} total.");

            //Send events
            //instance.waitHandle.Set();
        }

        public override void Close()
        {
            //Close device
            //device.Dispose();
            //device = null;
        }

        public override void Dispose()
        {
            //circularBuffer.Dispose();
        }
    }
}
