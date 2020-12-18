using RomanPort.LibSDR.Framework.Components.FFT;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanPort.LibSDR.UI.Framework
{
    public abstract class FFTBaseInterfaceView : RawDrawableView
    {
        private float _fftMaxDb = 0;
        public float FftMaxDb
        {
            get => _fftMaxDb;
            set
            {
                _fftMaxDb = value;
                FFTSettingsChanged();
            }
        }

        private float _fftMinDb = -80;
        public float FftMinDb
        {
            get => _fftMinDb;
            set
            {
                _fftMinDb = value;
                FFTSettingsChanged();
            }
        }

        public abstract void FFTSettingsChanged();

        /// <summary>
        /// Scales the incoming db float to 0-1
        /// </summary>
        /// <returns></returns>
        protected float ScaleDbToStandard(float db)
        {
            return (db - FftMinDb) / (FftMaxDb - FftMinDb);
        }

        protected FFTScaler fft;
        private UnsafeBuffer fftBuffer;
        private unsafe float* fftBufferPtr;

        public void SetFFT(FFTInterface fft)
        {
            this.fft = new FFTScaler(fft, Width);
            UpdateFFTWidth();
        }

        public unsafe void RefreshFFT()
        {
            //If there is no FFT set, abort
            if (fft == null)
                return;
            
            //Update width if needed
            if (fft.Width != Width)
                UpdateFFTWidth();

            //Update
            fft.GetFFTSnapshot(fftBufferPtr);
            RefreshFFT(fftBufferPtr, Width);
        }

        private unsafe void UpdateFFTWidth()
        {
            //Set
            fft.Width = Width;

            //Free buffer
            fftBuffer?.Dispose();

            //Create buffer
            fftBuffer = UnsafeBuffer.Create(Width, sizeof(float));
            fftBufferPtr = (float*)fftBuffer;
        }

        public unsafe abstract void RefreshFFT(float* pixelDbs, int count);
    }
}
