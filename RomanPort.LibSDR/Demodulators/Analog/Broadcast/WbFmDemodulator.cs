using RomanPort.LibSDR.Components.Digital.RDS;
using RomanPort.LibSDR.Components.Filters;
using System;
using System.Collections.Generic;
using System.Text;
using RomanPort.LibSDR.Components.FFT.Generators;
using RomanPort.LibSDR.Components.Filters.Builders;
using RomanPort.LibSDR.Components.Analog.Primitive;
using RomanPort.LibSDR.Components.Decimators;
using RomanPort.LibSDR.Components;
using RomanPort.LibSDR.Components.Misc;
using RomanPort.LibSDR.Components.Filters.IIR;
using RomanPort.LibSDR.Components.General;
using RomanPort.LibSDR.Components.Filters.FIR;
using RomanPort.LibSDR.Components.Analog;

namespace RomanPort.LibSDR.Demodulators.Analog.Broadcast
{
    public delegate void StereoDetectedEventArgs(bool stereoDetected);
    public unsafe delegate void MpxDataEmitted(float* ptr, int count);

    public unsafe class WbFmDemodulator : IAudioDemodulator
    {
        private const int MIN_BC_AUDIO_FREQ = 20;
        private const int MAX_BC_AUDIO_FREQ = 16000;
        private const int STEREO_PILOT_FREQ = 19000;

        public WbFmDemodulator()
        {
            //Create various bits
            snr = new SnrCalculator();
            fm = new FmBasebandDemodulator();
            rdsDemodulator = new RDSDecoder();
            stereoPilotFilter = new FloatIirFilter();
            deemphasisL = new DeemphasisProcessor();
            deemphasisR = new DeemphasisProcessor();
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

        private const int MIN_MPX_RATE = (58000 * 2) + MPX_TRANSITION_WIDTH; //Includes all the way up to RDS
        private const int MPX_TRANSITION_WIDTH = 8000;

        public bool StereoDetected { get => stereoPilot.IsLocked; }
        public event StereoDetectedEventArgs OnStereoDetected;
        public float MpxSampleRate { get => sampleRate; }
        public event MpxDataEmitted OnMpxSamplesEmitted;
        public float DeemphasisTime
        {
            get => deemphasisTime;
            set
            {
                deemphasisTime = value;
                deemphasisL.Time = value;
                deemphasisR.Time = value;
            }
        }

        //Rate of audio, also the output
        private int audioDecimationRate;
        private float audioSampleRate;
        private FloatFirFilter channelAFilter;
        private FloatFirFilter channelBFilter;

        //Misc
        private SnrCalculator snr;
        private FmBasebandDemodulator fm;
        private FloatIirFilter stereoPilotFilter;
        private Pll stereoPilot;
        private DeemphasisProcessor deemphasisL;
        private DeemphasisProcessor deemphasisR;
        private float deemphasisTime = 75;
        private RDSDecoder rdsDemodulator;
        private bool rdsEnabled;
        private float sampleRate;

        //Buffers
        private UnsafeBuffer mpxBuffer;
        private float* mpx;

        public FFTGenerator EnableMpxFFT(int fftBinSize = 2048)
        {
            var fft = new FFTGenerator(fftBinSize, true);
            OnMpxSamplesEmitted += (float* ptr, int count) => fft.AddSamples(ptr, count);
            return fft;
        }

        public RDSDecoder UseRds()
        {
            rdsEnabled = true;
            return rdsDemodulator;
        }

        public float Configure(int bufferSize, float sampleRate, float targetOutputRate)
        {
            //Set
            this.sampleRate = sampleRate;
            
            //Configure base FM
            fm.Configure(bufferSize, sampleRate);

            //Calculate the audio decimation rate
            audioDecimationRate = DecimationUtil.CalculateDecimationRate(sampleRate, targetOutputRate, out audioSampleRate);
            var coefficients = new BandPassFilterBuilder(sampleRate, MIN_BC_AUDIO_FREQ, MAX_BC_AUDIO_FREQ)
                .SetAutomaticTapCount(STEREO_PILOT_FREQ - MAX_BC_AUDIO_FREQ, 40)
                .SetWindow(WindowType.Hamming)
                .BuildFilter();
            channelAFilter = new FloatFirFilter(coefficients, audioDecimationRate);
            channelBFilter = new FloatFirFilter(coefficients, audioDecimationRate);

            //Create buffer
            if (mpxBuffer == null || mpxBuffer.Length != bufferSize)
            {
                mpxBuffer?.Dispose();
                mpxBuffer = UnsafeBuffer.Create(bufferSize, out mpx);
            }

            //Configure stereo
            stereoPilot.SampleRate = sampleRate;
            stereoPilotFilter.Init(IirFilterType.BandPass, STEREO_PILOT_FREQ, sampleRate, 200);

            //Configure RDS
            rdsDemodulator.Configure(sampleRate);

            //Configure Deemphasis
            deemphasisL.SampleRate = audioSampleRate;
            deemphasisR.SampleRate = audioSampleRate;

            return audioSampleRate;
        }

        public int Demodulate(Complex* iq, float* audio, int count)
        {
            return DemodulateBase(iq, audio, null, count, true, false);
        }

        public int DemodulateStereo(Complex* iq, float* left, float* right, int count)
        {
            return DemodulateBase(iq, left, right, count, true, true);
        }

        public void DemodulateRDS(Complex* iq, int count)
        {
            DemodulateBase(iq, null, null, count, false, false);
        }

        private int DemodulateBase(Complex* iq, float* left, float* right, int count, bool audioEnabled, bool audioStereoEnabled)
        {
            //Demodulate FM into the MPX buffer
            fm.Demodulate(iq, mpx, count);

            //Emit MPX
            OnMpxSamplesEmitted?.Invoke(mpx, count);

            //Add MPX to SNR calculator
            snr.AddSamples(mpx, count);

            //Process RDS
            if (rdsEnabled)
                rdsDemodulator.Process(mpx, count);

            //If audio is disabled, abort now
            if (!audioEnabled)
                return count;

            //Decimate and filter L+R
            int audioLength = channelAFilter.Process(mpx, left, count);

            //Demodulate L - R
            bool hadStereo = StereoDetected;
            for (var i = 0; i < count; i++)
            {
                var pilot = stereoPilotFilter.Process(mpx[i]);
                stereoPilot.Process(pilot);
                if(audioStereoEnabled)
                    right[i] = mpx[i] * Trig.Sin(stereoPilot.AdjustedPhase * 2.0f);
            }

            //Send events if mono/stereo status changed
            if (StereoDetected != hadStereo)
                OnStereoDetected?.Invoke(StereoDetected);

            //Switch depending on the mode
            if (StereoDetected && audioStereoEnabled) //Stereo detected and stereo requested
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
            }
            else if (audioStereoEnabled) //No stereo enabled, but stereo is requested
            {
                //Process deemp
                deemphasisL.Process(left, audioLength);

                //Copy to the other channel
                Utils.Memcpy(right, left, audioLength * sizeof(float));
            }
            else //Stereo not enabled
            {
                //Process deemp
                deemphasisL.Process(left, audioLength);
            }

            return audioLength;
        }

        public SnrReading ReadAverageSnr()
        {
            return snr.CalculateAverageSnr();
        }

        public SnrReading ReadInstantSnr()
        {
            return snr.CalculateInstantSnr();
        }
    }
}
