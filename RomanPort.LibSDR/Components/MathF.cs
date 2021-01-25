using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components
{
    /// <summary>
    /// Wrapper for .NET Standard 2.1 MathF class. Unfortunately, we can't actually upgrade to 2.1, as it drops support for .NET Framework...
    /// 
    /// Performance on x64 is significantly faster using doubles than floats, but it is slower on ARM. This is a compromise that puts us in the middle.
    /// </summary>
    public static class MathF
    {
        public const float PI = (float)Math.PI;

        public static float Exp(float x)
        {
            return (float)Math.Exp(x);
        }

        public static float Abs(float x)
        {
            return (float)Math.Abs(x);
        }

        public static float Floor(float x)
        {
            return (float)Math.Floor(x);
        }

        public static float Cos(float x)
        {
            return (float)Math.Cos(x);
        }

        public static float Sin(float x)
        {
            return (float)Math.Sin(x);
        }

        public static float Log10(float x)
        {
            return (float)Math.Log10(x);
        }

        public static float Sqrt(float x)
        {
            return (float)Math.Sqrt(x);
        }

        public static float Atan2(float x, float z)
        {
            return (float)Math.Atan2(x, z);
        }
    }
}
