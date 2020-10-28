using NAudio.Wave;
using RomanPort.LibSDR.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanPort.IQFileIndexingTool
{
    /*public unsafe class LiveAudioSupplier : IWaveProvider
    {
        private WaveFormat _fmt;
        private CircularBuffer<byte> buffer;

        private const int BYTES_PER_SAMPLE = 8;

        public LiveAudioSupplier(int sampleRate, int bufferedSeconds)
        {
            _fmt = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 2);
            buffer = new CircularBuffer<byte>(sampleRate * bufferedSeconds * BYTES_PER_SAMPLE);
        }

        public WaveFormat WaveFormat => _fmt;

        public int Read(byte[] buffer, int offset, int count)
        {
            this.buffer.Read()
        }

        public void WriteData(float* samples, int count)
        {
            buffer.Write(samples, count);
        }
    }*/
}
