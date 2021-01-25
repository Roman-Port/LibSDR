using RomanPort.LibSDR.Benchmarks.Benchmarks;
using System;
using System.IO;

namespace RomanPort.LibSDR.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            //Load file for benchmarking
            BenchmarkData file = new BenchmarkData(@"C:\Users\Roman\Desktop\Unpacked IQ\93700000Hz 93x no excuses toth.wav", 10, 30);
            //BenchmarkData file = new BenchmarkData(@"/home/pi/benchmark/benchmark.wav", 10, 30);
            file.Load();

            //Create benchmarks
            BenchmarkBase[] benchmarks = new BenchmarkBase[]
            {
                new WbFmDemodBenchmark(8192, 250000, 48000),
                new WbFmDemodBenchmark(16384, 250000, 48000),
                new WbFmDemodBenchmark(32768, 250000, 48000),
            };

            //Process
            double[] times = new double[benchmarks.Length];
            for (int i = 0; i < benchmarks.Length; i++)
                times[i] = benchmarks[i].RunBenchmark(file);

            //Serialize
            string[] logLines = new string[times.Length];
            for (int i = 0; i < benchmarks.Length; i++)
                logLines[i] = $"\"{benchmarks[i].BenchmarkName}\",\"{benchmarks[i].BenchmarkArgs}\",{times[i]}";

            //Prompt for name
            Console.WriteLine("Benchmarks completed. Choose a filename for this file.");
            string name = Console.ReadLine();

            //Save
            File.WriteAllLines(name, logLines);
        }
    }
}
