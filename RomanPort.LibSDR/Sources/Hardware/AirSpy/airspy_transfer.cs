using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Sources.Hardware.AirSpy
{
    public struct airspy_transfer
    {
        public IntPtr device;
        public IntPtr ctx;
        public unsafe void* samples;
        public int sample_count;
        public ulong dropped_samples;
        public airspy_sample_type sample_type;
    }
}
