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
            if(Width < fft.FftBins)
            {
                //More bins than pixels
                float factor = (float)Width / (float)fft.FftBins;
                float colTotal = 0;
                int colCount = 0;
                int colIndex = 0;
                for(int i = 0; i<fft.FftBins; i++)
                {
                    //Get pixel index
                    int pixel = (int)(factor * i);

                    //Check if changed
                    if(pixel != colIndex)
                    {
                        //Write and reset
                        ptr[colIndex] = colTotal / colCount;
                        colTotal = 0;
                        colCount = 0;
                        colIndex = pixel;
                    }

                    //Add
                    colTotal += spectrumBufferPtr[i];
                    colCount++;
                }
            } else
            {
                //More pixels than bins
                float factor = (float)fft.FftBins / (float)Width;
                for (int i = 0; i<Width; i++)
                {
                    ptr[i] = spectrumBufferPtr[(int)(i * factor)];
                }
            }
        }
    }
}
