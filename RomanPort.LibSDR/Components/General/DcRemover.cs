using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RomanPort.LibSDR.Components.General
{
    public unsafe class DcRemover
    {
        private float _average;
        private float _ratio;

        public const float DC_TIME_CONST = 0.00001f;

        public DcRemover(float ratio = DC_TIME_CONST)
        {
            _ratio = ratio;
            _average = 0.0f;
        }

        public void Init(float ratio = DC_TIME_CONST)
        {
            _ratio = ratio;
            _average = 0.0f;
        }

        public float Offset
        {
            get { return _average; }
        }

        public void Process(float* buffer, int length)
        {
            for (var i = 0; i < length; i++)
            {
                _average += _ratio * (buffer[i] - _average);
                buffer[i] -= _average;
            }
        }

        public void ProcessInterleaved(float* buffer, int length)
        {
            length *= 2;

            for (var i = 0; i < length; i += 2)
            {
                _average += _ratio * (buffer[i] - _average);
                buffer[i] -= _average;
            }
        }

        public void Reset()
        {
            _average = 0.0f;
        }
    }
}
