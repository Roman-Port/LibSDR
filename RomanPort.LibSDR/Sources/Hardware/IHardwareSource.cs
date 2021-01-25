using RomanPort.LibSDR.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Sources.Hardware
{
    public interface IHardwareSource : ISource
    {
        long CenterFrequency { get; set; }
        bool AutoGainEnabled { get; set; }
        int ManualGainLevel { get; set; }
        long TotalDroppedSamples { get; }
        float SampleRate { get; set; }
        event HardwareSourceSamplesDroppedArgs OnSamplesDropped;
    }

    public delegate void HardwareSourceSamplesDroppedArgs(long dropped);
}
