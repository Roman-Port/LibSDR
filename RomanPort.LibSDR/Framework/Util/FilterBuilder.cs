using RomanPort.LibSDR.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework.Util
{
    public enum WindowType
    {
        None,
        Hamming,
        Blackman,
        BlackmanHarris4,
        BlackmanHarris7,
        HannPoisson,
        Youssef
    }

    public static class FilterBuilder
    {
        public const int DefaultFilterOrder = 500;

        public static int CalculateFilterOrder(double sampleRate, float transitionWidth, float attenuation)
        {
            //Based on formula from Multirate Signal Processing for Communications Systems, Fredric J Harris
            int count = (int)(attenuation * sampleRate / (22 * transitionWidth));
            if ((count & 1) == 0) //If this is odd, make it even
                count++;
            return count;
        }

        public unsafe static float[] MakeWindow(WindowType windowType, int length)
        {
            float[] buf = new float[length];
            fixed (float* bufPtr = buf)
                MakeWindow(windowType, length, bufPtr);
            return buf;
        }

        public unsafe static void MakeWindow(WindowType windowType, int length, float* w)
        {
            length--;
            for (var i = 0; i <= length; i++)
            {
                float n;
                float a0;
                float a1;
                float a2;
                float a3;
                float a4;
                float a5;
                float a6;
                float alpha;

                w[i] = 1.0f;

                switch (windowType)
                {
                    case WindowType.Hamming:
                        a0 = 0.54f;
                        a1 = 0.46f;
                        a2 = 0.0f;
                        a3 = 0.0f;
                        w[i] *= a0
                              - a1 * MathF.Cos(2.0f * MathF.PI * i / length)
                              + a2 * MathF.Cos(4.0f * MathF.PI * i / length)
                              - a3 * MathF.Cos(6.0f * MathF.PI * i / length);
                        break;

                    case WindowType.Blackman:
                        a0 = 0.42f;
                        a1 = 0.5f;
                        a2 = 0.08f;
                        a3 = 0.0f;
                        w[i] *= a0
                              - a1 * MathF.Cos(2.0f * MathF.PI * i / length)
                              + a2 * MathF.Cos(4.0f * MathF.PI * i / length)
                              - a3 * MathF.Cos(6.0f * MathF.PI * i / length);
                        break;

                    case WindowType.BlackmanHarris4:
                        a0 = 0.35875f;
                        a1 = 0.48829f;
                        a2 = 0.14128f;
                        a3 = 0.01168f;
                        w[i] *= a0
                              - a1 * MathF.Cos(2.0f * MathF.PI * i / length)
                              + a2 * MathF.Cos(4.0f * MathF.PI * i / length)
                              - a3 * MathF.Cos(6.0f * MathF.PI * i / length);
                        break;

                    case WindowType.BlackmanHarris7:
                        a0 = 0.27105140069342f;
                        a1 = 0.43329793923448f;
                        a2 = 0.21812299954311f;
                        a3 = 0.06592544638803f;
                        a4 = 0.01081174209837f;
                        a5 = 0.00077658482522f;
                        a6 = 0.00001388721735f;
                        w[i] *= a0
                              - a1 * MathF.Cos(2.0f * MathF.PI * i / length)
                              + a2 * MathF.Cos(4.0f * MathF.PI * i / length)
                              - a3 * MathF.Cos(6.0f * MathF.PI * i / length)
                              + a4 * MathF.Cos(8.0f * MathF.PI * i / length)
                              - a5 * MathF.Cos(10.0f * MathF.PI * i / length)
                              + a6 * MathF.Cos(12.0f * MathF.PI * i / length);
                        break;

                    case WindowType.HannPoisson:
                        n = i - length / 2.0f;
                        alpha = 0.005f;
                        w[i] *= 0.5f * (1.0f + MathF.Cos(2.0f * MathF.PI * n / length)) * MathF.Exp(-2.0f * alpha * MathF.Abs(n) / length);
                        break;

                    case WindowType.Youssef:
                        a0 = 0.35875f;
                        a1 = 0.48829f;
                        a2 = 0.14128f;
                        a3 = 0.01168f;
                        n = i - length / 2.0f;
                        alpha = 0.005f;
                        w[i] *= a0
                              - a1 * MathF.Cos(2.0f * MathF.PI * i / length)
                              + a2 * MathF.Cos(4.0f * MathF.PI * i / length)
                              - a3 * MathF.Cos(6.0f * MathF.PI * i / length);
                        w[i] *= MathF.Exp(-2.0f * alpha * MathF.Abs(n) / length);
                        break;
                }
            }
        }

        public static float[] MakeRootRaisedCosine(float gain, float sampling_freq, float symbol_rate, float alpha, int ntaps)
        {
            ntaps |= 1; // ensure that ntaps is odd

            float spb = sampling_freq / symbol_rate; // samples per bit/symbol
            float[] taps = new float[ntaps];
            float scale = 0;
            for (int i = 0; i < ntaps; i++)
            {
                float x1, x2, x3, num, den;
                float xindx = i - ntaps / 2;
                x1 = MathF.PI * xindx / spb;
                x2 = 4 * alpha * xindx / spb;
                x3 = x2 * x2 - 1;

                if (MathF.Abs(x3) >= 0.000001)
                {
                    if (i != ntaps / 2)
                        num = MathF.Cos((1 + alpha) * x1) +
                              MathF.Sin((1 - alpha) * x1) / (4 * alpha * xindx / spb);
                    else
                        num = MathF.Cos((1 + alpha) * x1) + (1 - alpha) * MathF.PI / (4 * alpha);
                    den = x3 * MathF.PI;
                }
                else
                {
                    if (alpha == 1)
                    {
                        taps[i] = -1;
                        scale += taps[i];
                        continue;
                    }
                    x3 = (1 - alpha) * x1;
                    x2 = (1 + alpha) * x1;
                    num = (MathF.Sin(x2) * (1 + alpha) * MathF.PI -
                           MathF.Cos(x3) * ((1 - alpha) * MathF.PI * spb) / (4 * alpha * xindx) +
                           MathF.Sin(x3) * spb * spb / (4 * alpha * xindx * xindx));
                    den = -32 * MathF.PI * alpha * alpha * xindx / spb;
                }
                taps[i] = 4 * alpha * num / den;
                scale += taps[i];
            }

            for (int i = 0; i < ntaps; i++)
                taps[i] = taps[i] * gain / scale;

            return taps;
        }

        public static float[] MakeSinc(double sampleRate, double frequency, int length)
        {
            if (length % 2 == 0)
            {
                throw new ArgumentException("Length should be odd", "length");
            }

            var freqInRad = 2.0 * MathF.PI * frequency / sampleRate;
            var h = new float[length];

            for (var i = 0; i < length; i++)
            {
                var n = i - length / 2;
                if (n == 0)
                {
                    h[i] = (float)freqInRad;
                }
                else
                {
                    h[i] = (float)(Math.Sin(freqInRad * n) / n);
                }
            }

            return h;
        }

        public static float[] MakeSin(float sampleRate, float frequency, int length)
        {
            if (length % 2 == 0)
            {
                throw new ArgumentException("Length should be odd", "length");
            }

            var freqInRad = 2.0f * MathF.PI * frequency / sampleRate;
            var h = new float[length];

            var halfLength = length / 2;
            for (var i = 0; i <= halfLength; i++)
            {
                var y = MathF.Sin(freqInRad * i);
                h[halfLength + i] = y;
                h[halfLength - i] = -y;
            }

            return h;
        }

        public static float[] MakeLowPassKernel(double sampleRate, int cutoffFrequency, WindowType windowType)
        {
            int count = FilterBuilder.CalculateFilterOrder(sampleRate, cutoffFrequency, 92);
            return MakeLowPassKernel(sampleRate, count, cutoffFrequency, windowType);
        }

        public static float[] MakeLowPassKernel(double sampleRate, int filterOrder, int cutoffFrequency, WindowType windowType)
        {
            filterOrder |= 1;

            var h = MakeSinc(sampleRate, cutoffFrequency, filterOrder);
            var w = MakeWindow(windowType, filterOrder);

            ApplyWindow(h, w);

            Normalize(h);

            return h;
        }

        public static float[] MakeHighPassKernel(double sampleRate, int filterOrder, int cutoffFrequency, WindowType windowType)
        {
            return InvertSpectrum(MakeLowPassKernel(sampleRate, filterOrder, cutoffFrequency, windowType));
        }

        public static float[] MakeBandPassKernel(double sampleRate, int cutoff1, int cutoff2, WindowType windowType)
        {
            int count = FilterBuilder.CalculateFilterOrder(sampleRate, MathF.Abs(cutoff2 - cutoff1), 92);
            return MakeBandPassKernel(sampleRate, count, cutoff1, cutoff2, windowType);
        }

        public static float[] MakeBandPassKernel(double sampleRate, int filterOrder, int cutoff1, int cutoff2, WindowType windowType)
        {
            var bw = (cutoff2 - cutoff1) / 2;
            var fshift = cutoff2 - bw;
            var shiftRadians = 2 * MathF.PI * fshift / sampleRate;

            var h = MakeLowPassKernel(sampleRate, filterOrder, bw, windowType);

            for (var i = 0; i < h.Length; i++)
            {
                var n = i - filterOrder / 2;
                h[i] *= (float)(2 * Math.Cos(shiftRadians * n));
            }
            return h;
        }

        #region Utility functions

        public static void Normalize(float[] h)
        {
            // Normalize the filter kernel for unity gain at DC
            var sum = 0.0f;
            for (var i = 0; i < h.Length; i++)
            {
                sum += h[i];
            }
            for (var i = 0; i < h.Length; i++)
            {
                h[i] /= sum;
            }
        }

        public static void ApplyWindow(float[] coefficients, float[] window)
        {
            for (var i = 0; i < coefficients.Length; i++)
            {
                coefficients[i] *= window[i];
            }
        }

        // See the bottom of
        // http://www.dspguide.com/ch14/4.htm
        // for an explanation of spectral inversion
        private static float[] InvertSpectrum(float[] h)
        {
            for (var i = 0; i < h.Length; i++)
            {
                h[i] = -h[i];
            }
            h[(h.Length - 1) / 2] += 1.0f;
            return h;
        }

        #endregion
    }
}
