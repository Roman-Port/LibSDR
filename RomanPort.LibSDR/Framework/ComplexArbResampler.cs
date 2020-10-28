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

        private IQFirFilter filter;
        private FloatMultichannelArbResampler resampler;

        private UnsafeBuffer tempBuffer;
        private Complex* tempBufferComplexPtr;
        private float* tempBufferFloatPtr;

        public ComplexArbResampler(float inputSampleRate, float outputSampleRate, int bufferSize)
        {
            this.inputSampleRate = inputSampleRate;
            this.outputSampleRate = outputSampleRate;
            this.bufferSize = bufferSize;

            //Create resampler
            resampler = new FloatMultichannelArbResampler(inputSampleRate, outputSampleRate, 2);

            //Create filter
            var coefficients = FilterBuilder.MakeBandPassKernel(inputSampleRate, 250, 0, (int)(outputSampleRate / 2), WindowType.BlackmanHarris4);
            filter = new IQFirFilter(coefficients);

            //Create buffer
            tempBuffer = UnsafeBuffer.Create(bufferSize, sizeof(Complex));
            tempBufferComplexPtr = (Complex*)tempBuffer;
            tempBufferFloatPtr = (float*)tempBuffer;
        }

        public void Dispose()
        {
            tempBuffer.Dispose();
            filter.Dispose();
        }

        public int Process(Complex* input, Complex* output, int count)
        {
            //Copy to temp buffer
            /*for (int i = 0; i < count; i++)
                tempBufferComplexPtr[i] = input[i];

            //Filter
            filter.Process(tempBufferComplexPtr, count);*/

            //Resample
            int resampleCount = resampler.Process((float*)input, count * 2, (float*)output, count * 2, false) / 2;

            return resampleCount;
        }
    }
}
