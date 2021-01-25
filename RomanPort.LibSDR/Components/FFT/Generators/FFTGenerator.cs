using RomanPort.LibSDR.Components.Filters;
using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.FFT.Generators
{
    public unsafe class FFTGenerator : IDisposable, IFftMutatorSource
    {
        public FFTGenerator(int fftBins, bool isHalf)
        {
            fftBinsMultiplier = isHalf ? 2 : 1;
            fftBufferBins = fftBins * fftBinsMultiplier;
            Configure();
        }

        public int FftBins
        {
            get => fftBufferBins / fftBinsMultiplier;
            set
            {
                fftBufferBins = value * fftBinsMultiplier;
                Configure();
            }
        }

        private int fftBufferBins;
        private int fftBinsMultiplier;
        private int fftOffset = -40; //not sure what this does
        private bool isAvgClearingNeeded;

        //Buffer for holding the samples we're waiting to process. May be accessed by multiple threads at once
        private UnsafeBuffer fftBuffer;
        private Complex* fftBufferPtr;

        //Buffer for holding samples that are waiting to be written to fftBuffer until we have a full frame
        private UnsafeBuffer fftWaitingBuffer;
        private Complex* fftWaitingBufferPtr;
        private int fftWaitingCount;

        //Buffer for holding the samples we're processing
        private UnsafeBuffer fftProcessingBuffer;
        private Complex* fftProcessingBufferPtr;

        //Buffer for holding the latest power
        private UnsafeBuffer fftPowerBuffer;
        private float* fftPowerBufferPtr;

        //Buffer for holding the FFT window
        private UnsafeBuffer fftWindowBuffer;
        private float* fftWindowBufferPtr;

        private void Configure()
        {
            //Dispose
            Dispose();

            //Create buffers
            fftBuffer = UnsafeBuffer.Create(fftBufferBins, out fftBufferPtr);
            fftWaitingBuffer = UnsafeBuffer.Create(fftBufferBins, out fftWaitingBufferPtr);
            fftProcessingBuffer = UnsafeBuffer.Create(fftBufferBins, out fftProcessingBufferPtr);
            fftPowerBuffer = UnsafeBuffer.Create(fftBufferBins, out fftPowerBufferPtr);
            fftWindowBuffer = UnsafeBuffer.Create(fftBufferBins, out fftWindowBufferPtr);

            //Reset state
            fftWaitingCount = 0;

            //Create new window
            FilterWindowUtil.MakeWindow(WindowType.BlackmanHarris7, fftBufferBins, fftWindowBufferPtr);
        }

        public void Dispose()
        {
            fftBuffer?.Dispose();
            fftWaitingBuffer?.Dispose();
            fftProcessingBuffer?.Dispose();
            fftPowerBuffer?.Dispose();
            fftWindowBuffer?.Dispose();
        }

        public void AddSamples(Complex* ptr, int len)
        {
            while(len > 0)
            {
                //Transfer to temp buffer
                int transferrable = Math.Min(len, fftBufferBins - fftWaitingCount);
                Utils.Memcpy(fftWaitingBufferPtr + fftWaitingCount, ptr, transferrable * sizeof(Complex));
                len -= transferrable;
                ptr += transferrable;
                fftWaitingCount += transferrable;

                //Process block if we can
                if (fftWaitingCount == fftBufferBins)
                    ProcessBlock();
            }
        }

        public void AddSamples(float* ptr, int len)
        {
            while (len > 0)
            {
                //Transfer to temp buffer
                int transferrable = Math.Min(len, fftBufferBins - fftWaitingCount);
                for (int i = 0; i < transferrable; i++)
                    fftWaitingBufferPtr[fftWaitingCount + i] = new Complex(ptr[i], 0);
                len -= transferrable;
                ptr += transferrable;
                fftWaitingCount += transferrable;

                //Process block if we can
                if (fftWaitingCount == fftBufferBins)
                    ProcessBlock();
            }
        }

        private bool blockSkipAveraging = true;

        private void ProcessBlock()
        {
            //Samples in fftWaitingBufferPtr
            if(blockSkipAveraging)
            {
                //Just copy
                Utils.Memcpy(fftBufferPtr, fftWaitingBufferPtr, fftBufferBins * sizeof(Complex));
                blockSkipAveraging = false;
            } else
            {
                //Average all
                for (int i = 0; i < fftBufferBins; i++)
                    fftBufferPtr[i] = (fftBufferPtr[i] + fftWaitingBufferPtr[i]) / 2;
            }

            //Reset
            fftWaitingCount = 0;
        }

        public float* ProcessFFT(out int fftBins)
        {
            //Set
            fftBins = fftBufferBins / fftBinsMultiplier;
            isAvgClearingNeeded = true;

            //Copy from buffer
            Utils.Memcpy(fftProcessingBufferPtr, fftBufferPtr, fftBufferBins * sizeof(Complex));
            
            //Calculate FFT
            var fftGain = 10.0f * MathF.Log10(fftBufferBins / 2);
            var compensation = 24.0f - fftGain + fftOffset;
            Fourier.ApplyFFTWindow(fftProcessingBufferPtr, fftWindowBufferPtr, fftBufferBins);
            Fourier.ForwardTransform(fftProcessingBufferPtr, fftBufferBins);
            Fourier.SpectrumPower(fftProcessingBufferPtr, fftPowerBufferPtr, fftBufferBins, compensation);

            //Return pointer to the last part of the buffer
            return fftPowerBufferPtr + (fftBufferBins - FftBins);
        }
    }
}
