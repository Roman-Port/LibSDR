using RomanPort.LibSDR.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Sources.Hardware
{
    public abstract class IHardwareSource : IIQSource
    {
        public abstract long CenterFrequency { get; set; }
        public abstract bool AutoGainEnabled { get; set; }
        public abstract int ManualGainLevel { get; set; }
    }
}
