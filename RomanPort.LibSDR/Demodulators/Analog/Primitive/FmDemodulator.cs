using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Demodulators.Analog.Primitive
{
    public class FmDemodulator : IAudioDemodulator
    {
        public FmDemodulator()
        {

        }

        private Complex _iqState;
        public const float FM_GAIN = 0.5f;

        public unsafe void Demodulate(Complex* iq, float* audio, int count)
        {
            for (var i = 0; i < count; i++)
            {
                //Polar discriminator
                var f = iq[i] * _iqState.Conjugate();

                //Limiting
                var m = f.Modulus();
                if (m > 0.0f)
                {
                    f /= m;
                }

                //Angle estimate
                var a = f.Argument();

                //Write
                audio[i] = float.IsNaN(a) ? 0.0f : a * FM_GAIN;// * 0.5f;// * 1E-05f;

                //Update state
                _iqState = iq[i];
            }
        }

        public unsafe void DemodulateStereo(Complex* iq, float* left, float* right, int count)
        {
            //Simply clone to the other channel
            Demodulate(iq, left, count);
            Utils.Memcpy(right, left, count * sizeof(float));
        }

        public void Configure(int bufferSize, float sampleRate)
        {
            
        }
    }
}
