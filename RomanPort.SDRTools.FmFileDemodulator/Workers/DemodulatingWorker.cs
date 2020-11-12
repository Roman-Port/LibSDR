using RomanPort.LibSDR.Demodulators;
using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Multithreading;
using RomanPort.LibSDR.Framework.Radio;
using RomanPort.LibSDR.Framework.Resamplers.Decimators;
using RomanPort.LibSDR.Framework.Util;
using RomanPort.LibSDR.Sources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanPort.SDRTools.FmFileDemodulator.Workers
{
    public unsafe class DemodulatingWorker : LoadingForm
    {
        public DemodulatingWorker(WavStreamSource source, SdrAppBuffers buffers, IRadioSampleReceiver output, float inputAudioRate, int outputAudioRate, float bandwidth) : base("Demodulating audio...")
        {
            this.buffers = buffers;
            this.source = source;
            this.output = output;
            this.inputAudioRate = inputAudioRate;
            this.outputAudioRate = outputAudioRate;
            this.bandwidth = bandwidth;

            //Make multithread worker
            multithreader = new MultithreadWorker();

            //Create decimator
            int decimationFactor = SdrFloatDecimator.CalculateDecimationRate(inputAudioRate, bandwidth, out demodulationSampleRate);
            decimator = new SdrComplexDecimator(decimationFactor);

            //Create demodulator
            decimationFactor = SdrFloatDecimator.CalculateDecimationRate(demodulationSampleRate, 48000, out float audioDecimatedRate);
            demodulator = new WbFmDemodulator(decimationFactor);
            demodulator.OnAttached(buffers.size);
            demodulatorOutputRate = demodulator.OnInputSampleRateChanged(demodulationSampleRate);

            //Create filter
            var coefficients = FilterBuilder.MakeBandPassKernel(inputAudioRate, 250, 0, (int)(bandwidth / 2), WindowType.BlackmanHarris4);
            filter = new IQFirFilter(coefficients, multithreader);

            //Create resamplers
            resamplerA = new FloatArbResampler(audioDecimatedRate, outputAudioRate, 1, 0);
            resamplerB = new FloatArbResampler(audioDecimatedRate, outputAudioRate, 1, 0);
        }

        private MultithreadWorker multithreader;
        private SdrAppBuffers buffers;
        private WavStreamSource source;
        private IRadioSampleReceiver output;
        private float inputAudioRate;
        private int outputAudioRate;
        private float bandwidth;
        private float demodulationSampleRate;

        private IDemodulator demodulator;
        private SdrComplexDecimator decimator;
        private IQFirFilter filter;
        private FloatArbResampler resamplerA;
        private FloatArbResampler resamplerB;
        private float demodulatorOutputRate;
        private long totalSamplesRead;

        //private RomanPort.LibSDR.Extras.WavEncoder testC = new RomanPort.LibSDR.Extras.WavEncoder(new System.IO.FileStream("E:\\testC.wav", System.IO.FileMode.Create), 250000, 2, 16);

        public override object WorkThread()
        {
            //Run
            int read = source.Read(buffers.iqBufferPtr, buffers.size);
            while(read != 0)
            {
                //Filter
                filter.Process(buffers.iqBufferPtr, read);

                //Decimate
                int decimatedRead = decimator.Process(buffers.iqBufferPtr, read, buffers.iqBufferPtr, buffers.size);

                //Demodulate
                int audioRead = demodulator.DemodulateStereo(buffers.iqBufferPtr, buffers.audioBufferAPtr, buffers.audioBufferBPtr, decimatedRead);

                //Resample A -> C (left)
                int resampledRead = resamplerA.Process(buffers.audioBufferAPtr, audioRead, buffers.audioBufferCPtr, buffers.size, read != buffers.size);

                //Resample B -> A (right)
                resamplerB.Process(buffers.audioBufferBPtr, audioRead, buffers.audioBufferAPtr, buffers.size, read != buffers.size);

                //Write
                output.OnSamples(buffers.audioBufferCPtr, buffers.audioBufferAPtr, resampledRead);

                //Update status
                totalSamplesRead += read;
                StatusUpdate((double)totalSamplesRead / source.totalSamples);

                //Read next
                read = source.Read(buffers.iqBufferPtr, buffers.size);
            }

            //Do cleanup
            filter.Dispose();
            return totalSamplesRead;
        }
    }
}
