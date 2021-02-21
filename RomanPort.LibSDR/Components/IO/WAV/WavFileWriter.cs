using RomanPort.LibSDR.Components.IO.RAW;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace RomanPort.LibSDR.Components.IO.WAV
{
    public unsafe class WavFileWriter : WavFile, IDisposable
    {
        private StreamSampleWriter writer;

        public WavFileWriter(string path, FileMode mode, int sampleRate, short channels, SampleFormat format, int bufferSize) : this(new FileStream(path, mode), sampleRate, channels, format, bufferSize)
        {

        }

        public WavFileWriter(Stream underlyingStream, int sampleRate, short channels, SampleFormat format, int bufferSize) : base(underlyingStream)
        {
            //Create wrapper
            writer = new StreamSampleWriter(underlyingStream, format, sampleRate, WavHeaderUtil.HEADER_LENGTH, channels, bufferSize);

            //Create info
            info = new WavFileInfo
            {
                bitsPerSample = writer.BitsPerSample,
                channels = channels,
                sampleRate = sampleRate
            };

            //Write header
            byte[] header = WavHeaderUtil.CreateHeader(info);
            underlyingStream.Position = 0;
            underlyingStream.Write(header, 0, header.Length);
        }

        public void Write(Complex* ptr, int count)
        {
            writer.Write(ptr, count);
        }
        
        public void Write(float* ptr, int count)
        {
            writer.Write(ptr, count);
        }

        public void FinalizeFile()
        {
            long pos = underlyingStream.Position;
            WavHeaderUtil.UpdateLength(underlyingStream, (int)(pos - WavHeaderUtil.HEADER_LENGTH));
            underlyingStream.Position = pos;
        }

        public void Dispose()
        {
            writer.Dispose();
        }
    }
}
