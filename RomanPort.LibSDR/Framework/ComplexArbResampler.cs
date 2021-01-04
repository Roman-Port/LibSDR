using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework
{
    public unsafe class ComplexArbResampler : IDisposable
    {
        private float inputSampleRate;
        private float outputSampleRate;
        private int bufferSize;

        private FloatArbResampler resamplerA;
        private FloatArbResampler resamplerB;

        public ComplexArbResampler(float inputSampleRate, float outputSampleRate, int bufferSize)
        {
            this.inputSampleRate = inputSampleRate;
            this.outputSampleRate = outputSampleRate;
            this.bufferSize = bufferSize;

            //Create resampler
            resamplerA = new FloatArbResampler(inputSampleRate, outputSampleRate, 2, 0);
            resamplerB = new FloatArbResampler(inputSampleRate, outputSampleRate, 2, 1);
        }

        public void Dispose()
        {

        }

        public int Process(Complex* input, Complex* output, int count)
        {
            return Process(input, output, count, out int consumedSamples);
        }

        public int Process(Complex* input, Complex* output, int count, out int consumedSamples)
        {
            resamplerA.Process((float*)input, count, (float*)output, bufferSize, false, out consumedSamples, out int resampleCount);
            resamplerB.Process((float*)input, count, (float*)output, bufferSize, false);

            return resampleCount;
        }
    }
}
