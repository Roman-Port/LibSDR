using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters.Builders
{
    public class HighPassFilterBuilder : FilterBuilderBase
    {
        public HighPassFilterBuilder(float sampleRate, int cutoffFreq) : base(sampleRate)
        {
            CutoffFreq = cutoffFreq;
        }

        public int CutoffFreq { get; set; }

        public override float[] BuildFilter()
        {
            float[] taps = new float[TapCount];
            float[] window = FilterWindowUtil.MakeWindow(Window, TapCount);

            int M = (TapCount - 1) / 2;
            float fwT0 = 2 * MathF.PI * CutoffFreq / SampleRate;

            for (int n = -M; n <= M; n++)
            {
                if (n == 0)
                    taps[n + M] = ((1 - (fwT0 / MathF.PI)) * window[n + M]);
                else
                {
                    taps[n + M] = -MathF.Sin(n * fwT0) / (n * MathF.PI) * window[n + M];
                }
            }

            float fmax = taps[0 + M];
            for (int n = 1; n <= M; n++)
                fmax += 2 * taps[n + M] * MathF.Cos(n * MathF.PI);

            float gain = 1f / fmax;

            for (int i = 0; i < TapCount; i++)
                taps[i] *= gain;

            return taps;
        }
    }
}
