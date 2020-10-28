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

namespace RomanPort.LibSDR.UI
{
    public unsafe partial class FFTSpectrumView : UserControl
    {
        private Bitmap _buffer;
        private Graphics _graphics;

        private UnsafeBuffer dataBuffer;
        private float* dataBufferPtr;
        private int dataSize = -1;

        public float maxDb = 0;
        public float minDb = -100;

        public FFTSpectrumView()
        {
            InitializeComponent();
            _buffer = new Bitmap(ClientRectangle.Width, ClientRectangle.Height, PixelFormat.Format32bppPArgb);
            _graphics = Graphics.FromImage(_buffer);

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();
        }

        public void UpdateFFT(float* data, int dataSize)
        {
            //Reset buffer if needed
            if(dataSize != this.dataSize)
            {
                //Free old
                if (dataBuffer != null)
                    dataBuffer.Dispose();

                //Create new
                dataBuffer = UnsafeBuffer.Create(dataSize, sizeof(float));
                dataBufferPtr = (float*)dataBuffer;
                this.dataSize = dataSize;
            }

            //Copy to
            Utils.Memcpy(dataBufferPtr, data, dataSize * sizeof(float));
            Invalidate();
        }

        private void RenderFFT()
        {
            //Clear
            _graphics.Clear(Color.Black);

            //Get size
            int width = ClientRectangle.Width;
            int height = ClientRectangle.Height;

            //Write data
            if (dataSize != -1)
            {
                //Determine if we need to draw by interpolating points or pixels
                Point[] points;
                if (dataSize > width)
                {
                    //More points than pixels. Interpolate by pixel
                    points = new Point[width];
                    float scale = (float)dataSize / width;
                    for (int i = 0; i < width; i++)
                    {
                        //Get index to query
                        int dataIndex = (int)(scale * i);

                        //Write
                        points[i] = new Point(i, GetPixelFromData(dataBufferPtr[dataIndex], height));
                    }
                }
                else
                {
                    //More pixels than points. Interpolate by point
                    points = new Point[dataSize];
                    float scale = (float)width / dataSize;
                    for (int i = 0; i < dataSize; i++)
                    {
                        //Get index to draw
                        int xIndex = (int)(scale * i);

                        //Write
                        points[i] = new Point(xIndex, GetPixelFromData(dataBufferPtr[i], height));
                    }
                }

                //Draw
                _graphics.DrawLines(new Pen(Color.Red, 1), points);
            }
        }

        private int GetPixelFromData(float value, int areaHeight)
        {
            //Determine place in scale
            float pos = value / (maxDb - minDb);

            //Clamp
            if (pos > 0)
                pos = 0;
            if (pos < -1)
                pos = -1;

            //Get real pixel location
            return (int)(-pos * areaHeight);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            ConfigureGraphics(e.Graphics);
            RenderFFT();
            e.Graphics.DrawImageUnscaled(_buffer, 0, 0);
        }

        protected override void OnResize(EventArgs e)
        {
            //Clean up
            if(_graphics != null)
                _graphics.Dispose();
            if(_buffer != null)
                _buffer.Dispose();

            //Create
            _buffer = new Bitmap(ClientRectangle.Width, ClientRectangle.Height, PixelFormat.Format32bppPArgb);
            _graphics = Graphics.FromImage(_buffer);

            //Render
            RenderFFT();

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
