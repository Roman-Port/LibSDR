using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.General
{
    public class OscillatorAccurate
    {
        public OscillatorAccurate(float sampleRate, float frequency)
        {
            this.sampleRate = sampleRate;
            this.frequency = frequency;
            phase = 0;
            Configure();
        }

        public OscillatorAccurate()
        {

        }

        private float sampleRate;
        private float frequency;

        private double phase;
        private double rotation;

        public double Phase { get => phase; set => phase = value; }

        public float SampleRate
        {
            get => sampleRate;
            set
            {
                sampleRate = value;
                Configure();
            }
        }

        public float Frequency
        {
            get => frequency;
            set
            {
                frequency = value;
                Configure();
            }
        }

        private void Configure()
        {
            if(sampleRate != 0)
                rotation = 2.0 * Math.PI * frequency / sampleRate;
        }

        private void Tick()
        {
            phase += rotation;
            if (Math.Abs(phase) > Math.PI)
            {
                while (phase > Math.PI)
                    phase -= 2 * Math.PI;

                while (phase < -Math.PI)
                    phase += 2 * Math.PI;
            }
        }

        public unsafe void Mix(Complex* ptr, int count)
        {
            Mix(ptr, ptr, count);
        }

        public unsafe void Mix(Complex* input, Complex* output, int count)
        {
            Complex temp;
            for (int i = 0; i < count; i++)
            {
                temp = new Complex(
                    (float)Math.Cos(phase),
                    (float)Math.Sin(phase)
                );
                output[i] = input[i] * temp;
                Tick();
            }
        }

        public unsafe void Mix(float* data, int count)
        {
            Mix(data, data, count);
        }

        public unsafe void Mix(float* input, float* output, int count)
        {
            for (int i = 0; i < count; i++)
            {
                output[i] = (input[i] * (float)Math.Cos(phase));
                Tick();
            }
        }
    }
}
