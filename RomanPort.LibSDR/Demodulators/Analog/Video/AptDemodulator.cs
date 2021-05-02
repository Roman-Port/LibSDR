using RomanPort.LibSDR.Components;
using RomanPort.LibSDR.Components.Analog.Primitive;
using RomanPort.LibSDR.Components.Analog.Video;
using RomanPort.LibSDR.Components.Filters.Builders;
using RomanPort.LibSDR.Components.Filters.FIR;
using RomanPort.LibSDR.Components.Filters.FIR.Real;
using RomanPort.LibSDR.Components.General;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Demodulators.Analog.Video
{
    public unsafe class AptDemodulator : AptImageDecoder
    {
        public AptDemodulator(float sampleRate, int bufferSize) : base()
        {
            Configure(sampleRate, bufferSize);
        }

        public const int BAUD_RATE = 4160;

        private FmBasebandDemodulator fmDemod;
        private AmBasebandDemodulator amDemod;
        private Oscillator osc;
        private IRealFirFilter filter;
        private float symbolsPerSample;
        private float resampleOffset;
        private float resampleMax;

        private UnsafeBuffer buffer;
        private float* bufferPtr;

        public void Configure(float sampleRate, int bufferSize)
        {
            //Complain if too low
            if (sampleRate < BAUD_RATE)
                throw new Exception($"The sample rate specified, {sampleRate}, is too low to be useful. Must be >= {BAUD_RATE}.");
            
            //Create parts
            fmDemod = new FmBasebandDemodulator();
            fmDemod.Configure(bufferSize, sampleRate);
            amDemod = new AmBasebandDemodulator();
            osc = new Oscillator(sampleRate, -2400);
            symbolsPerSample = BAUD_RATE / sampleRate;

            //Make filter
            var filterBuilder = new LowPassFilterBuilder(sampleRate, 2080)
                .SetAutomaticTapCount(200)
                .SetWindow();
            filter = RealFirFilter.CreateFirFilter(filterBuilder);

            //Create buffer
            buffer?.Dispose();
            buffer = UnsafeBuffer.Create(bufferSize, out bufferPtr);
        }

        public void ProcessIQ(Complex* iq, int count)
        {
            fmDemod.Demodulate(iq, bufferPtr, count);
            ProcessBuffer(count);
        }

        public void ProcessFM(float* ptr, int count)
        {
            Utils.Memcpy(bufferPtr, ptr, count * sizeof(float));
            ProcessBuffer(count);
        }

        private void ProcessBuffer(int count)
        {
            //AM demodulate and offset
            osc.Mix(bufferPtr, count);
            amDemod.Demodulate(bufferPtr, bufferPtr, count);

            //Filter
            filter.Process(bufferPtr, count);

            //Get pixels
            for (int i = 0; i < count; i++)
            {
                if(resampleOffset >= 1)
                {
                    ProcessSample(resampleMax);
                    resampleMax = 0;
                    resampleOffset -= 1;
                }
                resampleOffset += symbolsPerSample;
                resampleMax = Math.Max(resampleMax, bufferPtr[i]);
            }
        }
    }
}
