using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters.Builders
{
    public class HighPassFilterBuilder : FilterPassBuilderBase, IFilterBuilderReal
    {
        public HighPassFilterBuilder(float sampleRate, int cutoffFreq) : base(sampleRate)
        {
            CutoffFreq = cutoffFreq;
        }

        public int CutoffFreq { get; set; }

        protected override float MaxFilterFreq => SampleRate / 2;

        public float[] BuildFilterReal()
        {
            float[] taps = new float[TapCount];
            float[] window = WindowUtil.MakeWindow(Window, TapCount);

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

        public override void ValidateDecimation(int decimation)
        {

        }

        public HighPassFilterBuilder SetAutomaticTapCount(float transitionWidth, float attenuation = 30)
        {
            _SetAutomaticTapCount(transitionWidth, attenuation);
            return this;
        }

        public HighPassFilterBuilder SetManualTapCount(int taps)
        {
            _SetManualTapCount(taps);
            return this;
        }

        public HighPassFilterBuilder SetWindow(WindowType window = WindowType.BlackmanHarris7)
        {
            _SetWindow(window);
            return this;
        }
    }
}
