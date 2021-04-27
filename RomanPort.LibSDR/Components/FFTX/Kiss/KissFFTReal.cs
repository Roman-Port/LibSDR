using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.FFTX.Kiss
{
    public unsafe class KissFFTReal : KissFFT
    {
        public KissFFTReal(int nfft) : base(nfft, false)
        {
            tempbufBuffer = UnsafeBuffer.Create(nfft, out tmpbuf);
            superTwiddlesBuffer = UnsafeBuffer.Create(nfft, out super_twiddles);
        }

        private Complex* tmpbuf;
        private Complex* super_twiddles;

        private UnsafeBuffer tempbufBuffer;
        private UnsafeBuffer superTwiddlesBuffer;

        void kiss_fftr(float* timedata, Complex* freqdata)
        {
            /* input buffer timedata is stored row-wise */
            int k = 0;
            int ncfft = 0;
            Complex fpnk = 0;
            Complex fpk = 0;
            Complex f1k = 0;
            Complex f2k = 0;
            Complex tw = 0;
            Complex tdc = 0;

            /*perform the parallel fft of two real signals packed in real,imag*/
            kf_work(this.tmpbuf, (Complex*)timedata, 1, 1, factors);
            /* The real part of the DC element of the frequency spectrum in this.tmpbuf
             * contains the sum of the even-numbered elements of the input time sequence
             * The imag part is the sum of the odd-numbered elements
             *
             * The sum of tdc.r and tdc.i is the sum of the input time sequence.
             *      yielding DC of input time sequence
             * The difference of tdc.r - tdc.i is the sum of the input (dot product) [1,-1,1,-1...
             *      yielding Nyquist bin of input time sequence
             */

            tdc.Real = this.tmpbuf[0].Real;
            tdc.Imag = this.tmpbuf[0].Imag;
            freqdata[0].Real = tdc.Real + tdc.Imag;
            freqdata[ncfft].Real = tdc.Real - tdc.Imag;
            freqdata[ncfft].Imag = freqdata[0].Imag = 0;

            for (k = 1; k <= ncfft / 2; ++k)
            {
                fpk = this.tmpbuf[k];
                fpnk.Real = this.tmpbuf[ncfft - k].Real;
                fpnk.Imag = -this.tmpbuf[ncfft - k].Imag;

                C_ADD(&f1k, fpk, fpnk);
                C_SUB(&f2k, fpk, fpnk);
                C_MUL(&tw, f2k, this.super_twiddles[k - 1]);

                freqdata[k].Real = (f1k.Real + tw.Real) / 2;
                freqdata[k].Imag = (f1k.Imag + tw.Imag) / 2;
                freqdata[ncfft - k].Real = (f1k.Real - tw.Real) / 2;
                freqdata[ncfft - k].Imag = (tw.Imag - f1k.Imag) / 2;
            }
        }
    }
}
