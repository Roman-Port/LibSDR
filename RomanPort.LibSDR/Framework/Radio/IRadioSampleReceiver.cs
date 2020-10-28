using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework.Radio
{
    /// <summary>
    /// Users: Assume this will always be two channels
    /// </summary>
    public interface IRadioSampleReceiver
    {
        void Open(float sampleRate, int bufferSize);
        unsafe void OnSamples(float* left, float* right, int samplesRead);
    }
}
