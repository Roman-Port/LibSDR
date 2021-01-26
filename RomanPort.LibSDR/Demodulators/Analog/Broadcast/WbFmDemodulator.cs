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
            fm = new FmDemodulator();
            rdsDemodulator = new RDSDecoder();
            stereoPilotFilter = new FloatIirFilter();

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

        public bool StereoDetected { get; private set; }
        public event StereoDetectedEventArgs OnStereoDetected;
        public float MpxSampleRate { get => sampleRate; }
        public event MpxDataEmitted OnMpxSamplesEmitted;

        //Rate of audio, also the output
        private int audioDecimationRate;
        private float audioSampleRate;
        private FloatFirFilter channelAFilter;
        private FloatFirFilter channelBFilter;

        //Misc
        private SnrCalculator snr;
        private FmDemodulator fm;
        private FloatIirFilter stereoPilotFilter;
        private Pll stereoPilot;
        
        private float deemphasisAlpha;
        private float deemphasisAvgL;
        private float deemphasisAvgR;
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
            deemphasisAlpha = 1.0f - MathF.Exp(-1.0f / (audioSampleRate * (50f * 1e-6f)));
            deemphasisAvgL = 0;
            deemphasisAvgR = 0;

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

            //Process audio
            if (!audioEnabled)
                return count;
            if (audioStereoEnabled)
                return ProcessAudioStereo(left, right, count);
            else
                return ProcessAudioMono(left, count);
        }

        private int ProcessAudioMono(float* audio, int count)
        {
            //Decimate and filter L+R
            Utils.Memcpy(audio, mpx, count * sizeof(float));
            int audioLength = channelAFilter.Process(audio, count);

            //Demodulate L - R
            for (var i = 0; i < count; i++)
            {
                var pilot = stereoPilotFilter.Process(mpx[i]);
                stereoPilot.Process(pilot);
            }

            //Send events if mono/stereo status changed
            if (StereoDetected != stereoPilot.IsLocked)
                OnStereoDetected?.Invoke(stereoPilot.IsLocked);
            StereoDetected = stereoPilot.IsLocked;

            //Do preemp
            ProcessDeemphasisMono(audio, audioLength);

            return audioLength;
        }

        private int ProcessAudioStereo(float* left, float* right, int count)
        {
            //Decimate and filter L+R
            Utils.Memcpy(left, mpx, count * sizeof(float));
            int audioLength = channelAFilter.Process(left, count);

            //Demodulate L - R
            for (var i = 0; i < count; i++)
            {
                var pilot = stereoPilotFilter.Process(mpx[i]);
                stereoPilot.Process(pilot);
                right[i] = mpx[i] * Trig.Sin(stereoPilot.AdjustedPhase * 2.0f);
            }

            //Send events if mono/stereo status changed
            if (StereoDetected != stereoPilot.IsLocked)
                OnStereoDetected?.Invoke(stereoPilot.IsLocked);
            StereoDetected = stereoPilot.IsLocked;

            //Switch depending on the mode
            if(StereoDetected)
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
                ProcessDeemphasisStereo(left, right, audioLength);
            } else
            {
                //Process deemp
                ProcessDeemphasisMono(left, audioLength);

                //Copy to the other channel
                Utils.Memcpy(right, left, audioLength * sizeof(float));
            }

            return audioLength;
        }

        private void ProcessDeemphasisMono(float* audio, int audioLength)
        {
            for (var i = 0; i < audioLength; i++)
            {
                deemphasisAvgL += deemphasisAlpha * (audio[i] - deemphasisAvgL);
                audio[i] = deemphasisAvgL;
            }
        }

        private void ProcessDeemphasisStereo(float* left, float* right, int audioLength)
        {
            for (var i = 0; i < audioLength; i++)
            {
                //Left
                deemphasisAvgL += deemphasisAlpha * (left[i] - deemphasisAvgL);
                left[i] = deemphasisAvgL;

                //Right
                deemphasisAvgR += deemphasisAlpha * (right[i] - deemphasisAvgR);
                right[i] = deemphasisAvgR;
            }
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
