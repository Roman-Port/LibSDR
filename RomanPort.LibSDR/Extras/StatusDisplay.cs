using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Extras
{
    /// <summary>
    /// Renders stats to the console. Just for debugging.
    /// </summary>
    public class StatusDisplay
    {
        private DateTime startTime;
        private DateTime lastUpdate;
        private long samplesProcessed;
        private int sampleRate;

        public StatusDisplay(int sampleRate)
        {
            this.sampleRate = sampleRate;
            startTime = DateTime.UtcNow;
            RenderUpdate();
        }

        public void OnSamples(long sampleCount)
        {
            samplesProcessed += sampleCount;
            if ((DateTime.UtcNow - lastUpdate).TotalMilliseconds > 100)
                RenderUpdate();
        }

        private void RenderUpdate()
        {
            int timerSeconds = (int)(samplesProcessed / sampleRate);
            double timeSinceStart = Math.Max((DateTime.UtcNow - startTime).TotalSeconds, 0.000000000001f);
            double speed = (samplesProcessed / sampleRate) / timeSinceStart;
            Console.Write($"\rWORKING - {timerSeconds}s processed - {(int)timeSinceStart}s elapsed - {Math.Round(speed, 2)}x speed         ");
            lastUpdate = DateTime.UtcNow;
        }
    }
}
