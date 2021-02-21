using RomanPort.LibSDR.Components.IO.RAW;
using RomanPort.LibSDR.Sources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace RomanPort.LibSDR.Components.IO.WAV
{
    public unsafe class WavFileReader : WavFile, ISampleReader, IDisposable
    {
        private StreamSampleReader reader;
        
        public WavFileReader(Stream underlyingStream, int bufferSize = 16384) : base(underlyingStream)
        {
            //Read WAV header
            byte[] header = new byte[WavHeaderUtil.HEADER_LENGTH];
            underlyingStream.Read(header, 0, WavHeaderUtil.HEADER_LENGTH);
            if (!WavHeaderUtil.ParseWavHeader(header, out info))
                throw new Exception("This is not a valid WAV header.");

            //Create wrapper
            reader = new StreamSampleReader(underlyingStream, Format, SampleRate, WavHeaderUtil.HEADER_LENGTH, Channels, bufferSize);
        }

        public static bool IdentifyWavFile(string path, out WavFileInfo info)
        {
            bool ok;
            byte[] header = new byte[WavHeaderUtil.HEADER_LENGTH];
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                fs.Read(header, 0, WavHeaderUtil.HEADER_LENGTH);
                ok = WavHeaderUtil.ParseWavHeader(header, out info);
            }
            return ok;
        }

        public void Dispose()
        {
            reader.Dispose();
        }

        public int Read(Complex* iq, int count)
        {
            return Read((float*)iq, count * 2) / 2;
        }

        public int Read(float* ptr, int count)
        {
            return reader.Read(ptr, count);
        }
    }
}
