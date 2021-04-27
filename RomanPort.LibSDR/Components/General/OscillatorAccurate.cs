using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.General
{
    public class OscillatorAccurate
    {
        public OscillatorAccurate(float sampleRate, float frequency)
        {
            phase = 0;
            rotation = 2.0 * Math.PI * frequency / sampleRate;
        }

        private double phase;
        private double rotation;

        public double Phase { get => phase; set => phase = value; }

        void Tick()
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
