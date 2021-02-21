using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace RomanPort.LibSDR.Components.IO.RAW
{
    public unsafe class StreamSampleIO : IDisposable
    {
        public StreamSampleIO(Stream underlyingStream, SampleFormat format, int sampleRate, int headerLength, int channels, int bufferSize)
        {
            this.underlyingStream = underlyingStream;
            this.format = format;
            this.sampleRate = sampleRate;
            this.headerLength = headerLength;
            this.channels = channels;

            //Configure
            switch (format)
            {
                case SampleFormat.Float32:
                    bitsPerSample = 32;
                    break;
                case SampleFormat.Short16:
                    bitsPerSample = 16;
                    break;
                case SampleFormat.Byte:
                    bitsPerSample = 8;
                    break;
                default:
                    throw new Exception("Unknown sample format.");
            }

            //Open buffer
            bufferSizeSamples = bufferSize;
            bufferSizeBytes = bufferSizeSamples * BytesPerSample;
            buffer = new byte[bufferSizeBytes];
            bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            bufferPtrByte = (byte*)bufferHandle.AddrOfPinnedObject();
            bufferPtrShort = (short*)bufferHandle.AddrOfPinnedObject();
            bufferPtrFloat = (float*)bufferHandle.AddrOfPinnedObject();
        }

        private SampleFormat format;
        private int sampleRate;
        private int headerLength;
        private int channels;
        private short bitsPerSample;

        protected Stream underlyingStream;
        protected bool disposed;

        protected readonly int bufferSizeBytes;
        protected readonly int bufferSizeSamples;
        protected byte[] buffer;
        protected GCHandle bufferHandle;
        protected byte* bufferPtrByte;
        protected short* bufferPtrShort;
        protected float* bufferPtrFloat;

        public SampleFormat Format { get => format; }
        public int Channels { get => channels; }
        public int SampleRate { get => sampleRate; }
        public short BitsPerSample { get => bitsPerSample; }
        public int BytesPerSample { get => BitsPerSample / 8; }
        public long LengthSamples { get => (underlyingStream.Length - headerLength) / Channels / BytesPerSample; }
        public long PositionSamples
        {
            get => (underlyingStream.Position - headerLength) / Channels / BytesPerSample;
            set => underlyingStream.Position = headerLength + (Channels * BytesPerSample * value);
        }
        public double PositionSeconds
        {
            get => (double)PositionSamples / SampleRate;
            set => PositionSamples = (long)(value * SampleRate);
        }

        public void Dispose()
        {
            bufferHandle.Free();
            disposed = true;
        }
    }
}
