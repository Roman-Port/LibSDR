using RomanPort.LibSDR.Components.Decimators;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters.Builders
{
    public class LowPassFilterBuilder : FilterPassBuilderBase, IFilterBuilderReal, IFilterBuilderComplex
    {
        public LowPassFilterBuilder(float sampleRate, int cutoffFreq) : base(sampleRate)
        {
            CutoffFreq = cutoffFreq;
        }

        public int CutoffFreq { get; set; }
        public float Gain { get; set; } = 1;

        protected override float MaxFilterFreq => CutoffFreq;

        public float[] BuildFilterReal()
        {
            float[] taps = new float[TapCount];
            float[] window = WindowUtil.MakeWindow(Window, TapCount);

            int M = (TapCount - 1) / 2;
            float fwT0 = 2 * MathF.PI * CutoffFreq / SampleRate;

            for (int n = -M; n <= M; n++)
            {
                if (n == 0)
                    taps[n + M] = fwT0 / MathF.PI * window[n + M];
                else
                {
                    taps[n + M] = MathF.Sin(n * fwT0) / (n * MathF.PI) * window[n + M];
                }
            }

            float fmax = taps[0 + M];
            for (int n = 1; n <= M; n++)
                fmax += 2 * taps[n + M];

            float gain = 1f / fmax;
            for (int i = 0; i < TapCount; i++)
                taps[i] *= gain;

            return taps;
        }

        public Complex[] BuildFilterComplex()
        {
            //Convert to complex
            float[] realTaps = BuildFilterReal();
            Complex[] complexTaps = new Complex[realTaps.Length];
            for (int i = 0; i < realTaps.Length; i++)
                complexTaps[i] = new Complex(realTaps[i], realTaps[i]); //new Complex(realTaps[i], 0); maybe? seems to fix crackle if I use the current method
            return complexTaps;
        }

        public override void ValidateDecimation(int decimation)
        {
            float decimatedSampleRate = SampleRate / decimation;
            if (decimatedSampleRate < (CutoffFreq * 2))
                throw new Exception($"Decimation rate is invalid. Decimated sample rate (SampleRate / decimation, {decimatedSampleRate}) must be greater than or equal to cutoff freq * 2 ({CutoffFreq * 2}). Failiing to do this causes a problem known as aliasing, which leads to arifacting. The most likely cause of this problem is calculating the decimation rate from the cutoff freq. In this case, multiply the cutoff freq by two.");
        }

        public LowPassFilterBuilder SetAutomaticTapCount(float transitionWidth, float attenuation = 30)
        {
            _SetAutomaticTapCount(transitionWidth, attenuation);
            return this;
        }

        public LowPassFilterBuilder SetManualTapCount(int taps)
        {
            _SetManualTapCount(taps);
            return this;
        }

        public LowPassFilterBuilder SetWindow(WindowType window = WindowType.BlackmanHarris7)
        {
            _SetWindow(window);
            return this;
        }
    }
}
