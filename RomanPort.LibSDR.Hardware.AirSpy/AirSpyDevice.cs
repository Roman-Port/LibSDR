using RomanPort.LibSDR.Components;
using RomanPort.LibSDR.Components.Filters.FIR;
using RomanPort.LibSDR.Components.Interfaces;
using RomanPort.LibSDR.Components.IO.Buffers;
using RomanPort.LibSDR.Components.IO.USB;
using RomanPort.LibSDR.Hardware.AirSpy.IqConverter;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace RomanPort.LibSDR.Hardware.AirSpy
{
    public unsafe class AirSpyDevice : IRadioDevice
    {
        private const ushort AIRSPY_USB_VENDOR = 0x1d50;
        private const ushort AIRSPY_USB_PRODUCT = 0x60a1;

        private const int TRANSFER_COUNT = 16; //16
        private const int TRANSFER_BUFFER_SIZE = 1 << 16;
        private const byte TRANSFER_ENDPOINT_IN = 0x80;

        private const int SAMPLE_RESOLUTION = 12;
        private const int SAMPLE_ENCAPSULATION = 16;

        private const int SAMPLE_SHIFT = (SAMPLE_ENCAPSULATION - SAMPLE_RESOLUTION);
        private const float SAMPLE_SCALE = (1.0f / (1 << (15 - SAMPLE_SHIFT)));

        private readonly byte[] GAINCFG_VGA = { 13, 12, 11, 11, 11, 11, 11, 10, 10, 10, 10, 10, 10, 10, 10, 10, 9, 8, 7, 6, 5, 4 };
        private readonly byte[] GAINCFG_MIXER = { 12, 12, 11, 9, 8, 7, 6, 6, 5, 0, 0, 1, 0, 0, 2, 2, 1, 1, 1, 1, 0, 0 };
        private readonly byte[] GAINCFG_LNA = { 14, 14, 14, 13, 12, 10, 9, 9, 8, 9, 8, 6, 5, 3, 1, 0, 0, 0, 0, 0, 0, 0 };

        public string SerialNumber
        {
            get
            {
                string sn = device.DescriptorSerialNumber;
                if (!sn.StartsWith("AIRSPY SN:"))
                    throw new Exception("Serial number is not valid: " + sn);
                return sn.Substring(10);
            }
        }
        public unsafe long[] SupportedSampleRates
        {
            get
            {
                //Fetch count
                int count;
                using(var buffer = new UsbBuffer(sizeof(int)))
                {
                    device.ControlTransferRead((byte)AirSpyOpcodes.AIRSPY_GET_SAMPLERATES, 0, 0, buffer, 1000);
                    count = *(int*)buffer.AsPtr();
                }

                //Allocate array and fetch
                long[] output = new long[count + 1];
                using (var buffer = new UsbBuffer(count * sizeof(uint)))
                {
                    //Get
                    device.ControlTransferRead((byte)AirSpyOpcodes.AIRSPY_GET_SAMPLERATES, 0, (ushort)count, buffer, 1000);

                    //Copy to output
                    uint* next = (uint*)buffer.AsPtr();
                    for (int i = 0; i < count; i++)
                        output[i] = *next++;
                }

                //Manually add the high sample rate 10 MSPS option
                output[output.Length - 1] = 10000000;

                return output;
            }
        }
        public unsafe long SampleRate
        {
            get => sampleRate;
            set
            {
                //Mutate
                uint rate = (uint)value;
                rate *= 2; //Using IQ mode instead of real mode
                rate /= 1000;

                //Write
                using (UsbBuffer buffer = new UsbBuffer(1))
                    device.ControlTransferRead((byte)AirSpyOpcodes.AIRSPY_SET_SAMPLERATE, 0, (ushort)rate, buffer, 1000);

                //Apply
                sampleRate = (uint)value;
            }
        }
        public unsafe AirSpyReceiverMode ReceiverMode
        {
            set => device.ControlTransferWrite((byte)AirSpyOpcodes.AIRSPY_RECEIVER_MODE, (ushort)value, 0, null, 1000);
        }
        public unsafe long CenterFrequency
        {
            get => centerFreq;
            set
            {
                using(UsbBuffer buffer = UsbBuffer.FromValue((uint)value))
                    device.ControlTransferWrite((byte)AirSpyOpcodes.AIRSPY_SET_FREQ, 0, 0, buffer, 1000);
                centerFreq = value;
            }
        }
        public unsafe byte LnaGain
        {
            set => SendSetReadTransfer(AirSpyOpcodes.AIRSPY_SET_LNA_GAIN, value);
        }
        public unsafe bool LnaAgc
        {
            set => SendSetReadTransfer(AirSpyOpcodes.AIRSPY_SET_LNA_AGC, value);
        }
        public unsafe byte MixerGain
        {
            set => SendSetReadTransfer(AirSpyOpcodes.AIRSPY_SET_MIXER_GAIN, value);
        }
        public unsafe bool MixerAgc
        {
            set => SendSetReadTransfer(AirSpyOpcodes.AIRSPY_SET_MIXER_AGC, value);
        }
        public unsafe byte VgaGain
        {
            set => SendSetReadTransfer(AirSpyOpcodes.AIRSPY_SET_VGA_GAIN, value);
        }

        private IUsbDevice device;
        private uint sampleRate;
        private long centerFreq;
        private IUsbAsyncTransfer[] transfers;
        private bool streamingActive;
        private AirSpyIqConverter iqConverter;

        private UnsafeBuffer sampleBuffer;
        private Complex* sampleBufferPtr;
        private float* sampleBufferPtrFloat;

        private CircularBuffer<Complex> circBuffer;
        private AutoResetEvent circBufferAvailable;
        private long droppedSamples;

        public event IRadioDevice_ConnectionAborted OnAborted;

        public static AirSpyDevice OpenDevice(IUsbProvider provider)
        {
            //Get all AirSpy devices
            IUsbDevice[] devices = provider.FindDevices(AIRSPY_USB_VENDOR, AIRSPY_USB_PRODUCT);
            
            //Open the first device we can
            foreach(var d in devices)
            {
                if (d.OpenDevice())
                    return new AirSpyDevice(d);
            }

            return null;
        }

        public AirSpyDevice(IUsbDevice device)
        {
            //Configure
            this.device = device;

            //Create buffer
            sampleBuffer = UnsafeBuffer.Create(TRANSFER_BUFFER_SIZE / sizeof(short), out sampleBufferPtr);
            sampleBufferPtrFloat = (float*)sampleBufferPtr;

            //Create filter
            fixed (float* k = AirSpyConst.HP_KERNEL)
                iqConverter = new AirSpyIqConverter(k, AirSpyConst.HP_KERNEL.Length);

            //Create circular buffer stuff
            circBuffer = new CircularBuffer<Complex>(500000);
            circBufferAvailable = new AutoResetEvent(false);

            //Prepare transfers
            transfers = new IUsbAsyncTransfer[TRANSFER_COUNT];
            for(int i = 0; i < TRANSFER_COUNT; i++)
            {
                transfers[i] = device.OpenBulkTransfer(TRANSFER_ENDPOINT_IN | 1, TRANSFER_BUFFER_SIZE);
                transfers[i].OnTransferCompleted += AirSpyDevice_OnTransferCompleted;
                transfers[i].OnTransferFailed += AirSpyDevice_OnTransferFailed;
            }
        }

        public void StartRx()
        {
            //Make sure it's not already running
            if (streamingActive)
                return;
            
            //Set state
            streamingActive = true;
            ReceiverMode = AirSpyReceiverMode.RX;

            //Start transfers
            for (int i = 0; i < TRANSFER_COUNT; i++)
                transfers[i].SubmitTransfer();
        }

        private void AirSpyDevice_OnTransferCompleted(IUsbAsyncTransfer transfer, byte* bufferPtr, int count)
        {
            //Unpack samples into floats
            count /= 2;
            short* bufferPtrShort = (short*)bufferPtr;
            for(int i = 0; i<count; i++)
                sampleBufferPtrFloat[i] = (bufferPtrShort[i] - 2048) * SAMPLE_SCALE;

            //Submit the transfer to keep streaming. We are no longer using the buffer these will be written to.
            if (streamingActive)
                transfer.SubmitTransfer();

            //Convert
            iqConverter.Process(sampleBufferPtrFloat, count);

            //Write to buffer
            count /= 2;
            droppedSamples += count - circBuffer.Write(sampleBufferPtr, count);
            circBufferAvailable.Set();
        }

        private void AirSpyDevice_OnTransferFailed(IUsbAsyncTransfer transfer)
        {
            Abort(IRadioDevice_ConnectionAbortedReason.ERR_LOST);
        }

        private void Abort(IRadioDevice_ConnectionAbortedReason reason)
        {
            streamingActive = false;
            OnAborted?.Invoke(this, reason);
        }

        private bool SendSetReadTransfer(AirSpyOpcodes opcode, bool value)
        {
            return SendSetReadTransfer(opcode, (ushort)(value ? 1 : 0));
        }

        private bool SendSetReadTransfer(AirSpyOpcodes opcode, ushort value)
        {
            byte result;
            using(var buffer = new UsbBuffer(1))
            {
                device.ControlTransferRead((byte)opcode, 0, value, buffer, 1000);
                result = *buffer.AsPtr();
            }
            return result == 1;
        }

        public void SetLinearGain(float value)
        {
            //Get the index in the gains to use
            int gains = (int)((1 - value) * GAINCFG_VGA.Length);

            //Constrain
            if (gains < 0)
                gains = 0;
            if (gains >= GAINCFG_VGA.Length)
                gains = GAINCFG_VGA.Length - 1;

            //Apply
            VgaGain = GAINCFG_VGA[gains];
            MixerGain = GAINCFG_MIXER[gains];
            LnaGain = GAINCFG_LNA[gains];
        }

        public void StopRx()
        {
            Abort(IRadioDevice_ConnectionAbortedReason.NORMAL);
        }

        public int Read(Complex* ptr, int count, int timeout)
        {
            if (circBuffer.IsEmpty)
                circBufferAvailable.WaitOne(timeout);
            return circBuffer.Read(ptr, count);
        }
    }
}
