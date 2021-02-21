using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.IO
{
    public interface ISampleReader
    {
        unsafe int Read(Complex* iq, int count);
        unsafe int Read(float* ptr, int count);

        int Channels { get; }
        int SampleRate { get; }
        short BitsPerSample { get; }
        int BytesPerSample { get; }
        long LengthSamples { get; }
        long PositionSamples { get; set; }
        double PositionSeconds { get; set; }
    }
}
