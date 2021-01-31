using RomanPort.LibSDR.Components;
using RomanPort.LibSDR.UI.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RomanPort.LibSDR.UI
{
    public unsafe partial class FFTWaterfallView : FFTBaseInterfaceView
    {
        public FFTWaterfallView()
        {
            InitializeComponent();
        }

        private UnsafeBuffer pixelsBuffer;
        private UnsafeColor* pixelsBufferPtr;
        private int pixelsBufferPixelCount;

        private static readonly UnsafeColor[] WATERFALL_COLORS = new UnsafeColor[]
        {
            new UnsafeColor(0, 0, 32),
            new UnsafeColor(0, 0, 48),
            new UnsafeColor(0, 0, 80),
            new UnsafeColor(0, 0, 145),
            new UnsafeColor(30, 144, 255),
            new UnsafeColor(255, 255, 255),
            new UnsafeColor(255, 255, 0),
            new UnsafeColor(254, 109, 22),
            new UnsafeColor(255, 0, 0),
            new UnsafeColor(198, 0, 0),
            new UnsafeColor(159, 0, 0),
            new UnsafeColor(117, 0, 0),
            new UnsafeColor(74, 0, 0)
        };

        public override unsafe void RefreshFFT(float* samples, int count)
        {
            //Memcopy the upper part of this spectrum to our swap and then memcopy it back, down a line
            Utils.Memcpy(pixelsBufferPtr, pixels, pixelsBufferPixelCount * sizeof(UnsafeColor));
            Utils.Memcpy(pixels + pixelsWidth, pixelsBufferPtr, pixelsBufferPixelCount * sizeof(UnsafeColor));

            //Render
            float decimationScale = count / (float)pixelsWidth;
            for(int i = 0; i<pixelsWidth; i++)
            {
                //Scale value to fit within the range of 0-1
                float percent = ScaleDbToStandard(samples[(int)(i * decimationScale)]);

                //Get color and set
                pixels[i] = GetGradientColor(percent);
            }

            //Request redraw
            Invalidate();
        }

        protected override void DrawableViewReset(int width, int height)
        {
            //Dispose of old buffer
            if (pixelsBuffer != null)
                pixelsBuffer.Dispose();

            //Create new 
            pixelsBuffer = UnsafeBuffer.Create(width * (height - 1), sizeof(UnsafeColor));
            pixelsBufferPtr = (UnsafeColor*)pixelsBuffer;
            pixelsBufferPixelCount = (width * (height - 1));

            //Fill with black
            for (int i = 0; i < pixelsBufferPixelCount; i++)
            {
                pixels[i].a = byte.MaxValue;
                pixels[i].r = byte.MinValue;
                pixels[i].g = byte.MinValue;
                pixels[i].b = byte.MinValue;
            }
        }

        protected override unsafe void RenderDrawableView(int width, int height)
        {
            
        }

        private UnsafeColor GetGradientColor(float percent)
        {
            //Make sure percent is within range
            percent = Math.Max(0, percent);
            percent = Math.Min(1, percent);

            //Calculate
            var scale = WATERFALL_COLORS.Length - 1;

            //Get the two colors to mix
            var mix2 = WATERFALL_COLORS[(int)Math.Floor(percent * scale)];
            var mix1 = WATERFALL_COLORS[(int)Math.Ceiling(percent * scale)];

            //Get ratio
            float ratio = (percent * scale) - (int)(percent * scale);

            //Mix
            return new UnsafeColor(
                (byte)((mix1.r * ratio) + (mix2.r * (1 - ratio))),
                (byte)((mix1.g * ratio) + (mix2.g * (1 - ratio))),
                (byte)((mix1.b * ratio) + (mix2.b * (1 - ratio)))
            );
        }

        public override void FFTSettingsChanged()
        {
            
        }
    }
}
