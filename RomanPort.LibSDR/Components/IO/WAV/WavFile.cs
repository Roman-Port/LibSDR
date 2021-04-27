using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace RomanPort.LibSDR.Components.IO.WAV
{
    public unsafe abstract class WavFile
    {
        protected WavFileInfo info;
        protected Stream underlyingStream;

        public WavFile(Stream underlyingStream)
        {
            this.underlyingStream = underlyingStream;
        }

        public int Channels { get => info.channels; }
        public int SampleRate { get => info.sampleRate; }
        public short BitsPerSample { get => info.bitsPerSample; }
        public int BytesPerSample { get => BitsPerSample / 8; }
        public long LengthSamples { get => (underlyingStream.Length - WavHeaderUtil.HEADER_LENGTH) / Channels / BytesPerSample; }
        public double LengthSeconds { get => (double)LengthSamples / SampleRate; }
        public long PositionSamples
        {
            get => (underlyingStream.Position - WavHeaderUtil.HEADER_LENGTH) / Channels / BytesPerSample;
            set => underlyingStream.Position = WavHeaderUtil.HEADER_LENGTH + (Channels * BytesPerSample * value);
        }
        public double PositionSeconds
        {
            get => (double)PositionSamples / SampleRate;
            set => PositionSamples = (long)(value * SampleRate);
        }
        public SampleFormat Format
        {
            get
            {
                switch (BitsPerSample)
                {
                    case 8: return SampleFormat.Byte;
                    case 16: return SampleFormat.Short16;
                    case 32: return SampleFormat.Float32;
                    default: throw new Exception("Unknown bits per sample!");
                }
            }
        }
    }
}
