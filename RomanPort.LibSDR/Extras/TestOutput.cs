using RomanPort.LibSDR.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RomanPort.LibSDR.Extras
{
    public unsafe class TestOutput
    {
        private WavEncoder encoder;
        private FileStream file;

        private static TestOutput globalTest;

        public static TestOutput GetGlobalTestFile(int sampleRate, short channels = 2)
        {
            if (globalTest == null)
                globalTest = new TestOutput(sampleRate, channels);
            return globalTest;
        }

        public static TestOutput GetGlobalTestFile()
        {
            if (globalTest == null)
                throw new Exception("No global test set!");
            return globalTest;
        }

        public TestOutput(int sampleRate, short channels = 2)
        {
            //Get file path
            string path = "E:\\test_" + DateTime.UtcNow.Ticks + ".wav";
            Console.WriteLine("CREATED TEST OUTPUT AT " + path);
            Console.WriteLine("You shouldn't see this in prod. Sent by RomanPort LibSDR.");

            //Open
            file = new FileStream(path, FileMode.Create);
            encoder = new WavEncoder(file, sampleRate, channels, 16);
        }

        public void WriteSamples(Complex* ptr, int count)
        {
            encoder.Write(ptr, count);
        }

        public void WriteSamples(float* ptr, int count)
        {
            encoder.Write(ptr, count);
        }
    }
}
