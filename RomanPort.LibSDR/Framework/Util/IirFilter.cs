using RomanPort.LibSDR.Components;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RomanPort.LibSDR.Framework.Util
{
    public enum IirFilterType
    {
        LowPass,
        HighPass,
        BandPass,
        Notch
    }

    // http://www.musicdsp.org/files/Audio-EQ-Cookbook.txt

#if !__MonoCS__
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
#endif
    public unsafe struct IirFilter
    {
        private float _a0;
        private float _a1;
        private float _a2;
        private float _b0;
        private float _b1;
        private float _b2;

        private float _x1;
        private float _x2;
        private float _y1;
        private float _y2;

        public void Init(IirFilterType filterType, float frequency, float sampleRate, int qualityFactor)
        {
            var w0 = 2.0f * MathF.PI * frequency / sampleRate;
            var alpha = MathF.Sin(w0) / (2.0f * qualityFactor);

            switch (filterType)
            {
                case IirFilterType.LowPass:
                    _b0 = (1.0f - MathF.Cos(w0)) / 2.0f;
                    _b1 = 1.0f - MathF.Cos(w0);
                    _b2 = (1.0f - MathF.Cos(w0)) / 2.0f;
                    _a0 = 1.0f + alpha;
                    _a1 = -2.0f * MathF.Cos(w0);
                    _a2 = 1.0f - alpha;
                    break;

                case IirFilterType.HighPass:
                    _b0 = (1.0f + MathF.Cos(w0)) / 2.0f;
                    _b1 = -(1.0f + MathF.Cos(w0));
                    _b2 = (1.0f + MathF.Cos(w0)) / 2.0f;
                    _a0 = 1.0f + alpha;
                    _a1 = -2.0f * MathF.Cos(w0);
                    _a2 = 1.0f - alpha;
                    break;

                //case IirFilterType.BandPass:
                default:
                    _b0 = alpha;
                    _b1 = 0.0f;
                    _b2 = -alpha;
                    _a0 = 1.0f + alpha;
                    _a1 = -2.0f * MathF.Cos(w0);
                    _a2 = 1.0f - alpha;
                    break;

                case IirFilterType.Notch:
                    _b0 = 1.0f;
                    _b1 = -2.0f * MathF.Cos(w0);
                    _b2 = 1.0f;
                    _a0 = 1.0f + alpha;
                    _a1 = -2.0f * MathF.Cos(w0);
                    _a2 = 1.0f - alpha;
                    break;
            }

            _b0 /= _a0;
            _b1 /= _a0;
            _b2 /= _a0;
            _a1 /= _a0;
            _a2 /= _a0;

            _x1 = 0;
            _x2 = 0;
            _y1 = 0;
            _y2 = 0;
        }

        public void Reset()
        {
            _x1 = 0;
            _x2 = 0;
            _y1 = 0;
            _y2 = 0;
        }

        public float Process(float sample)
        {
            var result = _b0 * sample + _b1 * _x1 + _b2 * _x2
                                      - _a1 * _y1 - _a2 * _y2;
            _x2 = _x1;
            _x1 = sample;

            _y2 = _y1;
            _y1 = result;

            return result;
        }

        public void Process(float* buffer, int length)
        {
            for (var i = 0; i < length; i++)
            {
                buffer[i] = Process(buffer[i]);
            }
        }

        public void ProcessInterleaved(float* buffer, int length)
        {
            length *= 2;
            for (var i = 0; i < length; i += 2)
            {
                buffer[i] = Process(buffer[i]);
            }
        }
    }
}
