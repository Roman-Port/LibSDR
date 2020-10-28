using RomanPort.LibSDR;
using RomanPort.LibSDR.Demodulators;
using RomanPort.LibSDR.Extras;
using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Util;
using RomanPort.LibSDR.Receivers;
using RomanPort.LibSDR.Sources;
using RomanPort.LibSDR.Sources.Hardware.RTLSDR;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace RomanPort.LibSDRTests
{
    unsafe class Program
    {
        private static SDRRadio radio;
        private static FileStream outputFile;
        private static WavReceiver output;
        private static StatusDisplay stats;

        static void Main(string[] args)
        {
            //Open radio
            radio = new SDRRadio(new SDRRadioConfig
            {
                bufferSize = 16384,
                outputAudioSampleRate = 48000,
                realtime = false
            });

            //Create stats object. This object just logs to the console
            stats = new StatusDisplay(48000);

            //Open output
            outputFile = new FileStream("E:\\test_demod_audio.wav", FileMode.Create);
            output = new WavReceiver(outputFile, 16);

            //Open WAV file as a source
            FileStream sourceFile = new FileStream("E:\\KQRS-FM - Led Zeppelin - Whole Lotta Love.wav", FileMode.Open);
            WavStreamSource source = new WavStreamSource(sourceFile, false, 0);

            //Create the demodulator
            WbFmDemodulator demodulator = new WbFmDemodulator();

            //Open radio
            radio.OpenRadio(source);
            radio.AddDemodReceiver(output);
            radio.SetDemodulator(demodulator, 250000);

            //Add events
            radio.OnAudioSamplesAvailable += Radio_OnAudioSamplesAvailable;

            Console.ReadLine();
        }

        private static void Radio_OnAudioSamplesAvailable(float* left, float* right, int samplesRead)
        {
            //Log
            stats.OnSamples(samplesRead);
        }
    }
}
