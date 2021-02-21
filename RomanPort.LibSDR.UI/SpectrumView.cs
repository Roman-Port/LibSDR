using RomanPort.LibSDR.Components;
using RomanPort.LibSDR.Components.FFT.Mutators;
using RomanPort.LibSDR.UI.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanPort.LibSDR.UI
{
    public unsafe class SpectrumView : RawDrawableView, IPowerView
    {
        public SpectrumView()
        {
            FftOffset = 0;
            FftRange = 100;
        }

        public float FftOffset
        {
            get => fftOffset;
            set => fftOffset = value;
        }
        public float FftRange
        {
            get => fftRange;
            set => fftRange = value;
        }
        public UnsafeColor GradientBrightTop
        {
            get => gradientBrightTop;
            set
            {
                gradientBrightTop = value;
                RecalculateGradients();
            }
        }
        public UnsafeColor GradientBrightBottom
        {
            get => gradientBrightBottom;
            set
            {
                gradientBrightBottom = value;
                RecalculateGradients();
            }
        }
        public UnsafeColor GradientDarkTop
        {
            get => gradientDarkTop;
            set
            {
                gradientDarkTop = value;
                RecalculateGradients();
            }
        }
        public UnsafeColor GradientDarkBottom
        {
            get => gradientDarkBottom;
            set
            {
                gradientDarkBottom = value;
                RecalculateGradients();
            }
        }

        private float fftOffset = 0;
        private float fftRange = 100;
        private UnsafeColor gradientBrightTop = new UnsafeColor(112, 180, 255);
        private UnsafeColor gradientBrightBottom = new UnsafeColor(0, 0, 80);
        private UnsafeColor gradientDarkTop = new UnsafeColor(112 / BACKGROUND_DIM_RATIO, 180 / BACKGROUND_DIM_RATIO, 255 / BACKGROUND_DIM_RATIO);
        private UnsafeColor gradientDarkBottom = new UnsafeColor(0 / BACKGROUND_DIM_RATIO, 0 / BACKGROUND_DIM_RATIO, 80 / BACKGROUND_DIM_RATIO);
        private UnsafeColor[] gradientDark;
        private UnsafeColor[] gradient;

        private const int BACKGROUND_DIM_RATIO = 4;

        private UnsafeBuffer powerBuffer;
        private float* powerBufferPtr;

        private float* powerInputPtr;
        private int powerInputLen;

        protected override void Configure(int width, int height)
        {
            //Make new power buffer
            powerBuffer?.Dispose();
            powerBuffer = UnsafeBuffer.Create(width, out powerBufferPtr);

            //Precompute gradients
            RecalculateGradients();
        }

        private void RecalculateGradients()
        {
            gradient = new UnsafeColor[CanvasHeight];
            gradientDark = new UnsafeColor[CanvasHeight];
            for (int i = 0; i < CanvasHeight; i++)
            {
                gradient[i] = InterpColor((float)i / (CanvasHeight - 1), gradientBrightBottom, gradientBrightTop);
                gradientDark[i] = InterpColor((float)i / (CanvasHeight - 1), gradientDarkBottom, gradientDarkTop);
            }
        }
        
        public void WritePowerSamples(float* power, int powerLen)
        {
            powerInputPtr = power;
            powerInputLen = powerLen;
        }

        public void DrawFrame()
        {
            //Mutate into the width we need
            FFTResizer.ResizeFFT(powerInputPtr, powerInputLen, powerBufferPtr, CanvasWidth);

            //Draw
            RawDrawFrame(powerBufferPtr);
        }

        public void RawDrawFrame(float* fftPtr)
        {
            //Get pointers
            UnsafeColor* ptr = CanvasBufferPtr;

            //Convert
            for (int i = 0; i < CanvasWidth; i++)
                fftPtr[i] = ((Math.Abs(fftPtr[i]) - fftOffset) / fftRange) * CanvasHeight;

            //Loop
            float max;
            float value;
            float min;
            for (var x = 0; x < CanvasWidth; x += 1)
            {
                //Get where this pixel is
                if (x == 0)
                {
                    //Cant access first
                    max = Math.Max(fftPtr[x], fftPtr[x + 1]);
                    value = fftPtr[x];
                    min = Math.Min(fftPtr[x], fftPtr[x + 1]);
                }
                else if (x == CanvasWidth - 1)
                {
                    //Can't access last
                    max = Math.Max(fftPtr[x - 1], fftPtr[x]);
                    value = fftPtr[x];
                    min = Math.Min(fftPtr[x - 1], fftPtr[x]);
                }
                else
                {
                    //Normal
                    max = Math.Max(fftPtr[x - 1], Math.Max(fftPtr[x], fftPtr[x + 1]));
                    value = fftPtr[x];
                    min = Math.Min(fftPtr[x - 1], Math.Min(fftPtr[x], fftPtr[x + 1]));
                }

                //Loop
                for (var y = 0; y < CanvasHeight; y++)
                {
                    //Get offset
                    var offset = ((y * CanvasWidth) + x);

                    //Determine color
                    if (y > max)
                    {
                        //Full gradient
                        ptr[offset] = gradient[y];
                    }
                    else if (y == value)
                    {
                        //Point
                        ptr[offset] = new UnsafeColor(255, 255, 255);
                    }
                    else if (y <= max && y > value)
                    {
                        //Interp top
                        var c = InterpColor((float)Math.Pow((y - value) / (max - value), 3), gradient[y], new UnsafeColor(255, 255, 255));
                        ptr[offset + 0] = c;
                    }
                    else if (y < value && y >= min)
                    {
                        //Intep bottom
                        var c = InterpColor((float)Math.Pow((y - min) / (value - min), 3), new UnsafeColor(255, 255, 255), gradientDark[y]);
                        ptr[offset + 0] = c;
                    }
                    else
                    {
                        //Dark gradient
                        ptr[offset] = gradientDark[y];
                    }
                }
            }

            //Invalidate
            InvalidateCanvas();
        }
    }
}
