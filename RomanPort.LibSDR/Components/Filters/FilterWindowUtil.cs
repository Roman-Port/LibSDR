using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters
{
    public static class FilterWindowUtil
    {
        public static void ApplyWindow(float[] coefficients, float[] window)
        {
            for (var i = 0; i < coefficients.Length; i++)
            {
                coefficients[i] *= window[i];
            }
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
    }
}
