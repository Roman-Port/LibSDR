﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Analog.Primitive
{
    public class FmBasebandDemodulator
    {
        public FmBasebandDemodulator(float fmDeviation = DEVIATION_BROADCAST)
        {
            if (fmDeviation <= 0)
                throw new Exception("FmDeviation must be greater than zero!");
            this.fmDeviation = fmDeviation;
        }

        public const float DEVIATION_BROADCAST = 80000; //Supposed to be 75000, but in practice seems a little higher

        public float FmDeviation
        {
            get => fmDeviation;
            set
            {
                if (value <= 0)
                    throw new Exception("FmDeviation must be greater than zero!");
                fmDeviation = value;
                Configure();
            }
        }

        public float FmGain { get => gain; }

        private Complex lastSample;
        private float sampleRate;
        private float gain;
        private float fmDeviation;
        private float m; //temp

        public unsafe int Demodulate(Complex* iq, float* audio, int count)
        {
            //Check
            if (gain == 0)
                throw new Exception("You must call Configure before use!");
            
            //Process
            for (var i = 0; i < count; i++)
            {
                //Polar discriminator
                lastSample = iq[i] * lastSample.Conjugate();

                //Limiting
                m = lastSample.Modulus();
                if (m > 0.0f)
                {
                    lastSample /= m;
                }

                //Angle estimate
                audio[i] = lastSample.Argument() * gain;

                //Update state
                lastSample = iq[i];
            }

            return count;
        }

        public FmBasebandDemodulator Configure(int bufferSize, float sampleRate)
        {
            this.sampleRate = sampleRate;
            return Configure();
        }

        private FmBasebandDemodulator Configure()
        {
            if (fmDeviation != 0)
                gain = sampleRate / (2 * MathF.PI * fmDeviation); //POSSIBLE BUG: do we swap these around?
            else
                gain = 0;
            return this;
        }
    }
}
