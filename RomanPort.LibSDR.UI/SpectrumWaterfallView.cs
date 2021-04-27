using RomanPort.LibSDR.Components;
using RomanPort.LibSDR.Components.FFT.Mutators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RomanPort.LibSDR.UI
{
    public unsafe partial class SpectrumWaterfallView : UserControl, IPowerView
    {
        public SpectrumWaterfallView()
        {
            InitializeComponent();
        }

        public float FftOffset
        {
            get => spectrumView.FftOffset;
            set
            {
                spectrumView.FftOffset = value;
                waterfallView.FftOffset = value;
            }
        }
        public float FftRange
        {
            get => spectrumView.FftRange;
            set
            {
                spectrumView.FftRange = value;
                waterfallView.FftRange = value;
            }
        }
        public int SpectrumHeight
        {
            get => spectrumView.Height;
            set {
                spectrumView.Height = value;
                waterfallView.Top = value + 5;
                waterfallView.Height = Height - value - 5;
            }
        }

        private UnsafeBuffer powerBuffer;
        private float* powerBufferPtr;

        private float* powerInputPtr;
        private int powerInputLen;

        public void WritePowerSamples(float* power, int powerLen)
        {
            powerInputPtr = power;
            powerInputLen = powerLen;
        }

        public void DrawFrame()
        {
            //Mutate into the width we need
            if(powerInputPtr != null)
                FFTResizer.ResizeFFT(powerInputPtr, powerInputLen, powerBufferPtr, Width);

            //Draw
            RawDrawFrame(powerBufferPtr);
        }

        public void RawDrawFrame(float* fftPtr)
        {
            //ATTENTION: It is important that the waterfall updates first, as it is nondestructive to our buffer. The spectrum, however, is destructive to the buffer
            waterfallView.RawDrawFrame(fftPtr);
            spectrumView.RawDrawFrame(fftPtr);
        }

        private void Configure()
        {
            //Make new power buffer
            powerBuffer?.Dispose();
            powerBuffer = UnsafeBuffer.Create(Width, out powerBufferPtr);
        }

        private void SpectrumWaterfallView_Resize(object sender, EventArgs e)
        {
            Configure();
        }

        private void SpectrumWaterfallView_Load(object sender, EventArgs e)
        {
            Configure();
        }
    }
}
