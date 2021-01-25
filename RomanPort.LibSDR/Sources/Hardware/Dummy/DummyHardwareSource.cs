using RomanPort.LibSDR.Sources.Misc;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Sources.Hardware.Dummy
{
    /// <summary>
    /// Acts as a fake hardware source that is actually reading from an IQ file. Can be used for testing
    /// </summary>
    public class DummyHardwareSource : WavStreamSource, IHardwareSource
    {
        public DummyHardwareSource(string path) : base(path)
        {
        }

        public long CenterFrequency { get; set; }
        public bool AutoGainEnabled { get; set; }
        public int ManualGainLevel { get; set; }

        public long TotalDroppedSamples => 0;

        float IHardwareSource.SampleRate { get => SampleRate; set => throw new NotSupportedException(); }

        public event HardwareSourceSamplesDroppedArgs OnSamplesDropped;
    }
}
