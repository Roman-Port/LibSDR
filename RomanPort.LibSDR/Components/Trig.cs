using RomanPort.LibSDR.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components
{
    public static unsafe class Trig
    {
        private const int ResolutionInBits = 16;

        private static readonly int _mask;
        private static readonly float _indexScale;
        private static readonly UnsafeBuffer _sinBuffer;
        private static readonly UnsafeBuffer _cosBuffer;
        private static readonly float* _sinPtr;
        private static readonly float* _cosPtr;

        static Trig()
        {
            _mask = ~(-1 << ResolutionInBits);
            var sampleCount = _mask + 1;

            _sinBuffer = UnsafeBuffer.Create(sampleCount, sizeof(float));
            _cosBuffer = UnsafeBuffer.Create(sampleCount, sizeof(float));
            _sinPtr = (float*)_sinBuffer;
            _cosPtr = (float*)_cosBuffer;

            const float twoPi = MathF.PI * 2.0f;
            const float pi2 = MathF.PI / 2.0f;
            _indexScale = sampleCount / twoPi;

            for (var i = 0; i < sampleCount; i++)
            {
                _sinPtr[i] = MathF.Sin((i + 0.5f) / sampleCount * twoPi);
                _cosPtr[i] = MathF.Cos((i + 0.5f) / sampleCount * twoPi);
            }

            for (var angle = 0.0f; angle < twoPi; angle += pi2)
            {
                _sinPtr[(int)(angle * _indexScale) & _mask] = MathF.Sin(angle);
                _cosPtr[(int)(angle * _indexScale) & _mask] = MathF.Cos(angle);
            }
        }

        public static float Sin(float angle)
        {
            return _sinPtr[(int)(angle * _indexScale) & _mask];
        }

        public static float Cos(float angle)
        {
            return _cosPtr[(int)(angle * _indexScale) & _mask];
        }

        public static Complex SinCos(float rad)
        {
            var index = (int)(rad * _indexScale) & _mask;
            Complex result;
            result.Real = _cosPtr[index];
            result.Imag = _sinPtr[index];
            return result;
        }

        public static float Atan2(float y, float x)
        {
            const float pi = MathF.PI;
            const float pi2 = MathF.PI / 2.0f;

            float angle;
            if (x == 0.0)
            {
                if (y > 0.0)
                    return pi2;
                if (y == 0.0)
                    return 0.0f;
                return -pi2;
            }
            float z = y / x;
            if (z < 1 && z > -1) //previously "MathF.Abs(z) < 1"
            {
                angle = z / (1.0f + 0.2854f * z * z);
                if (x < 0.0)
                {
                    if (y < 0.0)
                        return angle - pi;
                    return angle + pi;
                }
            }
            else
            {
                angle = pi2 - z / (z * z + 0.2854f);
                if (y < 0.0)
                    return angle - pi;
            }
            return angle;
        }
    }
}
