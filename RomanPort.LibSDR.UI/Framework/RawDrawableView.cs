using RomanPort.LibSDR.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RomanPort.LibSDR.UI.Framework
{
    public abstract unsafe class RawDrawableView : UserControl
    {
        private Bitmap img;
        private UnsafeBuffer imgBuffer;

        protected UnsafeColor* pixels;
        protected int pixelsWidth { get; private set; }
        protected int pixelsHeight { get; private set; }

        public RawDrawableView()
        {
            ResetImg();
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();
        }

        private void ResetImg()
        {
            //Get width/height
            int width = ClientRectangle.Width;
            int height = ClientRectangle.Height;

            //Dispose of old if needed
            if (img != null)
                img.Dispose();
            if (imgBuffer != null)
                imgBuffer.Dispose();

            //Create buffer and image
            imgBuffer = UnsafeBuffer.Create(width * height, sizeof(UnsafeColor));
            pixels = (UnsafeColor*)imgBuffer;
            img = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, (IntPtr)imgBuffer.Address);
            pixelsWidth = width;
            pixelsHeight = height;

            //Send to users
            DrawableViewReset(pixelsWidth, pixelsHeight);
        }

        protected abstract void DrawableViewReset(int width, int height);

        protected abstract void RenderDrawableView(int width, int height);

        private void RenderDrawableView()
        {
            RenderDrawableView(img.Width, img.Height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            ConfigureGraphics(e.Graphics);
            RenderDrawableView();
            e.Graphics.DrawImageUnscaled(img, 0, 0);
        }

        protected override void OnResize(EventArgs e)
        {
            //Clean up
            if (img != null)
                img.Dispose();

            //Create
            ResetImg();

            //Render
            RenderDrawableView();

            base.OnResize(e);
        }

        public static void ConfigureGraphics(Graphics graphics)
        {
            graphics.CompositingMode = CompositingMode.SourceOver;
            graphics.CompositingQuality = CompositingQuality.HighSpeed;
            graphics.SmoothingMode = SmoothingMode.None;
            graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            graphics.InterpolationMode = InterpolationMode.High;
        }
    }
}
