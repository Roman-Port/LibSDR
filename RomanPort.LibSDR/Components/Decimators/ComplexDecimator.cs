using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;
using RomanPort.LibSDR.Framework;

namespace RomanPort.LibSDR.Components.Decimators
{
    public class ComplexDecimator
    {
        public ComplexDecimator(float sampleRate, float bandwidth, int decimationRate, float attenuation, float transitionWidth)
        {
            a = new FloatDecimator(sampleRate, bandwidth, decimationRate, attenuation, transitionWidth);
            b = new FloatDecimator(sampleRate, bandwidth, decimationRate, attenuation, transitionWidth);
        }

        public static ComplexDecimator CalculateDecimator(float inputSampleRate, float outputBandwidth, float attenuation, float transitionWidth, out float actualOutputRate)
        {
            int decimationRate = DecimationUtil.CalculateDecimationRate(inputSampleRate, outputBandwidth, out actualOutputRate);
            return new ComplexDecimator(inputSampleRate, outputBandwidth, decimationRate, attenuation, transitionWidth);
        }

        public int DecimationRate
        {
            get => a.DecimationRate;
            set
            {
                a.DecimationRate = value;
                b.DecimationRate = value;
            }
        }

        public float Bandwidth
        {
            get => a.Bandwidth;
            set
            {
                a.Bandwidth = value;
                b.Bandwidth = value;
            }
        }

        private FloatDecimator a;
        private FloatDecimator b;

        public unsafe int Process(Complex* ptr, int count)
        {
            float* fPtr = (float*)ptr;
            a.Process(fPtr, count, 2);
            return b.Process(fPtr + 1, count, 2);
        }

        public unsafe int Process(Complex* inPtr, Complex* outPtr, int count)
        {
            float* fInPtr = (float*)inPtr;
            float* fOutPtr = (float*)outPtr;
            a.Process(fInPtr, fOutPtr, count, 2);
            return b.Process(fInPtr + 1, fOutPtr + 1, count, 2);
        }
    }
}
