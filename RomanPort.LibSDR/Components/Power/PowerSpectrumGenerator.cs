using RomanPort.LibSDR.Components.FFTX;
using RomanPort.LibSDR.Components.FFTX.Kiss;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Power
{
    public unsafe class PowerSpectrumGenerator
    {
        public PowerSpectrumGenerator(int fftSize)
        {
            //Configure
            this.fftSize = fftSize;
            bufferSize = fftSize * 4;

            //Create buffers
            loopBuffer = UnsafeBuffer.Create(bufferSize, out loopBufferPtr);
            workingBuffer = UnsafeBuffer.Create(fftSize, out workingBufferPtr);
            powerBuffer = UnsafeBuffer.Create(fftSize, out powerBufferPtr);
            powerPersistBuffer = UnsafeBuffer.Create(fftSize, out powerPersistBufferPtr);

            //Create window and FFT
            window = new FFTWindow(fftSize, Filters.WindowType.BlackmanHarris7);
            fft = new KissFFTComplex(fftSize, false);
        }

        private int bufferSize;
        private int fftSize;
        private int bufferWritePos;
        private int persistPowerSize;
        private float attack = 0.3f;
        private float decay = 0.4f;

        private FFTWindow window;
        private KissFFTComplex fft;

        private UnsafeBuffer loopBuffer;
        private Complex* loopBufferPtr;
        private UnsafeBuffer workingBuffer;
        private Complex* workingBufferPtr;
        private UnsafeBuffer powerBuffer;
        private Complex* powerBufferPtr;
        private UnsafeBuffer powerPersistBuffer;
        private float* powerPersistBufferPtr;

        public float Attack
        {
            get => attack;
            set => attack = value;
        }
        public float Decay
        {
            get => decay;
            set => decay = value;
        }

        public void AddSamples(Complex* ptr, int count)
        {
            for (int i = 0; i < count && bufferWritePos < bufferSize; i++)
            {
                loopBufferPtr[bufferWritePos++] = ptr[i];
            }
        }

        public void AddSamples(float* ptr, int count)
        {
            for (int i = 0; i < count && bufferWritePos < bufferSize; i++)
            {
                loopBufferPtr[bufferWritePos++] = ptr[i];
            }
        }

        public void ReadPower(float* output, int length)
        {
            //Copy out samples
            Utils.Memcpy(workingBufferPtr, loopBufferPtr, fftSize * sizeof(Complex));
            bufferWritePos = 0;

            //Apply window
            window.Apply(workingBufferPtr);

            //Compute FFT
            fft.Process(workingBufferPtr, powerBufferPtr);

            //Compute power
            FFTUtil.CalculatePower(powerBufferPtr, (float*)powerBufferPtr, fftSize);

            //Fix spectrum
            FFTUtil.OffsetSpectrum((float*)powerBufferPtr, fftSize);

            //Resize
            FFTUtil.ResizePower((float*)powerBufferPtr, output, fftSize, length);

            //Check if the persist buffer is stale
            bool persistStale = persistPowerSize == 0 || length != persistPowerSize;
            if(persistStale)
            {
                persistPowerSize = length;
                powerPersistBuffer?.Dispose();
                powerPersistBuffer = UnsafeBuffer.Create(length, out powerPersistBufferPtr);
            }

            //Smoothen buffer
            if (persistStale)
            {
                //Just copy to the persist buffer, as no data currently exists
                Utils.Memcopy(powerPersistBufferPtr, output, length);
            }
            else
            {
                //Average
                FFTUtil.ApplySmoothening(powerPersistBufferPtr, output, length, attack, decay);

                //Copy to output
                Utils.Memcopy(output, powerPersistBufferPtr, length);
            }
        }
    }
}
