using RomanPort.LibSDR.Components;
using RomanPort.LibSDR.Components.FFT.Generators;
using RomanPort.LibSDR.Components.FFT.Mutators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanPort.LibSDR.UI
{
    public class FFTViewWrapper
    {
        public FFTViewWrapper(IPowerView view, int fftWidth, bool isHalf)
        {
            this.view = view;
            fft = new FFTGenerator(fftWidth, isHalf);
            smoothener = new FFTSmoothener(fft, attack, decay);
        }

        private IPowerView view;
        private FFTGenerator fft;
        private FFTSmoothener smoothener;

        private float attack = 0.4f;
        private float decay = 0.3f;

        public float Attack
        {
            get => attack;
            set
            {
                attack = value;
                if (smoothener != null)
                    smoothener.Attack = value;
            }
        }
        public float Decay
        {
            get => decay;
            set
            {
                decay = value;
                if (smoothener != null)
                    smoothener.Decay = value;
            }
        }

        public unsafe void AddSamples(Complex* ptr, int count)
        {
            fft.AddSamples(ptr, count);
        }

        public unsafe void AddSamples(float* ptr, int count)
        {
            fft.AddSamples(ptr, count);
        }

        public unsafe void DrawFrame()
        {
            float* power = smoothener.ProcessFFT(out int fftBins);
            view.WritePowerSamples(power, fftBins);
            view.DrawFrame();
        }
    }
}
