using RomanPort.LibSDR.Components.Decimators;
using RomanPort.LibSDR.Demodulators.Analog.Broadcast;
using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Benchmarks.Benchmarks
{
    public unsafe class WbFmDemodBenchmark : BenchmarkBase
    {
        public WbFmDemodBenchmark(int bufferSize, float bandwidth, int outputRateTarget) : base(bufferSize)
        {
            this.bandwidth = bandwidth;
            this.outputRateTarget = outputRateTarget;
        }

        private float bandwidth;
        private int outputRateTarget;

        private ComplexDecimator decimator;
        private WbFmDemodulator demod;
        private UnsafeBuffer iqBuffer;
        private Complex* iqBufferPtr;
        private UnsafeBuffer audioABuffer;
        private float* audioABufferPtr;
        private UnsafeBuffer audioBBuffer;
        private float* audioBBufferPtr;

        public override string BenchmarkName => "WbFm Demod";

        public override string BenchmarkArgs => $"buffer={bufferSize}, bw={bandwidth}, output={outputRateTarget}";

        protected override void PrepareBenchmark(int sampleRate, int bufferSize)
        {
            //Create buffers
            iqBuffer = UnsafeBuffer.Create(bufferSize, out iqBufferPtr);
            audioABuffer = UnsafeBuffer.Create(bufferSize, out audioABufferPtr);
            audioBBuffer = UnsafeBuffer.Create(bufferSize, out audioBBufferPtr);

            //Create IQ decimator
            decimator = ComplexDecimator.CalculateDecimator(sampleRate, bandwidth, 15, bandwidth * 0.05f, out float decimatedIqRate);

            //Create demodulator
            demod = new WbFmDemodulator();
            demod.Configure(bufferSize, sampleRate, DecimationUtil.CalculateDecimationRate(decimatedIqRate, outputRateTarget, out float actualOutputRate));
            demod.UseRds();
        }

        protected override unsafe void ProcessBlock(Complex* ptr, int count)
        {
            //Decimate
            count = decimator.Process(ptr, iqBufferPtr, count);

            //Demodulate
            count = demod.DemodulateStereo(iqBufferPtr, audioABufferPtr, audioBBufferPtr, count);
        }

        protected override void EndBenchmark()
        {
            //Clean up buffers
            iqBuffer.Dispose();
            audioABuffer.Dispose();
            audioBBuffer.Dispose();
        }
    }
}
