using RomanPort.LibSDR.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Sources
{
    public interface ISource
    {
        void Open(int bufferSize);
        void BeginStreaming();
        void EndStreaming();
        void Close();
        event SamplesAvailableEventArgs OnSamplesAvailable;
        event SampleRateChangedEventArgs OnSampleRateChanged;
    }

    public unsafe delegate void SamplesAvailableEventArgs(Complex* samples, int count);
    public unsafe delegate void SampleRateChangedEventArgs(float sampleRate);
}
