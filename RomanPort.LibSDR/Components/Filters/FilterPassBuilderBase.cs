using RomanPort.LibSDR.Components.Decimators;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters
{
    public abstract class FilterPassBuilderBase : IFilterBuilder
    {
        public FilterPassBuilderBase(float sampleRate)
        {
            SampleRate = sampleRate;
        }

        public float SampleRate { get; set; }

        private bool tapsSet;
        private int taps;
        private float transitionWidth;

        public int TapCount
        {
            get
            {
                if (!tapsSet)
                    throw new Exception("TapCount is not set. Cannot build filter!");
                return taps;
            }
            set
            {
                tapsSet = true;
                taps = value;
            }
        }
        protected float TransitionWidth { get => transitionWidth; }

        public WindowType Window { get; set; } = WindowType.None;

        protected void _SetAutomaticTapCount(float transitionWidth, float attenuation = 30)
        {
            //Validate
            if (transitionWidth <= 0)
                throw new Exception("TransitionWidth cannot be less than or equal to zero. A low value will also cause a large number of taps.");

            //Calculate
            int count = (int)(attenuation / (22 * (transitionWidth / SampleRate)));
            if ((count & 1) == 0) //If this is odd, make it even
                count++;
            TapCount = count;

            //Set
            this.transitionWidth = transitionWidth;
        }

        protected void _SetManualTapCount(int taps)
        {
            TapCount = taps;
        }

        protected void _SetWindow(WindowType window = WindowType.BlackmanHarris7)
        {
            Window = window;
        }

        protected float[] UtilBuildWindow()
        {
            return WindowUtil.MakeWindow(Window, TapCount);
        }

        protected abstract float MaxFilterFreq { get; }

        public int GetDecimation(out float outputSampleRate)
        {
            float bw = MaxFilterFreq + TransitionWidth + TransitionWidth;
            return DecimationUtil.CalculateDecimationRate(SampleRate, bw, out outputSampleRate);
        }

        public abstract void ValidateDecimation(int decimation);
    }
}
