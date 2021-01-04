using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Util;
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

        public static void WritePtrToFile(string path, void* ptr, int len)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            byte[] buffer = new byte[len];
            fixed (byte* outBuffer = buffer)
                Utils.Memcpy(outBuffer, ptr, len);
            fs.Write(buffer, 0, len);
            fs.Flush();
            fs.Close();
        }

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
            string path = "E:\\TEST_OUTPUT_FILE.wav";
            int index = 1;
            while(true)
            {
                try
                {
                    file = new FileStream(path, FileMode.Create);
                    encoder = new WavEncoder(file, sampleRate, channels, 16);
                    break;
                } catch
                {
                    index++;
                    path = $"E:\\TEST_OUTPUT_FILE_{index}.wav";
                }
            }

            //Log
            Console.WriteLine("CREATED TEST OUTPUT AT " + path);
            Console.WriteLine("You shouldn't see this in prod. Sent by RomanPort LibSDR.");
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
