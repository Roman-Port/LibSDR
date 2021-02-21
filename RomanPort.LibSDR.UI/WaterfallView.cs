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
    public unsafe class WaterfallView : RawDrawableView, IPowerView
    {
        public WaterfallView()
        {

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

        private static readonly int[][] WATERFALL_GRADIENT_COLORS = new int[][]
        {
            new int[] {0, 0, 32},
            new int[] {0, 0, 48},
            new int[] {0, 0, 80},
            new int[] {0, 0, 145},
            new int[] {30, 144, 255},
            new int[] {255, 255, 255},
            new int[] {255, 255, 0},
            new int[] {254, 109, 22},
            new int[] {255, 0, 0},
            new int[] {198, 0, 0},
            new int[] {159, 0, 0},
            new int[] {117, 0, 0},
            new int[] {74, 0, 0 }
        };

        private float fftOffset = 0;
        private float fftRange = 100;

        private UnsafeColor[] precomputedColors;
        private UnsafeBuffer swapBuffer;
        private UnsafeColor* swapPtr;

        private UnsafeBuffer powerBuffer;
        private float* powerBufferPtr;

        private float* powerInputPtr;
        private int powerInputLen;

        protected override void Configure(int width, int height)
        {
            //Make new power buffer
            powerBuffer?.Dispose();
            powerBuffer = UnsafeBuffer.Create(width, out powerBufferPtr);

            //Create swap
            swapBuffer = UnsafeBuffer.Create(width * (height - 1), out swapPtr);

            //Precompute gradients
            RecalculateColors();
        }

        private void RecalculateColors()
        {
            precomputedColors = new UnsafeColor[byte.MaxValue];
            for (int i = 0; i < byte.MaxValue; i++)
                precomputedColors[i] = GetColor((float)i / (byte.MaxValue - 1));
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

            //Shift data downwards by copying it into the swap and then back
            Utils.Memcpy(swapPtr, ptr, Width * (Height - 1) * sizeof(UnsafeColor));
            Utils.Memcpy(ptr + Width, swapPtr, Width * (Height - 1) * sizeof(UnsafeColor));

            //Convert
            int index;
            for (int i = 0; i < CanvasWidth; i++)
            {
                float sample = fftPtr[i];
                sample += fftOffset;
                sample /= fftRange;
                sample *= precomputedColors.Length;
                index = (int)Math.Abs(sample);
                index = Math.Max(Math.Min(precomputedColors.Length - 1, index), 0);
                ptr[i] = precomputedColors[index];
            }

            //Invalidate
            InvalidateCanvas();
        }

        private UnsafeColor GetColor(float percent)
        {
            //Make sure percent is within range
            percent = 1 - percent;
            percent = Math.Max(0, percent);
            percent = Math.Min(1, percent);

            //Calculate
            var scale = WATERFALL_GRADIENT_COLORS.Length - 1;

            //Get the two colors to mix
            var mix2 = WATERFALL_GRADIENT_COLORS[(int)Math.Floor(percent * scale)];
            var mix1 = WATERFALL_GRADIENT_COLORS[(int)Math.Ceiling(percent * scale)];

            //Get ratio
            var ratio = (percent * scale) - Math.Floor(percent * scale);

            //Mix
            return new UnsafeColor(
                (byte)(Math.Ceiling((mix1[0] * ratio) + (mix2[0] * (1 - ratio)))),
                (byte)(Math.Ceiling((mix1[1] * ratio) + (mix2[1] * (1 - ratio)))),
                (byte)(Math.Ceiling((mix1[2] * ratio) + (mix2[2] * (1 - ratio))))
            );
        }

    }
}
