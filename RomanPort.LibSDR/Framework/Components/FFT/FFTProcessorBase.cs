using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework.Components.FFT
{
    public unsafe delegate void FFTProcessorOnBlockEventArgs(float* frame, int fftBins);
    
    public abstract unsafe class FFTProcessorBase<T> : FFTInterface, IDisposable where T : unmanaged
    {
        public FFTProcessorBase(int fftBins, int bufferSizeMultiplier)
        {
            FftBins = fftBins;
            this.bufferSizeMultiplier = bufferSizeMultiplier;
            CreateBuffers();
            BuildFFTWindow();
        }

        private void DestroyBuffers()
        {
            fftBuffer?.Dispose();
            fftWindowBuffer?.Dispose();
            fftSpectrumBuffer?.Dispose();
            fftSpectrumWaitingBuffer?.Dispose();
            fftSpectrumFinalBuffer?.Dispose();
        }

        private void CreateBuffers()
        {
            fftBuffer = UnsafeBuffer.Create(FftBufferSize, sizeof(Complex));
            fftBufferPtr = (Complex*)fftBuffer;
            fftWindowBuffer = UnsafeBuffer.Create(FftBufferSize, sizeof(float));
            fftWindowBufferPtr = (float*)fftWindowBuffer;
            fftSpectrumBuffer = UnsafeBuffer.Create(FftBufferSize, sizeof(float));
            fftSpectrumBufferPtr = (float*)fftSpectrumBuffer;
            fftSpectrumWaitingBuffer = UnsafeBuffer.Create(FftBufferSize, sizeof(T));
            fftSpectrumWaitingPtr = (T*)fftSpectrumWaitingBuffer;
            fftSpectrumFinalBuffer = UnsafeBuffer.Create(FftBufferSize, sizeof(float));
            fftSpectrumFinalBufferPtr = (float*)fftSpectrumFinalBuffer;
        }

        protected int bufferSizeMultiplier;
        protected UnsafeBuffer fftBuffer;
        protected Complex* fftBufferPtr;
        protected UnsafeBuffer fftWindowBuffer;
        protected float* fftWindowBufferPtr;
        protected UnsafeBuffer fftSpectrumBuffer;
        protected float* fftSpectrumBufferPtr;
        protected UnsafeBuffer fftSpectrumWaitingBuffer;
        protected T* fftSpectrumWaitingPtr;
        protected UnsafeBuffer fftSpectrumFinalBuffer;
        protected float* fftSpectrumFinalBufferPtr;

        public override event FFTProcessorOnBlockEventArgs OnBlockProcessed;

        private int waitingSamples;
        private int _fftBins;
        public override int FftBins {
            get => _fftBins;
            set
            {
                _fftBins = value;
                DestroyBuffers();
                CreateBuffers();
            }
        }
        public int FftOffset { get; set; } = -40;
        protected int FftBufferSize { get => FftBins * bufferSizeMultiplier; }

        protected abstract void CopyIncomingSamples(T* ptr, Complex* dest);

        private void BuildFFTWindow()
        {
            var window = FilterBuilder.MakeWindow(WindowType.BlackmanHarris7, FftBufferSize);
            fixed (float* windowPtr = window)
            {
                Utils.Memcpy(fftWindowBufferPtr, windowPtr, FftBufferSize * sizeof(float));
            }
        }

        private void ProcessIncomingBlock(T* ptr)
        {
            //Copy to buffer
            CopyIncomingSamples(ptr, fftBufferPtr);

            //Calculate FFT
            var fftGain = (float)(10.0 * Math.Log10((double)(FftBufferSize) / 2));
            var compensation = 24.0f - fftGain + FftOffset;
            Fourier.ApplyFFTWindow(fftBufferPtr, fftWindowBufferPtr, FftBufferSize);
            Fourier.ForwardTransform(fftBufferPtr, FftBufferSize);
            Fourier.SpectrumPower(fftBufferPtr, fftSpectrumBufferPtr, FftBufferSize, compensation);

            //Dispatch event
            OnBlockProcessed?.Invoke(OffsetPointerToBins(fftSpectrumBufferPtr), FftBins);            
        }

        public void AddSamples(T* ptr, int count)
        {
            while(count > 0)
            {
                //Transfer
                int waitingTransfer = Math.Min(FftBufferSize - waitingSamples, count);
                Utils.Memcpy(fftSpectrumWaitingPtr + waitingSamples, ptr, waitingTransfer * sizeof(T));

                //Update state
                count -= waitingTransfer;
                ptr += waitingTransfer;
                waitingSamples += waitingTransfer;

                //Process block
                if (waitingSamples == FftBufferSize)
                {
                    ProcessIncomingBlock(fftSpectrumWaitingPtr);
                    waitingSamples = 0;
                }
            }

            //Copy final block
            Utils.Memcpy(fftSpectrumFinalBufferPtr, fftSpectrumBufferPtr, FftBufferSize * sizeof(float));
        }

        public override void GetFFTSnapshot(float* frame)
        {
            //Copy the last FftBins floats from the final buffer
            Utils.Memcpy(frame, OffsetPointerToBins(fftSpectrumFinalBufferPtr), sizeof(float) * FftBins);
        }

        private float* OffsetPointerToBins(float* ptr)
        {
            return ptr + (FftBins * (bufferSizeMultiplier - 1));
        }

        public void Dispose()
        {
            DestroyBuffers();
        }
    }
}
