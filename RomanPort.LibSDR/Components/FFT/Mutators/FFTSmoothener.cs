using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.FFT.Mutators
{
    public unsafe class FFTSmoothener : IFftMutatorSource
    {
        public FFTSmoothener(IFftMutatorSource source, float attack = 0.4f, float decay = 0.3f)
        {
            this.source = source;
            this.attack = attack;
            this.decay = decay;
        }

        private IFftMutatorSource source;
        private float attack;
        private float decay;

        private UnsafeBuffer buffer;
        private float* bufferPtr;

        public float Attack { get => attack; set => attack = value; }
        public float Decay { get => decay; set => decay = value; }
        
        public float* ProcessFFT(out int fftBins)
        {
            //Get pointer to parent
            float* src = source.ProcessFFT(out fftBins);

            //Make buffer if needed
            if (buffer == null || buffer.Length != fftBins)
            {
                //Remove old buffer
                buffer?.Dispose();

                //Make new buffer
                buffer = UnsafeBuffer.Create(fftBins, out bufferPtr);

                //Copy samples in
                Utils.Memcpy(bufferPtr, src, fftBins * sizeof(float));
            }

            //Perform smoothening
            Smoothen(bufferPtr, src, fftBins);

            return bufferPtr;
        }

        private void Smoothen(float* buffer, float* newBuffer, int fftBins)
        {
            for (var i = 0; i < fftBins; i++)
            {
                var ratio = buffer[i] < newBuffer[i] ? attack : decay;
                buffer[i] = buffer[i] * (1 - ratio) + newBuffer[i] * ratio;
            }
        }
    }
}
