using RomanPort.LibSDR.Components.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.FFT
{
    public unsafe class FFTInstance : IDisposable
    {
        public FFTInstance(int fftSize, WindowType window = WindowType.BlackmanHarris7)
        {
            //Set window
            this.window = window;

            //Use setters to set now, so we set everything up
            FftSize = fftSize;
        }

        //Misc
        private int fftSize;
        private WindowType window;
        private int fftOffset = -40; //not sure what this does

        //Buffer for holding the samples we're processing
        private UnsafeBuffer fftProcessingBuffer;
        private Complex* fftProcessingBufferPtr;

        //Buffer for holding the latest power
        private UnsafeBuffer fftPowerBuffer;
        private float* fftPowerBufferPtr;

        //Buffer for holding the FFT window
        private UnsafeBuffer fftWindowBuffer;
        private float* fftWindowBufferPtr;

        public WindowType Window
        {
            get => window;
            set
            {
                window = value;
                WindowUtil.MakeWindow(window, fftSize, fftWindowBufferPtr);
            }
        }

        public int FftSize
        {
            get => fftSize;
            set
            {
                //Validate
                if (!CheckFFTSize(value))
                    throw new Exception("FFT size is invalid. Must be a multiple of 2 (512, 1024, 2048, etc).");

                //Set
                fftSize = value;

                //Destory buffers
                DestroyBuffers();

                //Create buffers
                fftProcessingBuffer = UnsafeBuffer.Create(fftSize, out fftProcessingBufferPtr);
                fftPowerBuffer = UnsafeBuffer.Create(fftSize, out fftPowerBufferPtr);
                fftWindowBuffer = UnsafeBuffer.Create(fftSize, out fftWindowBufferPtr);

                //Set the window to recreate it
                Window = window;
            }
        }

        private static bool CheckFFTSize(int fftSize)
        {
            for(long i = 2; i <= fftSize; i *= 2)
            {
                if (fftSize == i)
                    return true;
            }
            return false;
        }

        private void DestroyBuffers()
        {
            fftProcessingBuffer?.Dispose();
            fftPowerBuffer?.Dispose();
            fftWindowBuffer?.Dispose();
        }

        public float* ProcessSampleBlock(float* ptr)
        {
            //Copy
            for (int i = 0; i < fftSize; i++)
                fftProcessingBufferPtr[i] = new Complex(ptr[i], 0);

            //Process
            return ProcessFft();
        }

        public float* ProcessSampleBlock(Complex* ptr)
        {
            //Copy
            Utils.Memcpy(fftProcessingBufferPtr, ptr, fftSize * sizeof(Complex));

            //Process
            return ProcessFft();
        }

        private float* ProcessFft()
        {
            //Calculate FFT
            var fftGain = 10.0f * MathF.Log10(fftSize / 2);
            var compensation = 24.0f - fftGain + fftOffset;
            FourierUtil.ApplyFFTWindow(fftProcessingBufferPtr, fftWindowBufferPtr, fftSize);
            FourierUtil.ForwardTransform(fftProcessingBufferPtr, fftSize);
            FourierUtil.SpectrumPower(fftProcessingBufferPtr, fftPowerBufferPtr, fftSize, compensation);

            return fftPowerBufferPtr;
        }

        public void Dispose()
        {
            DestroyBuffers();
        }
    }
}
