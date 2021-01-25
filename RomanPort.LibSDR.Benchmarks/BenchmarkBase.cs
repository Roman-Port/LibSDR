using RomanPort.LibSDR.Components.IO.WAV;
using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace RomanPort.LibSDR.Benchmarks
{
    public abstract unsafe class BenchmarkBase
    {
        public BenchmarkBase(int bufferSize)
        {
            this.bufferSize = bufferSize;
        }

        public readonly int bufferSize;

        public const int PASS_COUNT = 3;

        public abstract string BenchmarkName { get; }
        public abstract string BenchmarkArgs { get; }

        public double RunBenchmark(BenchmarkData data)
        {
            //Run passes
            double value = 0;
            for (int i = 0; i < PASS_COUNT; i++)
                value += RunBenchmarkPass(data, i);
            return value / PASS_COUNT;
        }

        public double RunBenchmarkPass(BenchmarkData data, int pass)
        {
            //Log basic info
            string logLine = $"Running [PASS {pass}] \"{BenchmarkName}\" ({BenchmarkArgs})...";
            Console.Write(logLine + "0%...");

            //Prepare
            PrepareBenchmark((int)data.sampleRate, bufferSize);
            long logUpdateSamples = 0;
            int logUpdateIndex = 0;
            Stopwatch timer = new Stopwatch();
            timer.Start();

            //Loop
            for(long sample = 0; sample < data.sampleCount; sample += bufferSize)
            {
                //Determine readable
                int readable = (int)Math.Min(bufferSize, data.sampleCount - sample);

                //Process
                ProcessBlock(data.ptr + sample, readable);

                //Log if needed
                logUpdateSamples += readable;
                if(logUpdateSamples > data.samplesPerLogUpdate)
                {
                    logUpdateIndex++;
                    logUpdateSamples -= data.samplesPerLogUpdate;
                    Console.Write("\r" + logLine + logUpdateIndex + "0%...");
                }
            }

            //Get time and clean up
            TimeSpan time = timer.Elapsed;
            EndBenchmark();

            //Log
            Console.WriteLine("\r" + logLine + $"DONE ({time.TotalSeconds} seconds)");

            return time.TotalSeconds;
        }

        protected abstract void PrepareBenchmark(int sampleRate, int bufferSize);
        protected abstract void EndBenchmark();
        protected abstract void ProcessBlock(Complex* ptr, int count);
    }
}
