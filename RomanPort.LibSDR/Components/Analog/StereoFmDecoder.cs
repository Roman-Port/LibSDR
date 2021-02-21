using RomanPort.LibSDR.Components.Decimators;
using RomanPort.LibSDR.Components.Filters;
using RomanPort.LibSDR.Components.Filters.Builders;
using RomanPort.LibSDR.Components.Filters.FIR;
using RomanPort.LibSDR.Components.Filters.IIR;
using RomanPort.LibSDR.Components.General;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Analog
{
    public delegate void StereoDetectedEventArgs(bool stereoDetected);

    public class StereoFmDecoder
    {
        public StereoFmDecoder()
        {
            stereoPilotFilter = new FloatIirFilter();
            deemphasisL = new DeemphasisProcessor();
            deemphasisR = new DeemphasisProcessor();
            stereoPilot = new Pll();
            stereoPilotFilter = new FloatIirFilter();

            //Configure
            DeemphasisTime = 75f; //Configured for America

            //Create stereo pilot
            stereoPilot = new Pll();
            stereoPilot.DefaultFrequency = STEREO_PILOT_FREQ;
            stereoPilot.Range = 20;
            stereoPilot.Bandwidth = 10;
            stereoPilot.Zeta = 0.707f;
            stereoPilot.PhaseAdjM = 0.0f;
            stereoPilot.PhaseAdjB = -1.75f;
            stereoPilot.LockTime = 0.5f;
            stereoPilot.LockThreshold = 1.0f;
        }

        private const int MIN_BC_AUDIO_FREQ = 20;
        private const int MAX_BC_AUDIO_FREQ = 16000;
        private const int STEREO_PILOT_FREQ = 19000;

        public bool StereoDetected
        {
            get => stereoDetected;
            private set
            {
                if (value == stereoDetected)
                    return;
                stereoDetected = value;
                OnStereoDetected?.Invoke(value);
            }
        }

        public float DeemphasisTime
        {
            get => deemphasisL.Time;
            set
            {
                deemphasisL.Time = value;
                deemphasisR.Time = value;
            }
        }

        public event StereoDetectedEventArgs OnStereoDetected;

        private DeemphasisProcessor deemphasisL;
        private DeemphasisProcessor deemphasisR;
        private FloatIirFilter stereoPilotFilter;
        private Pll stereoPilot;
        private bool stereoDetected;

        private FloatFirFilter channelAFilter;
        private FloatFirFilter channelBFilter;

        public float Configure(float sampleRate, int audioDecimationRate)
        {
            //Calculate the audio decimation rate
            float audioSampleRate = sampleRate / audioDecimationRate;
            var coefficients = new BandPassFilterBuilder(sampleRate, MIN_BC_AUDIO_FREQ, MAX_BC_AUDIO_FREQ)
                .SetAutomaticTapCount(STEREO_PILOT_FREQ - MAX_BC_AUDIO_FREQ, 20)
                .SetWindow(WindowType.BlackmanHarris7)
                .BuildFilter();
            channelAFilter = new FloatFirFilter(coefficients, audioDecimationRate);
            channelBFilter = new FloatFirFilter(coefficients, audioDecimationRate);

            //Configure stereo
            stereoPilot.SampleRate = sampleRate;
            stereoPilotFilter.Init(IirFilterType.BandPass, STEREO_PILOT_FREQ, sampleRate, 200);

            //Configure Deemphasis
            deemphasisL.SampleRate = audioSampleRate;
            deemphasisR.SampleRate = audioSampleRate;

            return audioSampleRate;
        }

        public unsafe int Demodulate(float* mpx, float* left, float* right, int count)
        {
            //Decimate and filter L+R
            int audioLength = channelAFilter.Process(mpx, left, count);

            //Demodulate L - R
            for (var i = 0; i < count; i++)
            {
                var pilot = stereoPilotFilter.Process(mpx[i]);
                stereoPilot.Process(pilot);
                right[i] = mpx[i] * Trig.Sin(stereoPilot.AdjustedPhase * 2.0f);
            }

            //Set
            StereoDetected = stereoPilot.IsLocked;

            //Switch depending on the state
            if(stereoPilot.IsLocked)
            {
                //Decimate and filter L-R
                channelBFilter.Process(right, count);

                //Recover L and R audio channels
                for (var i = 0; i < audioLength; i++)
                {
                    var a = left[i];
                    var b = 2f * right[i];
                    left[i] = (a + b);
                    right[i] = (a - b);
                }

                //Process deemp
                deemphasisL.Process(left, audioLength);
                deemphasisR.Process(right, audioLength);
            } else
            {
                //Process deemp
                deemphasisL.Process(left, audioLength);

                //Copy to the other channel
                Utils.Memcpy(right, left, audioLength * sizeof(float));
            }

            return audioLength;
        }
    }
}
