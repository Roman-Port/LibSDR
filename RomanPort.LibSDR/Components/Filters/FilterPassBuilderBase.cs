using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters
{
    public abstract class FilterPassBuilderBase : FilterBuilderBase
    {
        public FilterPassBuilderBase(float sampleRate) : base(sampleRate)
        {

        }

        private bool tapsSet;
        private int taps;

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
        public WindowType Window { get; set; } = WindowType.None;

        public FilterPassBuilderBase SetAutomaticTapCount(float transitionWidth, float attenuation = 30)
        {
            //Validate
            if (transitionWidth <= 0)
                throw new Exception("TransitionWidth cannot be less than or equal to zero. A low value will also cause a large number of taps.");

            //Calculate
            int count = (int)(attenuation / (22 * (transitionWidth / SampleRate)));
            if ((count & 1) == 0) //If this is odd, make it even
                count++;
            TapCount = count;

            return this;
        }

        public FilterPassBuilderBase SetManualTapCount(int taps)
        {
            TapCount = taps;
            return this;
        }

        public FilterPassBuilderBase SetWindow(WindowType window = WindowType.Hamming)
        {
            Window = window;
            return this;
        }

        protected float[] UtilBuildWindow()
        {
            return WindowUtil.MakeWindow(Window, TapCount);
        }
    }
}
