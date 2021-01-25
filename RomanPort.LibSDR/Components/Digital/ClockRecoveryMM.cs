using RomanPort.LibSDR.Components.Base;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Digital
{
    /// <summary>
    /// This is modeled after, but not immediately copying, the clock_recovery_mm block in GNURadio.
    /// This uses Mueller and Müller clock recovery.
    /// https://github.com/gnuradio/gnuradio/blob/master/gr-digital/lib/clock_recovery_mm_ff_impl.cc
    /// </summary>
    public unsafe class ClockRecoveryMM : BaseProcessorStreamed<float>
    {
        public ClockRecoveryMM(float omega, float gainOmega, float mu, float gainMu, float omegaRelativeLimit) : base(1024)
        {
            this.omega = omega;
            this.gainOmega = gainOmega;
            this.mu = mu;
            this.gainMu = gainMu;
            this.omegaRelativeLimit = omegaRelativeLimit;
            omegaMid = omega;
            omegaLim = omegaRelativeLimit * omega;
            originalOmega = omega;
            originalMu = mu;
        }

        private float originalOmega;
        private float originalMu;

        private float mu;
        private float gainMu;
        private float omega;
        private float gainOmega;
        private float omegaRelativeLimit;
        private float omegaMid;
        private float omegaLim;
        private float lastSample;

        public void Reset()
        {
            omega = originalOmega;
            mu = originalMu;
        }

        protected override unsafe int ProcessBlock(float* input, float* output, int count, int outputCount, out int inputIndex)
        {
            inputIndex = 0;
            int outputIndex = 0;
            int consumable = count - 8;

            //Loop through until we run out of space for something
            while (outputIndex < outputCount && inputIndex < consumable)
            {
                //Produce output
                output[outputIndex] = Interpolate(&input[inputIndex], mu);
                float mm_val = Binaryify(lastSample) * output[outputIndex] - Binaryify(output[outputIndex]) * lastSample;
                lastSample = output[outputIndex];

                //Calculate error
                omega = omega + gainOmega * mm_val;
                omega = omegaMid + (0.5f * (MathF.Abs((omega - omegaMid) + omegaLim) - MathF.Abs((omega - omegaMid) - omegaLim)));
                mu = mu + omega + gainMu * mm_val;

                //Offset by error
                inputIndex += (int)MathF.Floor(mu);
                mu = mu - MathF.Floor(mu);
                outputIndex++;
            }
            return outputIndex;
        }

        float Binaryify(float x) {
            return x < 0 ? -1.0F : 1.0F;
        }

        /// <summary>
        /// This page was very helpful in understanding how this works: https://edfuentetaja.github.io/sdr/m_m_gnu_radio_analysis/
        /// 
        /// "The interpolating filter is going to use samples i0 to i7 and produce an interpolated value between the central samples i3 and i4 according to the value of d_mu.
        /// With d_mu=0 the interpolated value should return exactly i3, with d_mu=1 it should return exactly i4, and with d_mu=0.5 it should return the interpolated value midway between them."
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="mu"></param>
        /// <returns></returns>
        static float Interpolate(float* samples, float mu)
        {
            return (samples[3] * (1 - mu)) + (samples[4] * mu);
        }
    }
}
