using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.FFT
{
    public unsafe class FFTCropper : FFTInterface
    {
        public float CroppingRatio
        {
            get => croppingRatio;
            set {
                if (value > 1)
                    throw new Exception("CroppingRatio must not be greater than 1.");
                if (value <= 0)
                    throw new Exception("CroppingRatio must not be less or equal to than 0.");
                croppingRatio = value;
            }
        }
        public bool CropToCenter { get; set; }
        
        public override int FftBins { get => parent.FftBins; set => parent.FftBins = value; }

        public override event FFTProcessorOnBlockEventArgs OnBlockProcessed;

        private float croppingRatio = 1;
        private FFTInterface parent;
        private UnsafeBuffer buffer;
        private float* bufferPtr;
        private int bufferBins;

        public FFTCropper(FFTInterface parent)
        {
            this.parent = parent;
            parent.OnBlockProcessed += Parent_OnBlockProcessed;
            CroppingRatio = 1;
        }

        public void CropToFreq(float sampleRate, float desiredBandwidth)
        {
            CroppingRatio = desiredBandwidth / sampleRate;
        }

        private void Parent_OnBlockProcessed(float* frame, int fftBins)
        {
            //Ensure buffer matches
            if(fftBins != bufferBins)
            {
                //Remove buffer if it exists
                if (buffer != null)
                    buffer.Dispose();

                //Create
                buffer = UnsafeBuffer.Create(fftBins, out bufferPtr);
                bufferBins = fftBins;
            }

            //Apply transitions
            if(CropToCenter)
            {
                //Center
                throw new NotImplementedException();
            } else
            {
                //Left
                for (int i = 0; i < fftBins; i++)
                    bufferPtr[i] = frame[(int)(i * CroppingRatio)];
            }

            //Send event
            OnBlockProcessed?.Invoke(bufferPtr, fftBins);
        }

        public override unsafe void GetFFTSnapshot(float* ptr)
        {
            Utils.Memcpy(ptr, bufferPtr, sizeof(float) * bufferBins);
        }
    }
}
