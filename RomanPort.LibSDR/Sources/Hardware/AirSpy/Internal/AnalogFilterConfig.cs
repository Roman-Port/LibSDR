using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Sources.Hardware.AirSpy.Internal
{
    public struct AnalogFilterConfig
    {
        public byte lpf;
        public byte hpf;
        public int shift;
    }
}
