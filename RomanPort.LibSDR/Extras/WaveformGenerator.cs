using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Util;
using RomanPort.LibSDR.Sources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RomanPort.LibSDR.Extras
{
    /// <summary>
    /// Generates a waveform from demodulated audio. Useful for previewing
    /// </summary>
    public unsafe class WaveformGenerator : IDisposable
    {
        private WavStreamSource source;
        private IDemodulator demodulator;
        private int maxBufferSize;
        private float sampleRate;
        private float demodBandwidth;
        private float audioFilterBandwidth;
        private FirFilter demodFilter;

        //Events
        public event WaveformChunkProgressEventArgs OnWaveformChunkProgress;

        //Buffers
        private UnsafeBuffer bufferA;
        private Complex* bufferAPtr;
        private UnsafeBuffer bufferB;
        private float* bufferBPtr;

        public WaveformGenerator(string filePath, IDemodulator demodulator, float demodBandwidth, float audioFilterBandwidth, int maxBufferSize)
        {
            //Set
            this.demodulator = demodulator;
            this.maxBufferSize = maxBufferSize;
            this.demodBandwidth = demodBandwidth;
            this.audioFilterBandwidth = audioFilterBandwidth;

            //Open source
            source = new WavStreamSource(new FileStream(filePath, FileMode.Open, FileAccess.Read), false, 0);
            sampleRate = source.Open(maxBufferSize);

            //Open demodulator
            demodulator.OnAttached(maxBufferSize);
            demodulator.OnInputSampleRateChanged(sampleRate);

            //Open buffers
            bufferA = UnsafeBuffer.Create(maxBufferSize, sizeof(Complex));
            bufferAPtr = (Complex*)bufferA;
            bufferB = UnsafeBuffer.Create(maxBufferSize, sizeof(Complex));
            bufferBPtr = (float*)bufferB;

            //Create demod filter
            var coefficients = FilterBuilder.MakeBandPassKernel(sampleRate, 250, 0, (int)(audioFilterBandwidth / 2), WindowType.BlackmanHarris4);
            demodFilter = new FirFilter(coefficients);
        }

        public void RequestFull(float* outputPtr, int outputCount, object eventContext = null)
        {
            RequestChunk(0, source.GetLengthSeconds(), outputPtr, outputCount, eventContext);
        }

        public void RequestChunk(float start, float end, float* outputPtr, int outputCount, object eventContext = null)
        {
            //Calculate range of samples to get
            long requestedSampleCount = (long)((end - start) * sampleRate);
            long samplesPerIndex = requestedSampleCount / outputCount;

            //Determine the next index
            int index = 0;
            long nextIndexSample = (index + 1) * samplesPerIndex;

            //Seek to and begin loop
            long totalRead = 0;
            int read = source.ReadSeek(bufferAPtr, (int)Math.Min(requestedSampleCount - totalRead, maxBufferSize), start);
            while(read != 0)
            {
                //Demodulate this
                int demodRead = demodulator.DemodulateMono(bufferAPtr, bufferBPtr, read);

                //Filter
                demodFilter.Process(bufferBPtr, demodRead);

                //Loop
                for(int i = 0; i<demodRead; i++)
                {
                    //Determine if we should update the index counter
                    if(totalRead + i > nextIndexSample)
                    {
                        index++;
                        nextIndexSample = (index + 1) * samplesPerIndex;
                        OnWaveformChunkProgress?.Invoke(index, this, eventContext);
                        if (index == outputCount)
                            return;
                    }

                    //Calculate
                    outputPtr[index] = Math.Max(outputPtr[index], Math.Abs(bufferBPtr[i]));
                }

                //Update
                totalRead += read;

                //Skip
                long skip = nextIndexSample - totalRead;
                source.SkipSamples(skip);
                totalRead += skip;

                //Read next and update
                read = source.Read(bufferAPtr, (int)Math.Min(requestedSampleCount - totalRead, maxBufferSize));
            }
        }

        public void Dispose()
        {
            bufferA.Dispose();
            bufferB.Dispose();
            source.Close();
        }
    }

    public delegate void WaveformChunkProgressEventArgs(int processedOutput, WaveformGenerator sender, object context);
}
