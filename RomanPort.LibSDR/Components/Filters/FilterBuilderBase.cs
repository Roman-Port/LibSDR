using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters
{
    public abstract class FilterBuilderBase
    {
        public FilterBuilderBase(float sampleRate)
        {
            SampleRate = sampleRate;
        }

        private bool tapsSet;
        private int taps;

        public float SampleRate { get; set; }
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

        public FilterBuilderBase SetAutomaticTapCount(float transitionWidth, float attenuation = 30)
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

        public FilterBuilderBase SetManualTapCount(int taps)
        {
            TapCount = taps;
            return this;
        }

        public FilterBuilderBase SetWindow(WindowType window = WindowType.Hamming)
        {
            Window = window;
            return this;
        }
        
        public abstract float[] BuildFilter();

        protected float[] UtilBuildWindow()
        {
            return FilterWindowUtil.MakeWindow(Window, TapCount);
        }
    }
}
