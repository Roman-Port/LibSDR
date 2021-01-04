using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.Layer1.Parts
{
    unsafe class Firdecim_q15 : CTranslationLayer
    {
        public const int WINDOW_SIZE = 2048;

        UnsafeBuffer tapsBuffer;
        float* tapsBufferPtr;
        int ntaps;
        UnsafeBuffer windowBuffer;
        Complex* windowBufferPtr;
        int idx;

        public Firdecim_q15(float[] tapsSrc)
        {
            //ntaps = (tapsSrc.Length == 32) ? 32 : 15;
            ntaps = tapsSrc.Length;
            tapsBuffer = UnsafeBuffer.Create(ntaps * 2, sizeof(float));
            tapsBufferPtr = (float*)tapsBuffer;
            windowBuffer = UnsafeBuffer.Create(WINDOW_SIZE, sizeof(Complex));
            windowBufferPtr = (Complex*)windowBuffer;

            firdecim_q15_reset();

            // reverse order so we can push into the window
            // duplicate for neon
            for (int i = 0; i < ntaps; ++i)
            {
                tapsBufferPtr[i * 2] = tapsSrc[ntaps - 1 - i];
                tapsBufferPtr[i * 2 + 1] = tapsSrc[ntaps - 1 - i];
            }
        }

        public void firdecim_q15_reset()
        {
            this.idx = this.ntaps - 1;
        }

        void push(Complex x)
        {
            if (this.idx == WINDOW_SIZE)
            {
                for (int i = 0; i < this.ntaps - 1; i++)
                    this.windowBufferPtr[i] = this.windowBufferPtr[this.idx - this.ntaps + 1 + i];
                this.idx = this.ntaps - 1;
            }
            this.windowBufferPtr[this.idx++] = x;
        }

        Complex dotprod_32(Complex* a, float* b)
        {
            Complex sum = new Complex(0, 0);
            int i;

            for (i = 1; i < 16; i++)
            {
                sum.Real += ((a[i].Real + a[32 - i].Real) * b[i * 2]); //>> 15;
                sum.Imag += ((a[i].Imag + a[32 - i].Imag) * b[i * 2]); //>> 15;
            }
            sum.Real += (a[i].Real * b[i * 2]); //>> 15
            sum.Imag += (a[i].Imag * b[i * 2]); //>> 15

            return sum;
        }

        Complex dotprod_halfband_4(Complex* a, float* b)
        {
            Complex sum = new Complex(0, 0);
            int i;

            for (i = 0; i < 7; i += 2)
            {
                sum.Real += ((a[i].Real + a[14 - i].Real) * b[i]); //>> 15;
                sum.Imag += ((a[i].Imag + a[14 - i].Imag) * b[i]); //>> 15;
            }
            sum.Real += a[7].Real;
            sum.Imag += a[7].Imag;

            return sum;
        }

        public Complex fir_q15_execute(Complex x)
        {
            push(x);
            return dotprod_32(this.windowBufferPtr + (this.idx - this.ntaps), this.tapsBufferPtr);
        }

        public Complex halfband_q15_execute(Complex x1, Complex x2)
        {
            push(x1);
            Complex response = dotprod_halfband_4(this.windowBufferPtr + (this.idx - this.ntaps), this.tapsBufferPtr);
            push(x2);
            return response;
        }
    }
}
