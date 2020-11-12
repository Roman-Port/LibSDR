using RomanPort.LibSDR.Framework.Extras;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework
{
    public unsafe abstract class FftView : IDisposable
    {
        public int fftBins;
        public int fftOffset = -40;
        public int averageSampleCount;

        internal int fftBinsBufferSize;

        internal UnsafeBuffer _fftBuffer;
        internal Complex* _fftPtr;
        internal UnsafeBuffer _fftWindow;
        internal float* _fftWindowPtr;
        internal UnsafeBuffer _fftSpectrum;
        internal float* _fftSpectrumPtr;
        internal UnsafeBuffer _scaledFFTSpectrum;
        internal byte* _scaledFFTSpectrumPtr;

        private DateTime lastFftEvent;
        private MovingAverage average;

        public FftView(int fftBins, int fftBinsBufferSize, int averageSampleCount)
        {
            this.fftBins = fftBins;
            this.fftBinsBufferSize = fftBinsBufferSize;
            this.averageSampleCount = averageSampleCount;
            average = new MovingAverage((uint)fftBins, (uint)averageSampleCount);
            CreateBuffers();
            BuildFFTWindow();
        }

        private void CreateBuffers()
        {
            _fftBuffer = UnsafeBuffer.Create(fftBinsBufferSize, sizeof(Complex));
            _fftWindow = UnsafeBuffer.Create(fftBinsBufferSize, sizeof(float));
            _fftSpectrum = UnsafeBuffer.Create(fftBinsBufferSize, sizeof(float));
            _scaledFFTSpectrum = UnsafeBuffer.Create(fftBinsBufferSize, sizeof(byte));

            _fftPtr = (Complex*)_fftBuffer;
            _fftWindowPtr = (float*)_fftWindow;
            _fftSpectrumPtr = (float*)_fftSpectrum;
            _scaledFFTSpectrumPtr = (byte*)_scaledFFTSpectrum;
        }

        private void BuildFFTWindow()
        {
            var window = FilterBuilder.MakeWindow(WindowType.BlackmanHarris7, fftBinsBufferSize);
            fixed (float* windowPtr = window)
            {
                Utils.Memcpy(_fftWindow, windowPtr, fftBinsBufferSize * sizeof(float));
            }
        }

        internal void BaseProcessSamples()
        {
            //Process FFT gain
            // http://www.designnews.com/author.asp?section_id=1419&doc_id=236273&piddl_msgid=522392
            var fftGain = (float)(10.0 * Math.Log10((double)fftBinsBufferSize / 2));
            var compensation = 24.0f - fftGain + fftOffset;

            //Calculate FFT
            Fourier.ApplyFFTWindow(_fftPtr, _fftWindowPtr, fftBinsBufferSize);
            Fourier.ForwardTransform(_fftPtr, fftBinsBufferSize);
            Fourier.SpectrumPower(_fftPtr, _fftSpectrumPtr, fftBinsBufferSize, compensation);

            //Add to moving average
            average.WriteBlock(_fftSpectrumPtr + (fftBinsBufferSize - fftBins));
        }

        public void GetFFTSnapshot(float* output)
        {
            average.ReadAverage(output);
        }

        public void ManagedGetFFTSnapshot(float[] output)
        {
            fixed (float* ptr = output)
                GetFFTSnapshot(ptr);
        }

        public float[] ManagedGetFFTSnapshot()
        {
            float[] f = new float[fftBins];
            ManagedGetFFTSnapshot(f);
            return f;
        }

        public void Dispose()
        {
            average.Dispose();
            _fftBuffer.Dispose();
            _fftWindow.Dispose();
            _fftSpectrum.Dispose();
            _scaledFFTSpectrum.Dispose();
        }
    }
}
