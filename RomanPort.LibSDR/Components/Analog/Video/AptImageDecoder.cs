using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Analog.Video
{
    /// <summary>
    /// Processes raw frames right after they are AM demodulated and is downsampled
    /// </summary>
    public unsafe class AptImageDecoder
    {
        public AptImageDecoder(int width = 2080)
        {
            //Set
            this.width = width;
            
            //Make buffers
            frameBuffer = UnsafeBuffer.Create(width, out framePtr);
        }

        private int width;
        private UnsafeBuffer frameBuffer;
        private float* framePtr;
        private int framePtrIndex;

        public event AptImageDecoderFrameEvent OnFrame;

        public void ProcessSample(float sample)
        {
            PushPixelToFrameBuffer(sample);
        }

        private void PushPixelToFrameBuffer(float sample)
        {
            framePtr[framePtrIndex] = sample;
            framePtrIndex++;
            if (framePtrIndex >= width)
            {
                OnFrame?.Invoke(framePtr, width);
                framePtrIndex = 0;
            }
        }
    }

    public unsafe delegate void AptImageDecoderFrameEvent(float* ptr, int width);
}
