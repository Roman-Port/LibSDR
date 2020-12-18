using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace RomanPort.LibSDR.Framework.Extras
{
    public class SampleThrottle
    {
        public float sampleRatePerMs;
        private Stopwatch timer;
        private long samples;

        public SampleThrottle(float sampleRate)
        {
            sampleRatePerMs = sampleRate / 1000;
            timer = new Stopwatch();
            timer.Start();
        }

        public void Process(long addedSamples)
        {
            //Update
            samples += addedSamples;

            //Calculate
            double targetSamples = timer.ElapsedMilliseconds * sampleRatePerMs;
            double samplesAhead = samples - targetSamples;
            double msAhead = samplesAhead / sampleRatePerMs;
            int msAheadInt = (int)msAhead;

            //Validate
            if (msAheadInt <= 0)
                return;

            //Delay
            Thread.Sleep(msAheadInt);
        }
    }
}
