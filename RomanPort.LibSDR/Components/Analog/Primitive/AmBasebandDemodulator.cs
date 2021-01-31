using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Analog.Primitive
{
    public unsafe class AmBasebandDemodulator
    {
        public AmBasebandDemodulator()
        {

        }

        public void Demodulate(Complex* iq, float* audio, int count)
        {
            for(int i = 0; i<count; i++)
            {
                audio[i] = iq[i].Modulus();
            }
        }

        public void Demodulate(float* samples, float* audio, int count)
        {
            for (int i = 0; i < count; i++)
            {
                audio[i] = new Complex(samples[i]).Modulus();
            }
        }
    }
}
