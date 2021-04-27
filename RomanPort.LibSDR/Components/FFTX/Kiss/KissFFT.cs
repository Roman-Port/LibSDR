using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.FFTX.Kiss
{
    /*
    KissFFT, licensed under BSD-3-Clause, was ported to CSharp by myself. It can be obtained at https://github.com/mborgerding/kissfft
    Below is a copy of the license KissFFT was originally published under:

        Copyright (c) 2003-2010 Mark Borgerding . All rights reserved.
        KISS FFT is provided under:
        SPDX-License-Identifier: BSD-3-Clause
        Being under the terms of the BSD 3-clause "New" or "Revised" License,
        according with:
        LICENSES/BSD-3-Clause

    */

    public unsafe class KissFFT
    {
        public KissFFT(int nfft, bool inverse)
        {
            this.nfft = nfft;
            this.inverse = inverse;
            twiddlesBuffer = UnsafeBuffer.Create(nfft, out twiddles);
            factorsBuffer = UnsafeBuffer.Create(2 * MAXFACTORS, out factors);

            for (int i = 0; i < nfft; ++i)
            {
                double phase = -2 * Math.PI * i / nfft;
                if (inverse)
                    phase *= -1;
                kf_cexp(twiddles + i, phase);
            }

            kf_factor(nfft, factors);
        }

        protected const int MAXFACTORS = 32;

        protected int nfft;
        protected bool inverse;
        protected int* factors;
        protected Complex* twiddles;

        protected UnsafeBuffer factorsBuffer;
        protected UnsafeBuffer twiddlesBuffer;

        protected static void kf_cexp(Complex* x, double phase)
        {
            x->Real = (float)Math.Cos(phase);
            x->Imag = (float)Math.Sin(phase);
        }

        protected static void C_MUL(Complex* m, Complex a, Complex b)
        {
            m->Real = a.Real * b.Real - a.Imag * b.Imag;
            m->Imag = a.Real * b.Imag + a.Imag * b.Real;
        }

        protected static void C_ADDTO(Complex* res, Complex a)
        {
            res->Real += a.Real;
            res->Imag += a.Imag;
        }

        protected static void C_ADD(Complex* res, Complex a, Complex b)
        {
            res->Real = a.Real + b.Real;
            res->Imag = a.Imag + b.Imag;
        }

        protected static void C_SUB(Complex* res, Complex a, Complex b)
        {
            res->Real = a.Real - b.Real;
            res->Imag = a.Imag - b.Imag;
        }

        protected static void C_MULBYSCALAR(Complex* c, float s)
        {
            c->Real *= s;
            c->Imag *= s;
        }

        protected static float S_MUL(float a, float b)
        {
            return a * b;
        }

        void kf_factor(int n, int* facbuf)
        {
            int p = 4;
            double floor_sqrt = Math.Floor(Math.Sqrt(n));

            /*factor out powers of 4, powers of 2, then any remaining primes */
            do
            {
                while (n % p != 0)
                {
                    switch (p)
                    {
                        case 4: p = 2; break;
                        case 2: p = 3; break;
                        default: p += 2; break;
                    }
                    if (p > floor_sqrt)
                        p = n;          /* no more factors, skip to end */
                }
                n /= p;
                *facbuf++ = p;
                *facbuf++ = n;
            } while (n > 1);
        }

        protected void kf_work(Complex* Fout, Complex* f, int fstride, int in_stride, int* factors)
        {
            Complex* Fout_beg = Fout;
            int p = *factors++; /* the radix  */
            int m = *factors++; /* stage's fft length/p */
            Complex* Fout_end = Fout + p * m;

            if (m == 1)
            {
                do
                {
                    *Fout = *f;
                    f += fstride * in_stride;
                } while (++Fout != Fout_end);
            }
            else
            {
                do
                {
                    // recursive call:
                    // DFT of size m*p performed by doing
                    // p instances of smaller DFTs of size m,
                    // each one takes a decimated version of the input
                    kf_work(Fout, f, fstride * p, in_stride, factors);
                    f += fstride * in_stride;
                } while ((Fout += m) != Fout_end);
            }

            Fout = Fout_beg;

            // recombine the p smaller DFTs
            switch (p)
            {
                case 2: kf_bfly2(Fout, fstride, m); break;
                case 3: kf_bfly3(Fout, fstride, m); break;
                case 4: kf_bfly4(Fout, fstride, m); break;
                case 5: kf_bfly5(Fout, fstride, m); break;
                default: kf_bfly_generic(Fout, fstride, m, p); break;
            }
        }

        void kf_bfly2(Complex* Fout, int fstride, int m)
        {
            Complex* Fout2;
            Complex* tw1 = twiddles;
            Complex t;
            Fout2 = Fout + m;
            do
            {
                C_MUL(&t, *Fout2, *tw1);
                tw1 += fstride;
                C_SUB(Fout2, *Fout, t);
                C_ADDTO(Fout, t);
                ++Fout2;
                ++Fout;
            } while (--m != 0);
        }

        void kf_bfly3(Complex* Fout, int fstride, int m)
        {
            int k = m;
            int m2 = 2 * m;
            Complex* tw1;
            Complex* tw2;
            Complex epi3;
            epi3 = twiddles[fstride * m];

            tw1 = tw2 = twiddles;

            fixed (Complex* scratch = new Complex[5])
            {
                do
                {
                    C_MUL(&scratch[1], Fout[m], *tw1);
                    C_MUL(&scratch[2], Fout[m2], *tw2);

                    C_ADD(&scratch[3], scratch[1], scratch[2]);
                    C_SUB(&scratch[0], scratch[1], scratch[2]);
                    tw1 += fstride;
                    tw2 += fstride * 2;

                    Fout[m].Real = Fout->Real - (scratch[3].Real / 2);
                    Fout[m].Imag = Fout->Imag - (scratch[3].Imag / 2);

                    C_MULBYSCALAR(&scratch[0], epi3.Imag);

                    C_ADDTO(Fout, scratch[3]);

                    Fout[m2].Real = Fout[m].Real + scratch[0].Imag;
                    Fout[m2].Imag = Fout[m].Imag - scratch[0].Real;

                    Fout[m].Real -= scratch[0].Imag;
                    Fout[m].Imag += scratch[0].Real;

                    ++Fout;
                } while (--k != 0);
            }
        }

        void kf_bfly4(Complex* Fout, int fstride, int m)
        {
            Complex* tw1;
            Complex* tw2;
            Complex* tw3;
            int k = m;
            int m2 = 2 * m;
            int m3 = 3 * m;
            tw3 = tw2 = tw1 = twiddles;

            fixed (Complex* scratch = new Complex[6])
            {
                do
                {
                    C_MUL(&scratch[0], Fout[m], *tw1);
                    C_MUL(&scratch[1], Fout[m2], *tw2);
                    C_MUL(&scratch[2], Fout[m3], *tw3);

                    C_SUB(&scratch[5], *Fout, scratch[1]);
                    C_ADDTO(Fout, scratch[1]);
                    C_ADD(&scratch[3], scratch[0], scratch[2]);
                    C_SUB(&scratch[4], scratch[0], scratch[2]);
                    C_SUB(&Fout[m2], *Fout, scratch[3]);
                    tw1 += fstride;
                    tw2 += fstride * 2;
                    tw3 += fstride * 3;
                    C_ADDTO(Fout, scratch[3]);

                    if (inverse)
                    {
                        Fout[m].Real = scratch[5].Real - scratch[4].Imag;
                        Fout[m].Imag = scratch[5].Imag + scratch[4].Real;
                        Fout[m3].Real = scratch[5].Real + scratch[4].Imag;
                        Fout[m3].Imag = scratch[5].Imag - scratch[4].Real;
                    }
                    else
                    {
                        Fout[m].Real = scratch[5].Real + scratch[4].Imag;
                        Fout[m].Imag = scratch[5].Imag - scratch[4].Real;
                        Fout[m3].Real = scratch[5].Real - scratch[4].Imag;
                        Fout[m3].Imag = scratch[5].Imag + scratch[4].Real;
                    }
                    ++Fout;
                } while (--k != 0);
            }
        }

        void kf_bfly5(Complex* Fout, int fstride, int m)
        {
            Complex* Fout0;
            Complex* Fout1;
            Complex* Fout2;
            Complex* Fout3;
            Complex* Fout4;
            int u;
            Complex* twiddles = this.twiddles;
            Complex* tw;
            Complex ya, yb;
            ya = twiddles[fstride * m];
            yb = twiddles[fstride * 2 * m];

            Fout0 = Fout;
            Fout1 = Fout0 + m;
            Fout2 = Fout0 + 2 * m;
            Fout3 = Fout0 + 3 * m;
            Fout4 = Fout0 + 4 * m;

            tw = twiddles;

            fixed (Complex* scratch = new Complex[13])
            {
                for (u = 0; u < m; ++u)
                {
                    scratch[0] = *Fout0;

                    C_MUL(&scratch[1], *Fout1, tw[u * fstride]);
                    C_MUL(&scratch[2], *Fout2, tw[2 * u * fstride]);
                    C_MUL(&scratch[3], *Fout3, tw[3 * u * fstride]);
                    C_MUL(&scratch[4], *Fout4, tw[4 * u * fstride]);

                    C_ADD(&scratch[7], scratch[1], scratch[4]);
                    C_SUB(&scratch[10], scratch[1], scratch[4]);
                    C_ADD(&scratch[8], scratch[2], scratch[3]);
                    C_SUB(&scratch[9], scratch[2], scratch[3]);

                    Fout0->Real += scratch[7].Real + scratch[8].Real;
                    Fout0->Imag += scratch[7].Imag + scratch[8].Imag;

                    scratch[5].Real = scratch[0].Real + S_MUL(scratch[7].Real, ya.Real) + S_MUL(scratch[8].Real, yb.Real);
                    scratch[5].Imag = scratch[0].Imag + S_MUL(scratch[7].Imag, ya.Real) + S_MUL(scratch[8].Imag, yb.Real);

                    scratch[6].Real = S_MUL(scratch[10].Imag, ya.Imag) + S_MUL(scratch[9].Imag, yb.Imag);
                    scratch[6].Imag = -S_MUL(scratch[10].Real, ya.Imag) - S_MUL(scratch[9].Real, yb.Imag);

                    C_SUB(Fout1, scratch[5], scratch[6]);
                    C_ADD(Fout4, scratch[5], scratch[6]);

                    scratch[11].Real = scratch[0].Real + S_MUL(scratch[7].Real, yb.Real) + S_MUL(scratch[8].Real, ya.Real);
                    scratch[11].Imag = scratch[0].Imag + S_MUL(scratch[7].Imag, yb.Real) + S_MUL(scratch[8].Imag, ya.Real);
                    scratch[12].Real = -S_MUL(scratch[10].Imag, yb.Imag) + S_MUL(scratch[9].Imag, ya.Imag);
                    scratch[12].Imag = S_MUL(scratch[10].Real, yb.Imag) - S_MUL(scratch[9].Real, ya.Imag);

                    C_ADD(Fout2, scratch[11], scratch[12]);
                    C_SUB(Fout3, scratch[11], scratch[12]);

                    ++Fout0; ++Fout1; ++Fout2; ++Fout3; ++Fout4;
                }
            }
        }

        void kf_bfly_generic(Complex* Fout, int fstride, int m, int p)
        {
            int u, k, q1, q;
            Complex* twiddles = this.twiddles;
            Complex t;
            int Norig = nfft;

            fixed (Complex* scratch = new Complex[p])
            {
                for (u = 0; u < m; ++u)
                {
                    k = u;
                    for (q1 = 0; q1 < p; ++q1)
                    {
                        scratch[q1] = Fout[k];
                        k += m;
                    }

                    k = u;
                    for (q1 = 0; q1 < p; ++q1)
                    {
                        int twidx = 0;
                        Fout[k] = scratch[0];
                        for (q = 1; q < p; ++q)
                        {
                            twidx += fstride * k;
                            if (twidx >= Norig) twidx -= Norig;
                            C_MUL(&t, scratch[q], twiddles[twidx]);
                            C_ADDTO(&Fout[k], t);
                        }
                        k += m;
                    }
                }
            }
        }
    }
}
