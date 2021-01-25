using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.FFT.Mutators
{
    public unsafe class FFTInterfaceAdapter : IFftMutatorSource
    {
        public FFTInterfaceAdapter(FFTInterface source)
        {
            this.source = source;
            source.OnBlockProcessed += Source_OnBlockProcessed;

            //Make new buffer so we have a default state
            buffer = UnsafeBuffer.Create(512, out bufferPtr);
            averagedCount = 1;
        }

        private FFTInterface source;
        private UnsafeBuffer buffer;
        private float* bufferPtr;
        private int averagedCount;

        private void Source_OnBlockProcessed(float* frame, int fftBins)
        {
            if (buffer == null || buffer.Length != fftBins)
            {
                //Remove old buffer
                buffer?.Dispose();

                //Make new buffer
                buffer = UnsafeBuffer.Create(fftBins, out bufferPtr);

                //Copy samples in
                Utils.Memcpy(bufferPtr, frame, fftBins * sizeof(float));
                averagedCount = 1;
            } else
            {
                //Add to average
                /*for (int i = 0; i < fftBins; i++)
                    bufferPtr[i] = (bufferPtr[i] + frame[i]) / 2f;*/
            }

            Utils.Memcpy(bufferPtr, frame, fftBins * sizeof(float));
            averagedCount = 1;
        }

        public float* ProcessFFT(out int fftBins)
        {
            //Perform averaging
            /*for (int i = 0; i < averagedCount; i++)
                bufferPtr[i] /= averagedCount;
            averagedCount = 1;*/

            //Output
            fftBins = buffer.Length;
            return bufferPtr;
        }
    }
}
