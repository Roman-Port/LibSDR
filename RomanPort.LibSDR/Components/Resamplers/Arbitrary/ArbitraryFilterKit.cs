using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Resamplers.Arbitrary
{
    /// <summary>
    ///     reference: "Digital Filters, 2nd edition"
    ///     This file provides Kaiser-windowed low-pass filter support,
    ///     including a function to create the filter coefficients, and
    ///     two functions to apply the filter at a particular point.
    ///     reference: "Digital Filters, 2nd edition"
    ///     R.W. Hamming, pp. 178-179
    ///     Izero() computes the 0th order modified bessel function of the first kind.
    ///     (Needed to compute Kaiser window).
    ///     LpFilter() computes the coeffs of a Kaiser-windowed low pass filter with
    ///     the following characteristics:
    ///     c[]  = array in which to store computed coeffs
    ///     frq  = roll-off frequency of filter
    ///     N    = Half the window length in number of coeffs
    ///     Beta = parameter of Kaiser window
    ///     Num  = number of coeffs before 1/frq
    ///     Beta trades the rejection of the lowpass filter against the transition
    ///     width from passband to stopband.  Larger Beta means a slower
    ///     transition and greater stopband rejection.  See Rabiner and Gold
    ///     (Theory and Application of DSP) under Kaiser windows for more about
    ///     Beta.  The following table from Rabiner and Gold gives some feel
    ///     for the effect of Beta:
    ///     All ripples in dB, width of transition band = D*N where N = window length
    ///     BETA    D       PB RIP   SB RIP
    ///     2.120   1.50  +-0.27      -30
    ///     3.384   2.23    0.0864    -40
    ///     4.538   2.93    0.0274    -50
    ///     5.658   3.62    0.00868   -60
    ///     6.764   4.32    0.00275   -70
    ///     7.865   5.0     0.000868  -80
    ///     8.960   5.7     0.000275  -90
    ///     10.056  6.4     0.000087  -100
    /// </summary>
    class ArbitraryFilterKit
    {
        public const int Npc = 4096;
        private const double ZeroEpsilon = 1e-21;

        private static double GetIdealZero(double x)
        {
            double u;
            int n;

            var sum = u = n = 1;
            var halfx = x / 2.0;
            do
            {
                var temp = halfx / n;
                n += 1;
                temp *= temp;
                u *= temp;
                sum += u;
            } while (u >= ZeroEpsilon * sum);

            return sum;
        }

        public static void LrsLpFilter(double[] c, int n, double frq, double beta, int num)
        {
            double temp;
            int i;

            // Calculate ideal lowpass filter impulse response :
            c[0] = 2.0 * frq;
            for (i = 1; i < n; i++)
            {
                temp = MathF.PI * i / num;
                c[i] = Math.Sin(2.0 * temp * frq) / temp; // Analog sinc function
            }

            /*
             * Calculate and Apply Kaiser window to ideal lowpass filter. Note: last
             * window value is IBeta which is NOT zero. You're supposed to really
             * truncate the window here, not ramp it to zero. This helps reduce the
             * first sidelobe.
             */
            var ibeta = 1.0 / GetIdealZero(beta);
            var inm1 = 1.0 / (n - 1);
            for (i = 1; i < n; i++)
            {
                temp = i * inm1;
                var temp1 = 1.0 - temp * temp;
                temp1 = (temp1 < 0 ? 0 : temp1);
                c[i] *= GetIdealZero(beta * Math.Sqrt(temp1)) * ibeta;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="imp">impulse response</param>
        /// <param name="impd">impulse response deltas</param>
        /// <param name="nwing">length of one wing of filter</param>
        /// <param name="interp">interpolate coefs using deltas</param>
        /// <param name="xpArray">current sample array</param>
        /// <param name="xpIndex">current sample index</param>
        /// <param name="ph">phase</param>
        /// <param name="inc">increment (1 for right wing or -1 for left)</param>
        /// <returns></returns>
        public static float LrsFilterUp(float[] imp, float[] impd, int nwing, bool interp, float[] xpArray, int xpIndex,
            double ph, int inc)
        {
            double a = 0;
            float t;

            ph *= Npc; // Npc is number of values per 1/delta in impulse

            var v = 0.0f;

            var hpArray = imp;
            var hpIndex = (int)ph;

            var endIndex = nwing;

            var hdpArray = impd;
            var hdpIndex = (int)ph;

            if (interp)
            {
                a = ph - Math.Floor(ph); //fractional part of Phase
            }

            if (inc == 1) // If doing right wing...
            {
                // ...drop extra coeff, so when ph is
                endIndex--; // 0.5 we don't do too many mult's
                if (ph.Equals(0))
                {
                    hpIndex += Npc; // first sample, so we must also 
                    hdpIndex += Npc; // skip ahead in imp[] and impd[]
                }
            }

            if (interp)
            {
                while (hpIndex < endIndex)
                {
                    t = hpArray[hpIndex];
                    t += (float)(hdpArray[hdpIndex] * a);
                    hdpIndex += Npc;
                    t *= xpArray[xpIndex];
                    v += t;
                    hpIndex += Npc;
                    xpIndex += inc;
                }
            }
            else
            {
                while (hpIndex < endIndex)
                {
                    t = hpArray[hpIndex];
                    t *= xpArray[xpIndex];
                    v += t;
                    hpIndex += Npc;
                    xpIndex += inc;
                }
            }

            return v;
        }

        /// <summary>
        /// </summary>
        /// <param name="imp">impulse response</param>
        /// <param name="impd">impulse response deltas</param>
        /// <param name="nwing">length of one wing of filter</param>
        /// <param name="interp">interpolate coefs using deltas</param>
        /// <param name="xpArray">current sample array</param>
        /// <param name="xpIndex">current sample index</param>
        /// <param name="ph">phase</param>
        /// <param name="inc">increment (1 for right wing or -1 for left)</param>
        /// <param name="dhb">filter sampling period</param>
        /// <returns></returns>
        public static float LrsFilterUd(float[] imp, float[] impd, int nwing, bool interp, float[] xpArray, int xpIndex,
            double ph, int inc, double dhb)
        {
            float t;

            var v = 0.0f;
            var ho = ph * dhb;

            var endIndex = nwing;

            if (inc == 1)
            {
                endIndex--;
                if (ph.Equals(0))
                {
                    ho += dhb;
                }
            }

            var hpArray = imp;
            int hpIndex;

            if (interp)
            {
                var hdpArray = impd;

                while ((hpIndex = (int)ho) < endIndex)
                {
                    t = hpArray[hpIndex];
                    var hdpIndex = (int)ho;
                    var a = (float)(ho - Math.Floor(ho));

                    t += hdpArray[hdpIndex] * a;
                    t *= xpArray[xpIndex];
                    v += t;
                    ho += dhb;
                    xpIndex += inc;
                }
            }
            else
            {
                while ((hpIndex = (int)ho) < endIndex)
                {
                    t = hpArray[hpIndex];
                    t *= xpArray[xpIndex];
                    v += t;
                    ho += dhb;
                    xpIndex += inc;
                }
            }

            return v;
        }
    }
}
