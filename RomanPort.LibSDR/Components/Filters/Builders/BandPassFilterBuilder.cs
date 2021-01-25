using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters.Builders
{
    public class BandPassFilterBuilder : FilterBuilderBase
    {
        public BandPassFilterBuilder(float sampleRate, int lowCutoffFreq, int highCutoffFreq) : base(sampleRate)
        {
            LowCutoffFreq = lowCutoffFreq;
            HighCutoffFreq = highCutoffFreq;
        }

        public int LowCutoffFreq { get; set; }
        public int HighCutoffFreq { get; set; }

        public override float[] BuildFilter()
        {
            float[] taps = new float[TapCount];
            float[] window = FilterWindowUtil.MakeWindow(Window, TapCount);

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
    }
}
