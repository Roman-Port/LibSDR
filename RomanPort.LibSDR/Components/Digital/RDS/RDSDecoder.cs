using RomanPort.LibSDR.Components.Decimators;
using RomanPort.LibSDR.Components.Filters;
using RomanPort.LibSDR.Components.Filters.Builders;
using RomanPort.LibSDR.Components.Filters.FIR;
using RomanPort.LibSDR.Components.Filters.IIR;
using RomanPort.LibSDR.Components.General;
using RomanPort.LibSDR.Components.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Digital.RDS
{
    public unsafe class RDSDecoder : RDSClient
    {
        public RDSDecoder()
        {
            //Make buffers
            workingBuffer = UnsafeBuffer.Create(BUFFER_SIZE, out workingBufferPtr);
            mainBuffer = UnsafeBuffer.Create(BUFFER_SIZE, out mainBufferPtr);
            magBuffer = UnsafeBuffer.Create(BUFFER_SIZE, out magBufferPtr);
            basebandBuffer = UnsafeBuffer.Create(BUFFER_SIZE, out basebandBufferPtr);

            //Set up bit decoder
            bitDecoder = new RDSBitDecoder();
            bitDecoder.OnFrameDecoded += BitDecoder_OnFrameDecoded;
            bitDecoder.OnSyncStateChanged += (bool sync) => OnSyncStateChanged?.Invoke(sync);
        }

        public event RDSFrameDecoded OnFrameDecoded;
        public event RDSSyncStateChanged OnSyncStateChanged;
        public bool IsRdsSynced { get => bitDecoder.IsSynced; }
        public float SampleRate
        {
            get => sampleRate;
            set => Configure(value);
        }

        private float sampleRate;

        private const int RDS_CARRIER_FREQ = 57000;
        private const int RDS_BANDWIDTH = 2400;
        private const int RDS_BANDWIDTH_TRANSITION = 2000;
        private const int RDS_SYMBOL_RATE = 2375;
        private const float RDS_BIT_RATE = (float)RDS_SYMBOL_RATE / 2;
        private const int BUFFER_SIZE = 4096;

        private UnsafeBuffer mainBuffer;
        private float* mainBufferPtr;
        private int bufferUsage;

        private UnsafeBuffer magBuffer;
        private float* magBufferPtr;

        private UnsafeBuffer basebandBuffer;
        private Complex* basebandBufferPtr;

        private UnsafeBuffer workingBuffer;
        private float* workingBufferPtr;
        private int workingBufferUsage;

        private Oscillator osc;
        private ComplexFirFilter filter;
        private float decimatedSampleRate;
        private int decimationRate;
        private RDSBitDecoder bitDecoder;
        private Pll pll;
        private FloatFirFilter matchedFilter;
        private FloatIirFilter syncFilter;

        private float lastSync;
        private float lastData;
        private float lastSyncSlope;
        private byte lastBit;

        /// <summary>
        /// Sets up the decoder
        /// </summary>
        public void Configure(float sampleRate)
        {
            //Apply
            this.sampleRate = sampleRate;

            //Set up Oscillator, which will offset to 57 kHz
            osc = new Oscillator();
            osc.SampleRate = sampleRate;
            osc.Frequency = -RDS_CARRIER_FREQ;

            //Make a filter to filter just RDS. This also acts as a decimator
            decimationRate = DecimationUtil.CalculateDecimationRate(sampleRate, RDS_BANDWIDTH + RDS_BANDWIDTH_TRANSITION + RDS_BANDWIDTH_TRANSITION, out decimatedSampleRate);
            var filterBuilder = new LowPassFilterBuilder(sampleRate, RDS_BANDWIDTH)
                .SetWindow()
                .SetAutomaticTapCount(RDS_BANDWIDTH_TRANSITION, 20);
            filter = new ComplexFirFilter(filterBuilder, decimationRate);

            //Old
            pll = new Pll();
            pll.SampleRate = decimatedSampleRate;
            pll.DefaultFrequency = 0;
            pll.Range = 12;
            pll.Bandwidth = 1;
            pll.Zeta = 0.707f;
            pll.LockTime = 0.5f;
            pll.LockThreshold = 3.2f;

            var matchedFilterLength = (int)(decimatedSampleRate / RDS_BIT_RATE) | 1;
            var coefficients = new SinFilterBuilder(decimatedSampleRate, RDS_BIT_RATE, matchedFilterLength);
            matchedFilter = new FloatFirFilter(coefficients);

            syncFilter = new FloatIirFilter(IirFilterType.BandPass, RDS_BIT_RATE, decimatedSampleRate, 500);
        }

        /// <summary>
        /// Input is FM baseband
        /// </summary>
        public void Process(float* ptr, int count)
        {
            while(count > 0)
            {
                //Transfer into buffer
                int block = Math.Min(count, BUFFER_SIZE - workingBufferUsage);
                Utils.Memcpy(workingBufferPtr + workingBufferUsage, ptr, block * sizeof(float));
                workingBufferUsage += block;

                //Process
                if (workingBufferUsage == BUFFER_SIZE)
                {
                    ProcessBlockNew(workingBufferPtr, BUFFER_SIZE);
                    workingBufferUsage = 0;
                }

                //Update
                ptr += block;
                count -= block;
            }
        }

        private void BitDecoder_OnFrameDecoded(RDSFrame frame)
        {
            ProcessFrame(frame);
            OnFrameDecoded?.Invoke(frame);
        }

        private void ProcessBlockNew(float* ptr, int count)
        {
            //Offset by 57 kHz to the RDS carriers
            for (var i = 0; i < count; i++)
            {
                osc.Tick();
                basebandBufferPtr[i] = (osc.Phase * ptr[i]);
            }

            //Decimate + filter
            count = filter.Process(basebandBufferPtr, count);

            //PLL
            for (var i = 0; i < count; i++)
            {
                mainBufferPtr[i] = pll.Process(basebandBufferPtr[i]).Imag;
            }

            //Matched filter
            matchedFilter.Process(mainBufferPtr, count);

            //Recover signal energy to sustain the oscillation in the IIR
            for (var i = 0; i < count; i++)
                magBufferPtr[i] = Math.Abs(mainBufferPtr[i]);

            //Synchronize to RDS bitrate
            syncFilter.Process(magBufferPtr, count);

            //Detect RDS bits
            float slope;
            for (int i = 0; i < count; i++)
            {
                slope = magBufferPtr[i] - lastSync;
                if (slope < 0.0f && lastSyncSlope * slope < 0.0f)
                {
                    byte bit = lastData > 0 ? (byte)1 : (byte)0;
                    bitDecoder.ProcessBit((byte)(bit ^ lastBit));
                    lastBit = bit;
                }
                lastSync = magBufferPtr[i];
                lastData = mainBufferPtr[i];
                lastSyncSlope = slope;
            }
        }
    }
}
