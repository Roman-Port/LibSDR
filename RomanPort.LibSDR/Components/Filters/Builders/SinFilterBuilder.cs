using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters.Builders
{
    public class SinFilterBuilder : IFilterBuilderReal
    {
        public SinFilterBuilder(float sampleRate, float frequency, int length)
        {
            SampleRate = sampleRate;
            Frequency = frequency;
            Length = length;
        }

        private int length;

        public float SampleRate { get; set; }
        public float Frequency { get; set; }
        public int Length
        {
            get => length;
            set
            {
                //Validate
                if (value % 2 == 0)
                    throw new ArgumentException("Length should be odd", "length");

                //Set
                length = value;
            }
        }

        private float FreqInRad { get => 2.0f * MathF.PI * Frequency / SampleRate; }
        private int HalfLength { get => Length / 2; }

        public float[] BuildFilterReal()
        {
            var freqInRad = FreqInRad;
            var taps = new float[length];
            for (var i = 0; i <= HalfLength; i++)
            {
                var y = MathF.Sin(freqInRad * i);
                taps[HalfLength + i] = y;
                taps[HalfLength - i] = -y;
            }
            return taps;
        }

        public void ValidateDecimation(int decimation)
        {

        }

        public int GetDecimation(out float outputSampleRate)
        {
            throw new NotSupportedException();
        }
    }
}
