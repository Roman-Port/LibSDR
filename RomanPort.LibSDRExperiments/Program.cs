using RomanPort.LibSDR.Components;
using RomanPort.LibSDR.Components.Analog.Primitive;
using RomanPort.LibSDR.Components.Digital;
using RomanPort.LibSDR.Components.Filters.Builders;
using RomanPort.LibSDR.Components.Filters.FIR;
using RomanPort.LibSDR.Components.Filters.IIR;
using RomanPort.LibSDR.Components.General;
using RomanPort.LibSDR.Components.IO.WAV;
using RomanPort.LibSDR.Demodulators.Analog.Video;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace RomanPort.LibSDRExperiments
{
    unsafe class Program
    {
        public const int SAMPLE_RATE = 44100;
        public const int BAUD_RATE = 4160;
        public const int BUFFER_SIZE = SAMPLE_RATE;

        static void Main(string[] args)
        {
            //Open
            FileStream fsIn = new FileStream("F:\\NOAAAPT_Sound.raw", FileMode.Open);
            WavFileWriter fsOut = new WavFileWriter(new FileStream("F:\\noaa_test.wav", FileMode.Create), SAMPLE_RATE, 1, 16, BUFFER_SIZE);
            byte[] fsBuffer = new byte[BUFFER_SIZE * sizeof(float)];
            GCHandle fsHandle = GCHandle.Alloc(fsBuffer, GCHandleType.Pinned);
            float* fsPtr = (float*)fsHandle.AddrOfPinnedObject();

            //Open demod
            AptDemodulator apt = new AptDemodulator(SAMPLE_RATE, BUFFER_SIZE);
            apt.OnFrame += Apt_OnFrame;

            //Read
            while (true)
            {
                //Read
                int count = fsIn.Read(fsBuffer, 0, fsBuffer.Length) / sizeof(float);
                if (count == 0)
                    break;

                //Process
                apt.ProcessFM(fsPtr, count);
            }

            fsOut.FinalizeFile();
            img.SaveAsPng("F:\\test_noaa.png");
        }

        private static void Apt_OnFrame(float* ptr, int width)
        {
            for(int i = 0; i<width; i++)
            {
                byte p = (byte)(ptr[i] * 255);
                img[pixel % img.Width, pixel / img.Width] = new Rgba32(p, p, p);
                pixel++;
            }
        }

        static Image<Rgba32> img = new Image<Rgba32>(2080, 1000);
        static int pixel = 0;
    }
}
