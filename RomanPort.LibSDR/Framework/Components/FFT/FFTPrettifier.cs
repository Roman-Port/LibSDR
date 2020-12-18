using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework.Components.FFT
{
    public unsafe class FFTPrettifier : FFTInterface
    {
        public FFTPrettifier(FFTInterface processor, float attack = 0.4f, float decay = 0.3f)
        {
            this.processor = processor;
            this.attack = attack;
            this.decay = decay;
            processor.OnBlockProcessed += Processor_OnBlockProcessed;
            ChangeFFTBins();
        }

        private FFTInterface processor;
        private bool hasFirstFrame;
        private UnsafeBuffer spectrumBufferA;
        private float* spectrumBufferAPtr;
        private UnsafeBuffer spectrumBufferB;
        private float* spectrumBufferBPtr;
        private int addedFrames;

        public float attack = 0.4f; //Between 0-1
        public float decay = 0.3f; //Between 0-1

        public override int FftBins { get => processor.FftBins; set => processor.FftBins = value; }

        public override event FFTProcessorOnBlockEventArgs OnBlockProcessed;

        private void Processor_OnBlockProcessed(float* frame, int fftBins)
        {
            AddFFTSamples(frame);
        }

        private void ChangeFFTBins()
        {
            //Dispose of old buffers
            spectrumBufferA?.Dispose();

            //Set state
            hasFirstFrame = false;

            //Create new buffer
            spectrumBufferA = UnsafeBuffer.Create(FftBins, sizeof(float));
            spectrumBufferAPtr = (float*)spectrumBufferA;
            spectrumBufferB = UnsafeBuffer.Create(FftBins, sizeof(float));
            spectrumBufferBPtr = (float*)spectrumBufferB;
        }

        private void AddFFTSamples(float* fftSamples)
        {
            Smoothen(spectrumBufferAPtr, fftSamples);
            OnBlockProcessed?.Invoke(spectrumBufferAPtr, FftBins);
        }

        public override unsafe void GetFFTSnapshot(float* dest)
        {
            Utils.Memcpy(dest, spectrumBufferAPtr, sizeof(float) * FftBins);
        }

        private void Smoothen(float* buffer, float* newBuffer)
        {
            for (var i = 0; i < FftBins; i++)
            {
                var ratio = buffer[i] < newBuffer[i] ? attack : decay;
                buffer[i] = buffer[i] * (1 - ratio) + newBuffer[i] * ratio;
            }
        }
    }
}
