using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace RomanPort.LibSDR.Framework.Extras
{
    public class Throttle
    {
        public double sampleRate;
        public double samplesProcessed;
        public double offsetSamples;
        public Stopwatch stopwatch;

        public Throttle(double sampleRate, double offsetSamples = 0)
        {
            this.sampleRate = sampleRate;
            this.offsetSamples = offsetSamples;
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        public void Work(double currentSamplesProcessed)
        {
            //Update
            samplesProcessed += currentSamplesProcessed;

            //Get how far behind we are and wait it out
            double desiredSamplesProcessed = GetDesiredSamplesProcessed();
            while (desiredSamplesProcessed < samplesProcessed + offsetSamples)
                desiredSamplesProcessed = GetDesiredSamplesProcessed();
        }

        private double GetDesiredSamplesProcessed()
        {
            return (sampleRate / 1000) * stopwatch.ElapsedMilliseconds;
        }
    }
}
