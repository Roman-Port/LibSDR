using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework.Components.FFT
{
    public unsafe class FFTScaler : FFTInterface
    {
        public FFTScaler(FFTInterface fft, int width)
        {
            this.fft = fft;
            this.Width = width;
            CreateBuffers();
            fft.OnBlockProcessed += Fft_OnBlockProcessed;
        }

        private void Fft_OnBlockProcessed(float* frame, int fftBins)
        {
            Utils.Memcpy(spectrumBufferPtr, frame, fftBins * sizeof(float));
        }

        private void CreateBuffers()
        {
            spectrumBuffer = UnsafeBuffer.Create(fft.FftBins, sizeof(float));
            spectrumBufferPtr = (float*)spectrumBuffer;
        }

        public int Width { get; set; }
        private UnsafeBuffer spectrumBuffer;
        private float* spectrumBufferPtr;

        private FFTInterface fft;

        public override int FftBins { get => Width; set => Width = value; }

        public override event FFTProcessorOnBlockEventArgs OnBlockProcessed;

        public override unsafe void GetFFTSnapshot(float* ptr)
        {
            ScaleFFTSnapshot(fft.FftBins, spectrumBufferPtr, ptr, Width);
        }

        private const float OFFSET = 10000;

        public static unsafe void ScaleFFTSnapshot(int fftBins, float* snapshot, float* outputPtr, int width)
        {
            if (width < fftBins)
            {
                //More bins than pixels
                float factor = (float)width / (float)fftBins;
                float colTotal = 0;
                int colCount = 0;
                int colIndex = 0;
                for (int i = 0; i < fftBins; i++)
                {
                    //Get pixel index
                    int pixel = (int)(factor * i);

                    //Check if changed
                    if (pixel != colIndex)
                    {
                        //Write and reset
                        outputPtr[colIndex] = colTotal - OFFSET;
                        colTotal = 0;
                        colCount = 0;
                        colIndex = pixel;
                    }

                    //Add
                    colTotal = Math.Max(snapshot[i] + OFFSET, colTotal);
                    colCount++;
                }

                //Set last
                if (width >= 2)
                {
                    outputPtr[width - 1] = outputPtr[width - 2];
                }
            }
            else
            {
                //More pixels than bins
                float factor = (float)fftBins / (float)width;
                for (int i = 0; i < width; i++)
                {
                    outputPtr[i] = snapshot[(int)(i * factor)];
                }
            }
        }
    }
}
