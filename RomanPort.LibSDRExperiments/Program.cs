using RomanPort.LibSDR.Extras;
using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Util;
using RomanPort.LibSDR.Sources.Hardware.AirSpy;
using System;
using System.IO;

namespace RomanPort.LibSDRExperiments
{
    unsafe class Program
    {
        public const int BUFFER_SIZE = 65536;

        static void Main(string[] args)
        {
            //Buffer
            UnsafeBuffer buffer = UnsafeBuffer.Create(BUFFER_SIZE, sizeof(Complex));
            Complex* bufferPtr = (Complex*)buffer;

            //Radio
            AirSpySource s = new AirSpySource();
            s.OnSamplesDropped += S_OnSamplesDropped;
            float rate = s.Open(BUFFER_SIZE);
            s.CenterFrequency = 93700000;

            //Log
            Console.WriteLine("Supported (offical) sample rates:");
            foreach (var sRate in s.CompatibleSampleRates)
                Console.WriteLine("\t" + (sRate / 1000 / 1000) + " MHz");
            Console.WriteLine("Current sample rate:\n\t" + (rate / 1000 / 1000) + " MHz");
            Console.WriteLine("Current center freq:\n\t" + s.CenterFrequency);
            Console.WriteLine("");

            //Output
            WavEncoder encoder = new WavEncoder(new FileStream("E:\\test_file.wav", FileMode.Create), (int)rate, 2, 16);

            //Loop
            Console.Write("\rWriting...");
            long totalRead = 0;
            int read;
            while(true)
            {
                read = s.Read(bufferPtr, BUFFER_SIZE);
                encoder.Write(bufferPtr, read);
                Console.Write("\rWriting..." + (totalRead += read) + " samples written");
            }
        }

        private static void S_OnSamplesDropped(long dropped)
        {
            Console.Write($"\rWARNING: Dropped {dropped} samples!   \n");
        }
    }
}
