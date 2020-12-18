﻿using RomanPort.LibSDR.Extras.RDS;
using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Components.FFT.Processors;
using RomanPort.LibSDR.Framework.Extras;
using RomanPort.LibSDR.Framework.Extras.RDS;
using RomanPort.LibSDR.Framework.Resamplers.Decimators;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RomanPort.LibSDR.Demodulators
{
    public delegate void StereoDetectedEventArgs(bool stereoDetected);
    public delegate void RdsFrameEventArgs(ushort frameA, ushort frameB, ushort frameC, ushort frameD, WbFmDemodulator demodulator);

    public unsafe class WbFmDemodulator : IDemodulator, IRDSFrameReceiver
    {
        /* Public */
        public bool StereoDetected { get; private set; }
        public bool StereoEnabled { get; set; } = true;
        public float ifGain = FmDemodulator.FM_GAIN;

        /* Events */
        public event StereoDetectedEventArgs OnStereoDetected;
        public event RdsFrameEventArgs OnRdsFrame;

        //Modules. These may be null and are created using the "EnableX" functions
        private FFTProcessorHalfFloat fft;
        private RDSClient rdsClient;

        /* Private */
        private float sampleRate;
        private float decimatedSampleRate;
        private int outputDecimationRate; //Decimation rate of the output audio
        private bool fast;

        private RdsDemodulator rdsDemodulator;
        private bool rdsEnabled;
        
        private Complex _iqState;
        private Pll* _pll;
        private UnsafeBuffer _pllBuffer;
        private IirFilter* _pilotFilter;
        private UnsafeBuffer _pilotFilterBuffer;
        private UnsafeBuffer _channelABuffer;
        private UnsafeBuffer _channelBBuffer;
        private float* _channelAPtr;
        private float* _channelBPtr;
        private FirFilter _channelAFilter;
        private FirFilter _channelBFilter;
        private SdrFloatDecimator _channelADecimator;
        private SdrFloatDecimator _channelBDecimator;

        private float _deemphasisAlpha;
        private float _deemphasisAvgL;
        private float _deemphasisAvgR;

        private UnsafeBuffer _mpxBuffer;
        private float* _mpxBufferPtr;

        private UnsafeBuffer audioTempLBuffer;
        private float* audioTempLBufferPtr;
        private UnsafeBuffer audioTempRBuffer;
        private float* audioTempRBufferPtr;

        private const int MIN_BC_AUDIO_FREQ = 20;
        private const int MAX_BC_AUDIO_FREQ = 16000;
        private const int STEREO_PILOT_FREQ = 19000;
        private const float AUDIO_GAIN = 1f;

        public WbFmDemodulator(int outputDecimationRate = 1, bool fast = false)
        {
            this.outputDecimationRate = outputDecimationRate;
            this.fast = fast;

            //Make PLL
            _pllBuffer = UnsafeBuffer.Create(sizeof(Pll));
            _pll = (Pll*)_pllBuffer;
            _pll->DefaultFrequency = STEREO_PILOT_FREQ;
            _pll->Range = 20;
            _pll->Bandwidth = 10;
            _pll->Zeta = 0.707f;
            _pll->PhaseAdjM = 0.0f;
            _pll->PhaseAdjB = -1.75f;
            _pll->LockTime = 0.5f;
            _pll->LockThreshold = 1.0f;

            //Make RDS
            rdsDemodulator = new RdsDemodulator(this);
        }

        public FFTProcessorHalfFloat EnableMpxFFT(int fftBinSize = 2048)
        {
            if (fft == null)
                fft = new FFTProcessorHalfFloat(fftBinSize);
            return fft;
        }

        public RDSClient UseRds()
        {
            if(rdsClient == null)
            {
                rdsClient = new RDSClient();
                rdsClient.SubscribeFmDemodulator(this);
            }
            return rdsClient;
        }

        public RdsDemodulator UseRdsDemodulator()
        {
            rdsEnabled = true;
            return rdsDemodulator;
        }

        public override unsafe int DemodulateStereo(Complex* iq, float* audioL, float* audioR, int count)
        {
            return DemodulateBase(iq, audioL, audioR, count, false);
        }

        public override unsafe int DemodulateMono(Complex* iq, float* audio, int count)
        {
            return DemodulateBase(iq, audio, audio, count, true);
        }

        /// <summary>
        /// Demodulates the bare minimum to run RDS. Does not produce audio output.
        /// </summary>
        /// <param name="iq"></param>
        /// <param name="count"></param>
        public unsafe void DemodulateRDS(Complex* iq, int count)
        {
            //Demodulate MPX
            DemodulateIqSamples(iq, _mpxBufferPtr, count);

            //FFT
            fft?.AddSamples(_mpxBufferPtr, count);

            //Process RDS
            if (rdsEnabled)
                rdsDemodulator.Process(_mpxBufferPtr, count);
        }

        private unsafe int DemodulateBase(Complex* iq, float* audioL, float* audioR, int count, bool forceMono)
        {
            //Demodulate MPX
            DemodulateIqSamples(iq, _mpxBufferPtr, count);

            //FFT
            fft?.AddSamples(_mpxBufferPtr, count);

            //Process audio
            ProcessMPXAudio(_mpxBufferPtr, audioL, audioR, count, forceMono);

            //Process RDS
            if(rdsEnabled)
                rdsDemodulator.Process(_mpxBufferPtr, count);

            return count / outputDecimationRate;
        }

        public override void OnAttached(int bufferLen)
        {
            //Create L+R buffer
            _channelABuffer = UnsafeBuffer.Create(bufferLen, sizeof(float));
            _channelAPtr = (float*)_channelABuffer;

            //Create L-R buffer
            _channelBBuffer = UnsafeBuffer.Create(bufferLen, sizeof(float));
            _channelBPtr = (float*)_channelBBuffer;

            //Make raw audio buffer. This serves as a holder for audio as it's being demodulated
            _mpxBuffer = UnsafeBuffer.Create(bufferLen * 2, sizeof(float));
            _mpxBufferPtr = (float*)_mpxBuffer;

            //Open RDS buffers
            rdsDemodulator.CreateBuffers(bufferLen);

            //Open audio buffers
            audioTempLBuffer = UnsafeBuffer.Create(bufferLen, sizeof(float));
            audioTempLBufferPtr = (float*)audioTempLBuffer;
            audioTempRBuffer = UnsafeBuffer.Create(bufferLen, sizeof(float));
            audioTempRBufferPtr = (float*)audioTempRBuffer;
        }

        public override float OnInputSampleRateChanged(float sampleRate)
        {
            //Update
            this.sampleRate = sampleRate;
            decimatedSampleRate = sampleRate / outputDecimationRate;

            //Configure PLL
            _pll->SampleRate = sampleRate;

            //Make pilot filter
            _pilotFilterBuffer = UnsafeBuffer.Create(sizeof(IirFilter));
            _pilotFilter = (IirFilter*)_pilotFilterBuffer;
            _pilotFilter->Init(IirFilterType.BandPass, STEREO_PILOT_FREQ, sampleRate, fast ? 200 : 500);

            //Create filters
            var coefficients = FilterBuilder.MakeBandPassKernel(sampleRate, fast ? 100 : 250, MIN_BC_AUDIO_FREQ, MAX_BC_AUDIO_FREQ, WindowType.BlackmanHarris4);
            _channelAFilter = new FirFilter(coefficients, outputDecimationRate);
            _channelBFilter = new FirFilter(coefficients, outputDecimationRate);           

            //Configure Deemphasis
            _deemphasisAlpha = (float)(1.0 - Math.Exp(-1.0 / (decimatedSampleRate * (50f * 1e-6f))));
            _deemphasisAvgL = 0;
            _deemphasisAvgR = 0;

            //Configure RDS
            rdsDemodulator.Configure(sampleRate);

            return decimatedSampleRate;
        }

        private void DemodulateIqSamples(Complex* iq, float* audio, int length)
        {
            for (var i = 0; i < length; i++)
            {
                //Polar discriminator
                var f = iq[i] * _iqState.Conjugate();

                //Limiting
                var m = f.Modulus();
                if (m > 0.0f)
                {
                    f /= m;
                }

                //Angle estimate
                var a = f.Argument();

                //Scale
                a = float.IsNaN(a) ? 0.0f : a * ifGain;// * 0.5f;// * 1E-05f;
                audio[i] = a;

                _iqState = iq[i];
            }
        }

        private void ProcessMPXAudio(float* baseBand, float* audioL, float* audioR, int length, bool forceMono)
        {
            //Decimate and filter L+R
            Utils.Memcpy(_channelAPtr, baseBand, length * sizeof(float));
            int audioLength = length / outputDecimationRate;
            _channelAFilter.Process(_channelAPtr, length);

            //Demodulate L - R
            for (var i = 0; i < length; i++)
            {
                var pilot = _pilotFilter->Process(baseBand[i]);
                _pll->Process(pilot);
                _channelBPtr[i] = baseBand[i] * Trig.Sin((float)(_pll->AdjustedPhase * 2.0));
            }

            //Send events if mono/stereo status changed
            if (StereoDetected != _pll->IsLocked)
                OnStereoDetected?.Invoke(_pll->IsLocked);
            StereoDetected = _pll->IsLocked;

            //Handle mono-only audio if the stereo pilot isn't detected
            if (!_pll->IsLocked || !StereoEnabled || forceMono)
            {
                //Process mono deemphasis and gain
                for (var i = 0; i < audioLength; i++)
                {
                    _deemphasisAvgL += _deemphasisAlpha * (_channelAPtr[i] - _deemphasisAvgL);
                    _channelAPtr[i] = _deemphasisAvgL * AUDIO_GAIN;
                }

                //Fill output buffer with mono
                Utils.Memcpy(audioL, _channelAPtr, audioLength * sizeof(float));
                if(!forceMono)
                    Utils.Memcpy(audioR, _channelAPtr, audioLength * sizeof(float));
                return;
            }

            //Decimate and filter L-R
            _channelBFilter.Process(_channelBPtr, length);

            //Recover L and R audio channels
            for (var i = 0; i < audioLength; i++)
            {
                var a = _channelAPtr[i];
                var b = 2f * _channelBPtr[i];
                audioL[i] = (a + b) * AUDIO_GAIN;
                audioR[i] = (a - b) * AUDIO_GAIN;
            }

            //Process deemphasis
            for (var i = 0; i < audioLength; i++)
            {
                _deemphasisAvgL += _deemphasisAlpha * (audioL[i] - _deemphasisAvgL);
                audioL[i] = _deemphasisAvgL;

                _deemphasisAvgR += _deemphasisAlpha * (audioR[i] - _deemphasisAvgR);
                audioR[i] = _deemphasisAvgR;
            }
        }

        public void OnRDSFrameReceived(ushort a, ushort b, ushort c, ushort d)
        {
            OnRdsFrame?.Invoke(a, b, c, d, this);
        }

        public override void Dispose()
        {
            _pllBuffer.Dispose();
            _pilotFilterBuffer.Dispose();
            _channelABuffer.Dispose();
            _channelBBuffer.Dispose();
            _channelAFilter.Dispose();
            _channelBFilter.Dispose();
            _mpxBuffer.Dispose();
            rdsDemodulator?.Dispose();
            audioTempLBuffer.Dispose();
            audioTempRBuffer.Dispose();
        }

        public override float OnOutputDecimationChanged(int decimationRate)
        {
            this.outputDecimationRate = decimationRate;
            decimatedSampleRate = sampleRate / outputDecimationRate;
            return decimatedSampleRate;
        }
    }
}
