using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Analog
{
    public class PLL
    {
        public PLL(float sampleRate, float lockTime, float loopBandwidth, float maxFreq, float minFreq)
        {
            //Set
            this.loopBandwidth = loopBandwidth;
            this.maxFreq = 2 * MathF.PI * maxFreq / sampleRate;
            this.minFreq = 2 * MathF.PI * minFreq / sampleRate;
            damping = MathF.Sqrt(2) / 2f;

            //Calculate
            float d = (1 + 2 * damping * this.loopBandwidth + this.loopBandwidth * this.loopBandwidth);
            alpha = (4 * damping * this.loopBandwidth) / d;
            beta = (4 * this.loopBandwidth * this.loopBandwidth) / d;
            lockAlpha = 1f - MathF.Exp(-1.0f / (sampleRate * lockTime));
        }

        public Complex Ref { get => new Complex(MathF.Cos(phase), MathF.Sin(phase)); }
        public float Phase { get => phase; }
        public bool IsLocked { get => phaseErrorAvg < lockThreshold; }
        public float LockThreshold { get => lockThreshold; set => lockThreshold = value; }

        private float phase;
        private float freq;
        private float alpha;
        private float beta;
        private float maxFreq;
        private float minFreq;
        private float damping;
        private float loopBandwidth;
        private float phaseErrorAvg;
        private float lockAlpha;
        private float lockThreshold = 1;

        public void Process(float sample)
        {
            Process(new Complex(sample, 0));
        }

        public unsafe void Process(Complex sample)
        {
            //Calculate phase error
            float error = sample.ArgumentFast() - phase;
            if (error > MathF.PI)
                error -= (2 * MathF.PI);
            if (error < -MathF.PI)
                error += (2 * MathF.PI);
            phaseErrorAvg = (1 - lockAlpha) * phaseErrorAvg + lockAlpha * error * error;

            //Advance the loop by this error
            freq = freq + beta * error;
            phase = phase + freq + alpha * error;

            //Constrain phase
            while (phase > (2 * MathF.PI))
                phase -= 2 * MathF.PI;
            while (phase < (-2 * MathF.PI))
                phase += 2 * MathF.PI;

            //Constrain frequency
            if (freq > maxFreq)
                freq = maxFreq;
            else if (freq < minFreq)
                freq = minFreq;
        }
    }
}
