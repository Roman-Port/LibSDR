using RomanPort.LibSDR.Demodulators.Analog.Primitive;
using RomanPort.LibSDR.Extras.RDS;
using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Components.FFT.Processors;
using RomanPort.LibSDR.Framework.Extras.RDS;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Demodulators.Analog.Broadcast
{
    public delegate void StereoDetectedEventArgs(bool stereoDetected);

    public unsafe class WbFmDemodulator : IAudioDemodulator
    {
        private const int MIN_BC_AUDIO_FREQ = 20;
        private const int MAX_BC_AUDIO_FREQ = 16000;
        private const int STEREO_PILOT_FREQ = 19000;

        public WbFmDemodulator(int outputDecimationRate = 1)
        {
            //Set
            this.outputDecimationRate = outputDecimationRate;

            //Create various bits
            fm = new FmDemodulator();
            rdsDemodulator = new RdsDemodulator();
            stereoPilotFilter = new IirFilter();

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

        public bool StereoDetected { get; private set; }
        public event StereoDetectedEventArgs OnStereoDetected;

        //Misc
        private FmDemodulator fm;
        private int outputDecimationRate = 1;
        private IirFilter stereoPilotFilter;
        private Pll stereoPilot;
        private FirFilter channelAFilter;
        private FirFilter channelBFilter;
        private float deemphasisAlpha;
        private float deemphasisAvgL;
        private float deemphasisAvgR;
        private RdsDemodulator rdsDemodulator;
        private bool rdsEnabled;

        //Modules. These may be null and are created using the "EnableX" functions
        private FFTProcessorHalfFloat fft;
        private RDSClient rdsClient;

        //Buffers
        private UnsafeBuffer mpxBuffer;
        private float* mpx;

        public FFTProcessorHalfFloat EnableMpxFFT(int fftBinSize = 2048)
        {
            if (fft == null)
                fft = new FFTProcessorHalfFloat(fftBinSize);
            return fft;
        }

        public RDSClient UseRds()
        {
            if (rdsClient == null)
            {
                rdsClient = new RDSClient();
                rdsClient.SubscribeToRdsDemodulator(UseRdsDemodulator());
            }
            return rdsClient;
        }

        public RdsDemodulator UseRdsDemodulator()
        {
            rdsEnabled = true;
            return rdsDemodulator;
        }

        public void Configure(int bufferSize, float sampleRate)
        {
            //Caluclate
            int decimatedSampleRate = (int)(sampleRate / outputDecimationRate);

            //Create buffers
            mpxBuffer = UnsafeBuffer.Create(bufferSize, out mpx);

            //Configure stereo
            stereoPilot.SampleRate = sampleRate;
            stereoPilotFilter.Init(IirFilterType.BandPass, STEREO_PILOT_FREQ, sampleRate, 500);

            //Create filters
            var coefficients = FilterBuilder.MakeBandPassKernel(sampleRate, 250, MIN_BC_AUDIO_FREQ, MAX_BC_AUDIO_FREQ, WindowType.BlackmanHarris4);
            channelAFilter = new FirFilter(coefficients, outputDecimationRate);
            channelBFilter = new FirFilter(coefficients, outputDecimationRate);

            //Configure RDS
            rdsDemodulator.Configure(sampleRate);
            rdsDemodulator.CreateBuffers(bufferSize);

            //Configure Deemphasis
            deemphasisAlpha = (float)(1.0 - Math.Exp(-1.0 / (decimatedSampleRate * (50f * 1e-6f))));
            deemphasisAvgL = 0;
            deemphasisAvgR = 0;
        }

        public void Demodulate(Complex* iq, float* audio, int count)
        {
            //Demodulate FM into the MPX buffer
            fm.Demodulate(iq, mpx, count);

            //FFT
            fft?.AddSamples(mpx, count);

            //Process audio
            ProcessAudioMono(audio, count);

            //Process RDS
            if (rdsEnabled)
                rdsDemodulator.Process(mpx, count);
        }

        public void DemodulateStereo(Complex* iq, float* left, float* right, int count)
        {
            //Demodulate FM into the MPX buffer
            fm.Demodulate(iq, mpx, count);

            //FFT
            fft?.AddSamples(mpx, count);

            //Process audio
            ProcessAudioStereo(left, right, count);

            //Process RDS
            if (rdsEnabled)
                rdsDemodulator.Process(mpx, count);
        }

        public void DemodulateRDS(Complex* iq, int count)
        {
            //Demodulate FM into the MPX buffer
            fm.Demodulate(iq, mpx, count);

            //FFT
            fft?.AddSamples(mpx, count);

            //Process RDS
            if(rdsEnabled)
                rdsDemodulator.Process(mpx, count);
        }

        private void ProcessAudioMono(float* audio, int count)
        {
            //Decimate and filter L+R
            Utils.Memcpy(audio, mpx, count * sizeof(float));
            int audioLength = count / outputDecimationRate;
            channelAFilter.Process(audio, count);

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
        }

        private void ProcessAudioStereo(float* left, float* right, int count)
        {
            //Decimate and filter L+R
            Utils.Memcpy(left, mpx, count * sizeof(float));
            int audioLength = count / outputDecimationRate;
            channelAFilter.Process(left, count);

            //Demodulate L - R
            for (var i = 0; i < count; i++)
            {
                var pilot = stereoPilotFilter.Process(mpx[i]);
                stereoPilot.Process(pilot);
                right[i] = mpx[i] * Trig.Sin((float)(stereoPilot.AdjustedPhase * 2.0));
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
    }
}
