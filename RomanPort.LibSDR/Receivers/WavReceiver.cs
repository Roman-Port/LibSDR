using RomanPort.LibSDR.Extras;
using RomanPort.LibSDR.Framework.Radio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RomanPort.LibSDR.Receivers
{
    public class WavReceiver : IRadioSampleReceiver, IDisposable
    {
        public WavReceiver(Stream file, short bitsPerSample, bool keepOpen = false)
        {
            this.file = file;
            this.bitsPerSample = bitsPerSample;
            this.keepOpen = keepOpen;
        }

        private WavEncoder encoder;
        private short bitsPerSample;
        private bool keepOpen;
        private Stream file;

        public void Open(float sampleRate, int bufferSize)
        {
            encoder = new WavEncoder(file, (int)sampleRate, 2, bitsPerSample, bufferSize);
        }

        public unsafe void OnSamples(float* left, float* right, int samplesRead)
        {
            encoder.Write(left, right, samplesRead);
        }

        public void Dispose()
        {
            if(!keepOpen)
            {
                file?.Flush();
                file?.Close();
                file?.Dispose();
            }
        }
    }
}
