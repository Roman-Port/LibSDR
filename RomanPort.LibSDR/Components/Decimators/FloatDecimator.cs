using RomanPort.LibSDR.Components.Filters.FIR;
using RomanPort.LibSDR.Components.Filters.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using RomanPort.LibSDR.Components.Filters;

namespace RomanPort.LibSDR.Components.Decimators
{
    public unsafe class FloatDecimator
    {
        public FloatDecimator(float sampleRate, float bandwidth, int decimationRate, float attenuation, float transitionWidth)
        {
            //Set
            this.decimationRate = decimationRate;
            this.sampleRate = sampleRate;
            this.bandwidth = bandwidth;
            this.attenuation = attenuation;
            this.transitionWidth = transitionWidth;

            //Configure
            Configure();
        }

        private FloatFirFilter filter;
        private int decimationRate;
        private float sampleRate;
        private float bandwidth;
        private float attenuation;
        private float transitionWidth;

        public int DecimationRate
        {
            get => decimationRate;
            set
            {
                //Validate
                if (decimationRate < 1)
                    throw new Exception("Decimation rate must be >= 1.");

                //Set
                decimationRate = value;

                //Reconfigure
                Configure();
            }
        }

        public float Bandwidth
        {
            get => bandwidth;
            set
            {
                bandwidth = value;
                Configure();
            }
        }

        private void Configure()
        {
            var builder = new LowPassFilterBuilder(sampleRate, (int)(Math.Min(sampleRate / decimationRate, bandwidth) / 2))
                .SetWindow(WindowType.Hamming)
                .SetAutomaticTapCount(transitionWidth, attenuation);
            filter = new FloatFirFilter(builder, decimationRate);
        }

        public int Process(float* ptr, int count, int channelCount = 1)
        {
            return filter.Process(ptr, count, channelCount);
        }

        public int Process(float* inPtr, float* outPtr, int count, int channelCount = 1)
        {
            return filter.Process(inPtr, outPtr, count, channelCount);
        }
    }
}
