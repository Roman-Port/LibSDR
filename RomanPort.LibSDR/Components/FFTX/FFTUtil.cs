using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.FFTX
{
    public static unsafe class FFTUtil
    {
        public static void ApplySmoothening(float* buffer, float* incoming, int count, float attack, float decay)
        {
            float ratio;
            for (var i = 0; i < count; i++)
            {
                ratio = buffer[i] < incoming[i] ? attack : decay;
                buffer[i] = buffer[i] * (1 - ratio) + incoming[i] * ratio;
            }
        }

        public static void CalculatePower(Complex* input, float* power, int fftLen)
        {
            float normalizationFactor = 1.0f / fftLen;
            for (int i = 0; i < fftLen; i++)
            {
                float real = input->Real * normalizationFactor;
                float imag = input++->Imag * normalizationFactor;
                *power++ = (float)(10.0 * Math.Log10(((real * real) + (imag * imag)) + 1e-20));
            }
        }

        public static void ResizePower(float* input, float* output, int inputLen, int outputLen)
        {
            int inputIndex = 0;
            int outputIndex = 0;
            float max = 0;
            bool resetMax = true;
            while(inputIndex < inputLen)
            {
                //Read value
                if (resetMax)
                    max = input[inputIndex++];
                else
                    max = Math.Max(input[inputIndex++], max);

                //Write to output
                int targetOutput = (inputIndex * outputLen) / inputLen;
                resetMax = (outputIndex < targetOutput);
                while (outputIndex < targetOutput)
                    output[outputIndex++] = max;
            }
        }

        public static void CalculatePowerSnr(float* power, int count, out float ceil, out float floor)
        {
            //Define default outputs
            ceil = power[0];
            floor = power[0];

            //Loop
            for(int i = 1; i<count; i++)
            {
                ceil = Math.Max(ceil, power[i]);
                floor = Math.Min(floor, power[i]);
            }
        }

        public static void OffsetSpectrum<T>(T* buffer, int count) where T : unmanaged
        {
            count /= 2;
            T* left = buffer;
            T* right = buffer + count;
            T temp;
            for (int i = 0; i < count; i++)
            {
                temp = *left;
                *left++ = *right;
                *right++ = temp;
            }
        }
    }
}
