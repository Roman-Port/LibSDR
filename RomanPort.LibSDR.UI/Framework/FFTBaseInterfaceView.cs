using RomanPort.LibSDR.Components.FFT;
using RomanPort.LibSDR.Components.FFT.Mutators;
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

        protected FFTResizer fft;

        public void SetFFT(IFftMutatorSource fft)
        {
            this.fft = new FFTResizer(fft, Width);
        }

        public unsafe void RefreshFFT()
        {
            //If there is no FFT set, abort
            if (fft == null)
                return;

            //Grab frame
            fft.OutputSize = Width;
            float* fftFrame = fft.ProcessFFT(out int fftWidth);

            //Refresh
            RefreshFFT(fftFrame, fftWidth);
        }

        public unsafe abstract void RefreshFFT(float* pixelDbs, int count);
    }
}
