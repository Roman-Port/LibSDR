using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters.IIR
{
    public class IirFilter
    {
        public float a0;
        public float a1;
        public float a2;
        public float b1;
        public float b2;

        public float x1;
        public float x2;
        public float y1;
        public float y2;

        public static IirFilter CreateBandPass(float sampleRate, float centerFreq, float bw)
        {
            //Convert from samples to radians
            bw /= sampleRate;
            centerFreq /= sampleRate;

            //Calculate intermediate vars
            float R = 1 - 3 * bw;
            float K = (1 - 2 * R * MathF.Cos(2 * MathF.PI * centerFreq) + R * R) / (2 - 2 * MathF.Cos(2 * MathF.PI * centerFreq));

            //Calculate coeffs
            return new IirFilter
            {
                a0 = 1 - K,
                a1 = 2 * (K - R) * MathF.Cos(2 * MathF.PI * centerFreq),
                a2 = (R * R) - K,
                b1 = 2 * R * MathF.Cos(2 * MathF.PI * centerFreq),
                b2 = -(R * R),
            };
        }

        public static IirFilter CreateBandStop(float sampleRate, float centerFreq, float bw)
        {
            //Convert from samples to radians
            bw /= sampleRate;
            centerFreq /= sampleRate;

            //Calculate intermediate vars
            float R = 1 - 3 * bw;
            float K = (1 - 2 * R * MathF.Cos(2 * MathF.PI * centerFreq) + R * R) / (2 - 2 * MathF.Cos(2 * MathF.PI * centerFreq));

            //Calculate coeffs
            return new IirFilter
            {
                a0 = K,
                a1 = -2 * K * MathF.Cos(2 * MathF.PI * centerFreq),
                a2 = K,
                b1 = 2 * R * MathF.Cos(2 * MathF.PI * centerFreq),
                b2 = -R * R
            };
        }

        public static IirFilter CreateLowPass(float sampleRate, float cutoffFreq)
        {
            //Calculate intermediate vars
            float wc = MathF.Tan(cutoffFreq * MathF.PI / sampleRate);
            float k1 = 1.414213562f * wc;
            float k2 = wc * wc;

            //Calculate coeffs
            float a0 = k2 / (1 + k1 + k2);
            float a1 = 2 * a0;
            float a2 = a0;
            float k3 = a1 / k2;
            float b1 = -2 * a0 + k3;
            float b2 = 1 - (2 * a0) - k3;

            //Build
            return new IirFilter
            {
                a0 = a0,
                a1 = a1,
                a2 = a2,
                b1 = b1,
                b2 = b2
            };
        }

        public float Process(float sample)
        {
            float output = a0 * sample + a1 * x1 + a2 * x2 + b1 * y1 + b2 * y2;

            x2 = x1;
            x1 = sample;
            y2 = y1;
            y1 = output;

            return output;
        }
    }
}
