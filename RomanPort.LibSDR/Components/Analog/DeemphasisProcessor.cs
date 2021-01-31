using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Analog
{
    public class DeemphasisProcessor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sampleRate">Audio sample rate, used for configuring the deemphasis</param>
        /// <param name="time">Time in µs. Most of the world uses 50 µs, but the United States uses 75 µs.</param>
        public DeemphasisProcessor(float sampleRate, float time = 75f)
        {
            Reset(sampleRate, time);
        }

        public DeemphasisProcessor()
        {

        }

        private float sampleRate;
        private float time;

        private float deemphasisAlpha;
        private float deemphasisAvg;

        public float SampleRate
        {
            get => sampleRate;
            set
            {
                sampleRate = value;
                Reset();
            }
        }

        public float Time
        {
            get => time;
            set
            {
                time = value;
                Reset();
            }
        }

        public unsafe void Process(float* audio, int count)
        {
            for (var i = 0; i < count; i++)
            {
                deemphasisAvg += deemphasisAlpha * (audio[i] - deemphasisAvg);
                audio[i] = deemphasisAvg;
            }
        }

        public void Reset()
        {
            if (sampleRate == 0 || time == 0)
            {
                deemphasisAlpha = 0;
                deemphasisAvg = 0;
            }
            else
            {
                deemphasisAlpha = 1.0f - MathF.Exp(-1.0f / (sampleRate * (time * 1e-6f)));
                deemphasisAvg = 0;
            }
        }

        public void Reset(float sampleRate, float time)
        {
            this.sampleRate = sampleRate;
            this.time = time;
            Reset();
        }
    }
}
