using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace RomanPort.LibSDR.Components.IO
{
    public class SampleThrottle
    {
        public SampleThrottle(float sampleRate)
        {
            this.sampleRate = sampleRate;
            sync = new Stopwatch();
            sync.Start();
        }

        public float SampleRate
        {
            get => sampleRate;
            set
            {
                sampleRate = value;
                sync.Restart();
                syncSamples = 0;
            }
        }

        private float sampleRate;
        private Stopwatch sync;
        private long syncSamples;

        public void SamplesProcessed(long samples)
        {
            syncSamples += samples;
        }

        public void Throttle()
        {
            double targetSamples = sync.Elapsed.TotalSeconds * sampleRate;
            double samplesAhead = syncSamples - targetSamples;
            int msAhead = (int)((samplesAhead / sampleRate) * 1000);
            if (msAhead > 0)
                Thread.Sleep(msAhead);
        }
    }
}
