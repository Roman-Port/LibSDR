using RomanPort.LibSDR.Components.FFTX.Kiss;
using RomanPort.LibSDR.Components.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.FFTX.PowerSpectrum
{
    public unsafe delegate void PowerPreview_OnFrameEventArgs(float* frame, int width);
    
    public unsafe class PowerPreview
    {
        public PowerPreview(int fftBins, int sampleRate, int frameWidth, int targetFrameRate, bool isHalf = false)
        {
            //Apply
            this.fftBins = fftBins;
            this.isHalf = isHalf;
            TargetFrameRate = targetFrameRate;
            SampleRate = sampleRate;
            FrameWidth = frameWidth;

            //Set defaults
            Attack = 0.4f;
            Decay = 0.3f;
            
            //Get FFT window and an FFT
            window = new FFTWindow(fftBins, WindowType.Youssef);
            fft = new KissFFTComplex(fftBins, false);

            //Open constant buffers
            fftCirularBuffer = UnsafeBuffer.Create(fftBins, out fftCirularPtr);
            fftIncomingBuffer = UnsafeBuffer.Create(fftBins, out fftIncomingPtr);
            fftProcessingBuffer = UnsafeBuffer.Create(fftBins, out fftProcessingPtr);
            powerProcessingBuffer = UnsafeBuffer.Create(fftBins, out powerProcessingPtr);
        }

        public event PowerPreview_OnFrameEventArgs OnFrame;

        public int SampleRate
        {
            get => sampleRate;
            set
            {
                sampleRate = value;
                frameSamplesRead = 0;
                RecalculateSamplesPerFrame();
            }
        }
        public int FrameWidth
        {
            get => frameWidth;
            set
            {
                if (value <= 0)
                    throw new Exception("Value must be > 0!");

                frameWidth = value;
                frameWidthDirty = true;
            }
        }
        public int TargetFrameRate
        {
            get => targetFrameRate;
            set
            {
                if (value <= 0)
                    throw new Exception("Value must be > 0!");
                targetFrameRate = value;
                RecalculateSamplesPerFrame();
            }
        }
        public float Attack
        {
            get => attack;
            set
            {
                if (value > 1)
                    attack = 1;
                else if (value < 0)
                    attack = 0;
                else
                    attack = value;
            }
        }
        public float Decay
        {
            get => decay;
            set
            {
                if (value > 1)
                    decay = 1;
                else if (value < 0)
                    decay = 0;
                else
                    decay = value;
            }
        }

        private FFTWindow window;
        private KissFFTComplex fft;
        private int fftBins;
        private bool isHalf;

        private float samplesPerFrame;
        private int incomingBufferIndex;
        private float frameSamplesRead;
        private int frameBufferWidth;
        private bool frameWidthDirty = true;
        private bool firstFrame = true; //If set to true, the frame isn't averaged. The data is just copied

        /* Constant arrays */
        private UnsafeBuffer fftCirularBuffer; //Size fftBins. Holds the samples as they're incoming
        private Complex* fftCirularPtr;
        private UnsafeBuffer fftIncomingBuffer; //Size fftBins. Holds the samples to be processed
        private Complex* fftIncomingPtr;
        private UnsafeBuffer fftProcessingBuffer; //Size fftBins. Holds the FFT computed samples
        private Complex* fftProcessingPtr;
        private UnsafeBuffer powerProcessingBuffer; //Size fftBins. Holds the computed power levels
        private float* powerProcessingPtr;

        /* Dynamic arrays, only should be used/created/destroyed in AddSamples */
        private UnsafeBuffer powerResizedBuffer; //Size frameWidth. Holds the resized computed power levels
        private float* powerResizedPtr;
        private UnsafeBuffer powerFrameBuffer; //Size frameWidth. Holds the persistent, smoothened, resized, power levels
        private float* powerFramePtr;

        private int sampleRate;
        private int frameWidth;
        private int targetFrameRate;
        private float attack;
        private float decay;

        public void AddSamples(Complex* ptr, int count)
        {
            //If the frame buffer width is changed (dirty), make a new file
            CheckIfDirty();

            //If the sample rate is zero, that means we aren't ready yet. Just wait
            if (sampleRate == 0)
                return;

            //Loop while we have samples
            while (count > 0)
            {
                //Write to buffer
                fftCirularPtr[incomingBufferIndex] = *ptr++;
                incomingBufferIndex = (incomingBufferIndex + 1) % fftBins; 
                count--;

                //Check if we have a full frame
                if (frameSamplesRead++ >= samplesPerFrame)
                    ProcessFrame();
            }
        }

        public void AddSamples(float* ptr, int count)
        {
            //If the frame buffer width is changed (dirty), make a new file
            CheckIfDirty();

            //If the sample rate is zero, that means we aren't ready yet. Just wait
            if (sampleRate == 0)
                return;

            //Loop while we have samples
            while (count > 0)
            {
                //Write to buffer
                fftCirularPtr[incomingBufferIndex] = new Complex(*ptr, 0);
                incomingBufferIndex = (incomingBufferIndex + 1) % fftBins;
                count--;

                //Check if we have a full frame
                if (frameSamplesRead++ >= samplesPerFrame)
                    ProcessFrame();
            }
        }

        private void CheckIfDirty()
        {
            if (frameWidthDirty)
            {
                powerResizedBuffer?.Dispose();
                powerFrameBuffer?.Dispose();
                frameBufferWidth = frameWidth;
                frameWidthDirty = false;
                firstFrame = true;
                powerResizedBuffer = UnsafeBuffer.Create(frameBufferWidth, out powerResizedPtr);
                powerFrameBuffer = UnsafeBuffer.Create(frameBufferWidth, out powerFramePtr);
            }
        }

        private void ProcessFrame()
        {
            //Copy out samples
            for (int i = 0; i < fftBins; i++)
                fftIncomingPtr[i] = fftCirularPtr[(incomingBufferIndex + i) % fftBins];

            //Apply window
            window.Apply(fftIncomingPtr);

            //Compute FFT
            fft.Process(fftIncomingPtr, fftProcessingPtr);

            //Compute power
            FFTUtil.CalculatePower(fftProcessingPtr, powerProcessingPtr, fftBins);

            //Fix spectrum
            FFTUtil.OffsetSpectrum(powerProcessingPtr, fftBins);

            //Resize power
            if(isHalf)
                FFTUtil.ResizePower(powerProcessingPtr + (fftBins / 2), powerResizedPtr, fftBins / 2, frameBufferWidth);
            else
                FFTUtil.ResizePower(powerProcessingPtr, powerResizedPtr, fftBins, frameBufferWidth);

            //Smoothen
            if (firstFrame)
                Utils.Memcpy(powerFramePtr, powerResizedPtr, frameBufferWidth * sizeof(float));
            else
                FFTUtil.ApplySmoothening(powerFramePtr, powerResizedPtr, frameBufferWidth, attack, decay);

            //Send events
            OnFrame?.Invoke(powerFramePtr, frameBufferWidth);

            //Reset state
            firstFrame = false;
            frameSamplesRead -= samplesPerFrame;
        }

        private void RecalculateSamplesPerFrame()
        {
            samplesPerFrame = (float)sampleRate / targetFrameRate;
        }
    }
}
