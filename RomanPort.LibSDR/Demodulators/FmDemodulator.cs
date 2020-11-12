using RomanPort.LibSDR.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Demodulators
{
    public class FmDemodulator : IDemodulator
    {
        private Complex _iqState;

        public const float FM_GAIN = 0.5f;

        private unsafe int Demodulate(Complex* iq, int count, params float*[] audioChannels)
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
                a = float.IsNaN(a) ? 0.0f : a * FM_GAIN;// * 0.5f;// * 1E-05f;
                foreach (var c in audioChannels)
                    c[i] = a;

                //Update state
                _iqState = iq[i];
            }
            return count;
        }

        public override unsafe int DemodulateMono(Complex* iq, float* audio, int count)
        {
            return Demodulate(iq, count, audio);
        }

        public override unsafe int DemodulateStereo(Complex* iq, float* audioL, float* audioR, int count)
        {
            return Demodulate(iq, count, audioL, audioR);
        }

        public override void OnAttached(int bufferSize)
        {
            
        }

        public override float OnInputSampleRateChanged(float sampleRate)
        {
            return sampleRate;
        }

        public override void Dispose()
        {
            
        }
    }
}
