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
    unsafe class RtlSdrDevice : IDisposable
    {
        public readonly IntPtr device;
        public readonly int[] supportedGains;
        public readonly string deviceName;

        public event SamplesAvailableDelegate SamplesAvailable;

        private static readonly float* _lutPtr;
        private static readonly UnsafeBuffer _lutBuffer = UnsafeBuffer.Create(256, sizeof(float));

        private const int RTL_SUCCESS_OPCODE = 0;

        private GCHandle gcHandle;
        private uint readBufferSize;
        private Thread streamThread;
        private UnsafeBuffer iqBuffer;
        private Complex* iqBufferPtr;

        static RtlSdrDevice()
        {
            _lutPtr = (float*)_lutBuffer;

            const float scale = 1.0f / 127.0f;
            for (var i = 0; i < 256; i++)
            {
                _lutPtr[i] = (i - 128) * scale;
            }
        }

        public RtlSdrDevice(uint index)
        {
            //Open
            if (NativeMethods.rtlsdr_open(out device, index) != RTL_SUCCESS_OPCODE)
                throw new RadioNotFoundException();

            //Load device gains
            int supportedGainsCount = NativeMethods.rtlsdr_get_tuner_gains(device, null);
            supportedGains = new int[supportedGainsCount];
            NativeMethods.rtlsdr_get_tuner_gains(device, supportedGains);

            //Load device name
            deviceName = NativeMethods.rtlsdr_get_device_name(index);

            //Create handle
            gcHandle = GCHandle.Alloc(this);
        }

        public void Dispose()
        {
            //Stop streaming
            StopStreaming();

            //Close RTL device
            NativeMethods.rtlsdr_close(device);

            //Free handle
            gcHandle.Free();
            GC.SuppressFinalize(this);
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

        public void BeginStreaming(uint bufferSampleCount)
        {
            //Check if already streaming
            if (streamThread != null)
                throw new Exception("Already streaming.");

            //Open buffers
            readBufferSize = bufferSampleCount;
            iqBuffer = UnsafeBuffer.Create((int)bufferSampleCount, sizeof(Complex));
            iqBufferPtr = (Complex*)iqBuffer;

            //Reset device buffer
            if (NativeMethods.rtlsdr_reset_buffer(device) != RTL_SUCCESS_OPCODE)
                throw new Exception("Failed to access RTL device.");

            //Start worker thread
            streamThread = new Thread(RunStreamWorker);
            streamThread.Start();
        }

        public void StopStreaming()
        {
            //If we're not streaming, ignore
            if (streamThread == null)
                return;

            //Send end command
            if(NativeMethods.rtlsdr_cancel_async(device) != RTL_SUCCESS_OPCODE)
                throw new Exception("Failed to access RTL device.");

            //Kill thread
            streamThread.Abort();
            streamThread = null;
        }

        private void RunStreamWorker()
        {
            NativeMethods.rtlsdr_read_async(device, RtlSdrSamplesAvailable, (IntPtr)gcHandle, 0, readBufferSize * 2);
        }

        private static void RtlSdrSamplesAvailable(byte* buf, uint len, IntPtr ctx)
        {
            //Get handle
            var gcHandle = GCHandle.FromIntPtr(ctx);
            if (!gcHandle.IsAllocated)
            {
                return;
            }

            //Load
            var instance = (RtlSdrDevice)gcHandle.Target;
            var sampleCount = (int)len / 2;
            var ptr = instance.iqBufferPtr;

            //Read
            for (var i = 0; i < sampleCount; i++)
            {
                ptr->Imag = _lutPtr[*buf++];
                ptr->Real = _lutPtr[*buf++];
                ptr++;
            }

            //Send events
            instance.ComplexSamplesAvailable(instance.iqBufferPtr, sampleCount);
        }

        private void ComplexSamplesAvailable(Complex* buffer, int length)
        {
            SamplesAvailable?.Invoke(buffer, length);
        }
    }

    public unsafe delegate void SamplesAvailableDelegate(Complex* buffer, int length);
}
