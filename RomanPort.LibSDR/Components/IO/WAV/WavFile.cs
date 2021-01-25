using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace RomanPort.LibSDR.Components.IO.WAV
{
    public unsafe abstract class WavFile : IDisposable
    {
        protected WavFileInfo info;
        protected Stream underlyingStream;
        protected bool disposed;

        protected readonly int bufferSizeBytes;
        protected byte[] buffer;
        protected GCHandle bufferHandle;
        protected byte* bufferPtrByte;
        protected short* bufferPtrShort;
        protected float* bufferPtrFloat;

        public WavFile(Stream underlyingStream, int bufferSize)
        {
            //Set
            this.underlyingStream = underlyingStream;

            //Open buffer
            bufferSizeBytes = bufferSize;
            buffer = new byte[bufferSize];
            bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            bufferPtrByte = (byte*)bufferHandle.AddrOfPinnedObject();
            bufferPtrShort = (short*)bufferHandle.AddrOfPinnedObject();
            bufferPtrFloat = (float*)bufferHandle.AddrOfPinnedObject();
        }

        public int Channels { get => info.channels; }
        public int SampleRate { get => info.sampleRate; }
        public short BitsPerSample { get => info.bitsPerSample; }
        public int BytesPerSample { get => BitsPerSample / 8; }
        public long LengthSamples { get => (underlyingStream.Length - WavHeaderUtil.HEADER_LENGTH) / Channels / BytesPerSample; }
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

        public void Dispose()
        {
            bufferHandle.Free();
            disposed = true;
        }
    }
}
