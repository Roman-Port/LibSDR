using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters.Builders
{
    public class BandPassFilterBuilder : FilterPassBuilderBase, IFilterBuilderReal, IFilterBuilderComplex
    {
        public BandPassFilterBuilder(float sampleRate, int lowCutoffFreq, int highCutoffFreq) : base(sampleRate)
        {
            LowCutoffFreq = lowCutoffFreq;
            HighCutoffFreq = highCutoffFreq;
        }

        public int LowCutoffFreq { get; set; }
        public int HighCutoffFreq { get; set; }

        protected override float MaxFilterFreq => HighCutoffFreq;

        public float[] BuildFilterReal()
        {
            float[] taps = new float[TapCount];
            float[] window = WindowUtil.MakeWindow(Window, TapCount);

            int M = (TapCount - 1) / 2;
            float fwT0 = 2 * MathF.PI * LowCutoffFreq / SampleRate;
            float fwT1 = 2 * MathF.PI * HighCutoffFreq / SampleRate;

            for (int n = -M; n <= M; n++)
            {
                if (n == 0)
                    taps[n + M] = (fwT1 - fwT0) / MathF.PI * window[n + M];
                else
                {
                    taps[n + M] = (MathF.Sin(n * fwT1) - MathF.Sin(n * fwT0)) / (n * MathF.PI) * window[n + M];
                }
            }

            float fmax = taps[0 + M];
            for (int n = 1; n <= M; n++)
                fmax += 2 * taps[n + M] * MathF.Cos(n * (fwT0 + fwT1) * 0.5f);

            float gain = 1f / fmax;

            for (int i = 0; i < TapCount; i++)
                taps[i] *= gain;

            return taps;
        }

        public override void ValidateDecimation(int decimation)
        {

        }

        public BandPassFilterBuilder SetAutomaticTapCount(float transitionWidth, float attenuation = 30)
        {
            _SetAutomaticTapCount(transitionWidth, attenuation);
            return this;
        }

        public BandPassFilterBuilder SetManualTapCount(int taps)
        {
            _SetManualTapCount(taps);
            return this;
        }

        public BandPassFilterBuilder SetWindow(WindowType window = WindowType.BlackmanHarris7)
        {
            _SetWindow(window);
            return this;
        }

        public Complex[] BuildFilterComplex()
        {
            //Construct base filter
            var baseTaps = new LowPassFilterBuilder(SampleRate, (HighCutoffFreq - LowCutoffFreq) / 2)
                .SetManualTapCount(TapCount)
                .SetWindow(Window)
                .BuildFilterReal();

            //Calculate freq
            float freq = MathF.PI * (HighCutoffFreq + LowCutoffFreq) / SampleRate;

            //Calculate initial phase
            float phase;
            if ((baseTaps.Length & 1) != 0)
                phase = -freq * (baseTaps.Length >> 1);
            else
                phase = -freq / 2 * ((1 + 2 * baseTaps.Length) >> 1);

            //Generate
            Complex[] output = new Complex[TapCount];
            for (int i = 0; i < baseTaps.Length; i++)
            {
                output[i] = new Complex(baseTaps[i] * MathF.Cos(phase), baseTaps[i] * MathF.Sin(phase));
                phase += freq;
            }

            return output;
        }
    }
}
