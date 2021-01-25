using System;
using System.Collections.Generic;
using System.Text;
using RomanPort.LibSDR.Framework;

namespace RomanPort.LibSDR.Components.Digital
{
    /// <summary>
    /// Costas loop designed for BPSK only, for the time being
    /// </summary>
    public unsafe class CostasLoop
    {
        public CostasLoop(float loopBw)
        {
            this.loopBw = loopBw;
            Reset();
        }

        private float loopBw;
        private float phase;
        private float freq;
        private float beta;
        private float alpha;

        private const float DAMPING = 0.70710678f; //Sqrt 2 divided by 2

        public void Reset()
        {
            float denom = (1 + 2 * DAMPING * loopBw + loopBw * loopBw);
            alpha = (4 * DAMPING * loopBw) / denom;
            beta = (4 * loopBw * loopBw) / denom;
        }

        public void Process(Complex* ptr, int count)
        {
            for(int i = 0; i<count; i++)
            {
                //Get the outut
                ptr[i] = ptr[i] * new Complex(MathF.Cos(-phase), MathF.Sin(-phase));

                //Calculate the error generated
                float error = (ptr[i].Real * ptr[i].Imag);

                //Advance loop
                freq = freq + beta * error;
                phase = phase + freq + alpha * error;

                //Phase wrap
                while (phase > (2 * 3.14159f))
                    phase -= 2 * 3.14159f;
                while (phase < (-2 * 3.14159f))
                    phase += 2 * 3.14159f;

                //Limit frequency to 1 and -1
                if (freq > 1)
                    freq = 1;
                if (freq < -1)
                    freq = -1;
            }
        }
    }
}
