using RomanPort.LibSDR.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RomanPort.LibSDR.UI.Framework
{
    public abstract unsafe partial class RawDrawableView : UserControl
    {
        public RawDrawableView()
        {
            InitializeComponent();
        }

        private void RawDrawableView_Load(object sender, EventArgs e)
        {
            InternalConfigure();
        }

        private void Resized(object sender, EventArgs e)
        {
            InternalConfigure();
        }

        private void InternalConfigure()
        {
            //Dispose of old buffer if needed
            imageBuffer?.Dispose();

            //Get size
            canvasHeight = Height;
            canvasWidth = Width;

            //Resize canvas
            canvas.Width = canvasWidth;
            canvas.Height = canvasHeight;

            //Make new buffer
            imageBuffer = UnsafeBuffer.Create(canvasWidth * canvasHeight, out imageBufferPtr);

            //Apply
            canvas.Image = new Bitmap(canvasWidth, canvasHeight, canvasWidth * sizeof(UnsafeColor), System.Drawing.Imaging.PixelFormat.Format32bppArgb, (IntPtr)imageBufferPtr);

            //Run configure
            Configure(canvasWidth, canvasHeight);
        }

        private int canvasWidth;
        private int canvasHeight;
        private UnsafeBuffer imageBuffer;
        private UnsafeColor* imageBufferPtr;

        protected int CanvasWidth { get => canvasWidth; }
        protected int CanvasHeight { get => canvasHeight; }
        protected UnsafeColor* CanvasBufferPtr { get => imageBufferPtr; }

        protected abstract void Configure(int width, int height);

        protected void InvalidateCanvas()
        {
            canvas.Invalidate();
        }

        protected static UnsafeColor InterpColor(float percent, UnsafeColor c1, UnsafeColor c2)
        {
            var invPercent = 1 - percent;
            return new UnsafeColor(
                (byte)((c1.r * percent) + (c2.r * invPercent)),
                (byte)((c1.g * percent) + (c2.g * invPercent)),
                (byte)((c1.b * percent) + (c2.b * invPercent))
            );
        }
    }
}
