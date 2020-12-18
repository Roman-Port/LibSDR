using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using RomanPort.LibSDR.Framework.Util;
using RomanPort.LibSDR.UI.Framework;

namespace RomanPort.LibSDR.UI
{
    public unsafe partial class FFTSpectrumView : FFTBaseInterfaceView
    {
        private UnsafeColor[] spectrumGradient;
        private UnsafeColor[] spectrumGradientHalf;

        private UnsafeBuffer fftBuffer;
        private float* fftBufferPtr;
        private int fftBufferSize;

        private UnsafeBuffer fftPixelBuffer;
        private float* fftPixelBufferPtr;
        private float* fftPixelMaxBufferPtr;
        private float* fftPixelMinBufferPtr;

        public static readonly UnsafeColor SPECTRUM_TOP_COLOR = new UnsafeColor( 112, 180, 255 );
        public static readonly UnsafeColor SPECTRUM_BOTTOM_COLOR = new UnsafeColor( 0, 0, 80 );

        public FFTSpectrumView()
        {
            InitializeComponent();
        }

        public override void RefreshFFT(float* data, int dataSize)
        {
            //Ensure buffer
            if(fftBufferSize != dataSize)
            {
                //We'll need to resize the buffer
                if (fftBuffer != null)
                    fftBuffer.Dispose();
                fftBuffer = UnsafeBuffer.Create(dataSize, sizeof(float));
                fftBufferPtr = (float*)fftBuffer;
                fftBufferSize = dataSize;
            }

            //Copy
            Utils.Memcpy(fftBufferPtr, data, dataSize * sizeof(float));

            //Invalidate to request a draw
            Invalidate();
        }

        protected override void DrawableViewReset(int width, int height)
        {
            //Precompute the gradient for fast lookup
            PrecomputeGradients(height);

            //Create the pixel data array (which we split into three pointers)
            if (fftPixelBuffer != null)
                fftPixelBuffer.Dispose();
            fftPixelBuffer = UnsafeBuffer.Create(width * 3, sizeof(float));
            fftPixelBufferPtr = (float*)fftPixelBuffer;
            fftPixelMaxBufferPtr = fftPixelBufferPtr + width;
            fftPixelMinBufferPtr = fftPixelMaxBufferPtr + width;
        }

        protected override void RenderDrawableView(int width, int height)
        {
            //Scale to pixel space
            if(fftBufferPtr != null)
            {
                float decimationScale = fftBufferSize / (float)width;
                for (int i = 0; i < width; i++)
                    fftPixelBufferPtr[i] = (1 - ScaleDbToStandard(fftBufferPtr[(int)(decimationScale * i)])) * height;
            } else
            {
                //No data yet. Set all to 0
                for (int i = 0; i < width; i++)
                    fftPixelBufferPtr[i] = 0;
            }
            
            //Determine the min/max of each
            for (int i = 1; i < width - 2; i++)
            {
                fftPixelMaxBufferPtr[i] = Math.Max(Math.Max(fftPixelBufferPtr[i + 1], fftPixelBufferPtr[i - 1]), fftPixelBufferPtr[i]);
                fftPixelMinBufferPtr[i] = Math.Min(Math.Min(fftPixelBufferPtr[i + 1], fftPixelBufferPtr[i - 1]), fftPixelBufferPtr[i]);
            }

            //Determine min/max of edge pixels
            fftPixelMaxBufferPtr[0] = Math.Max(fftPixelBufferPtr[0], fftPixelBufferPtr[1]);
            fftPixelMinBufferPtr[0] = Math.Min(fftPixelBufferPtr[0], fftPixelBufferPtr[1]);
            fftPixelMaxBufferPtr[width - 1] = Math.Max(fftPixelBufferPtr[width - 1], fftPixelBufferPtr[width - 2]);
            fftPixelMinBufferPtr[width - 1] = Math.Min(fftPixelBufferPtr[width - 1], fftPixelBufferPtr[width - 2]);

            //Draw each scanline
            for (int y = 0; y<height; y++)
            {
                //Get pointer to these pixels
                UnsafeColor* scanline = pixels + (y * width);

                //Loop each pixel
                for(int x = 0; x<width; x++)
                {
                    if (y < fftPixelMaxBufferPtr[x] && y > fftPixelBufferPtr[x])
                    {
                        //On the top part of a line
                        scanline[x] = InterpColor(spectrumGradient[y], UnsafeColor.WHITE, (y - fftPixelBufferPtr[x]) / (fftPixelMaxBufferPtr[x] - fftPixelBufferPtr[x]));
                    }
                    else if (y < fftPixelBufferPtr[x] && y > fftPixelMinBufferPtr[x])
                    {
                        //On the bottom part of a line
                        scanline[x] = InterpColor(UnsafeColor.WHITE, spectrumGradientHalf[y], (y - fftPixelMinBufferPtr[x]) / (fftPixelBufferPtr[x] - fftPixelMinBufferPtr[x]));
                    }
                    else if (y > fftPixelBufferPtr[x])
                    {
                        //Render foreground
                        scanline[x] = spectrumGradient[y];
                    }
                    else if (y < fftPixelBufferPtr[x])
                    {
                        //Render background
                        scanline[x] = spectrumGradientHalf[y];
                    } else
                    {
                        //If this is hit, we are spot on the value
                        //This will likely NEVER happen, but if it does, buy a lottery ticket...or check your floating point math
                        scanline[x] = UnsafeColor.WHITE;
                    }
                }
            }
        }

        private void PrecomputeGradients(int height)
        {
            spectrumGradient = new UnsafeColor[height];
            spectrumGradientHalf = new UnsafeColor[height];
            for (int i = 0; i < height; i++)
            {
                float scale = i * (1 / (float)height);
                UnsafeColor c = InterpColor(SPECTRUM_BOTTOM_COLOR, SPECTRUM_TOP_COLOR, scale);
                spectrumGradient[i] = c;
                spectrumGradientHalf[i] = new UnsafeColor(
                    (byte)(c.r / 4),
                    (byte)(c.g / 4),
                    (byte)(c.b / 4)
                );
            }
        }

        private UnsafeColor InterpColor(UnsafeColor a, UnsafeColor b, float percent)
        {
            var invPercent = 1 - percent;
            return new UnsafeColor(
                (byte)((a.r * percent) + (b.r * invPercent)),
                (byte)((a.g * percent) + (b.g * invPercent)),
                (byte)((a.b * percent) + (b.b * invPercent))
            );
        }

        public override void FFTSettingsChanged()
        {
            
        }
    }
}
