using RomanPort.LibSDR.Components.IO.WAV;
using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RomanPort.LibSDR.Benchmarks
{
    public unsafe class BenchmarkData
    {
        public BenchmarkData(string filename, int startSeconds, int lengthSeconds)
        {
            //Open file
            this.filename = filename;
            wav = new WavFileReader(new FileStream(filename, FileMode.Open, FileAccess.Read));
            sampleRate = wav.SampleRate;
            firstSample = startSeconds * sampleRate;
            sampleCount = lengthSeconds * sampleRate;
            samplesPerLogUpdate = sampleCount / 10;

            //Open buffer
            buffer = UnsafeBuffer.Create((int)sampleCount, out ptr);
        }

        public readonly Complex* ptr;
        public readonly long sampleRate;
        public readonly long firstSample;
        public readonly long sampleCount;
        public readonly long samplesPerLogUpdate;
        public readonly string filename;

        private UnsafeBuffer buffer;
        private WavFileReader wav;

        private const long READ_BUFFER_SIZE = 65536;

        public void Load()
        {
            Console.WriteLine($"Using file \"{filename}\", {((sampleCount * 8) / 1000 / 1000)} MB");
            Console.Write("Reading file...");
            wav.PositionSamples = firstSample;
            long remaining = sampleCount;
            Complex* outPtr = ptr;
            int lastProgress = 0;
            long lastProgressSample = 0;
            while (remaining > 0)
            {
                int readable = (int)Math.Min(READ_BUFFER_SIZE, remaining);
                if (readable != wav.Read(outPtr, readable))
                    throw new Exception("Failed to read entire file!");
                outPtr += readable;
                remaining -= readable;
                lastProgressSample += readable;
                if(lastProgressSample > samplesPerLogUpdate)
                {
                    lastProgressSample -= samplesPerLogUpdate;
                    lastProgress++;
                    Console.Write("\rReading file..." + lastProgress + "0%...");
                }
            }
            Console.WriteLine("\rReading file...DONE    ");
        }
    }
}
