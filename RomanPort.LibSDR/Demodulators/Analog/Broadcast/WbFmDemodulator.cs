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
using RomanPort.LibSDR.Components.Filters.FIR;
using RomanPort.LibSDR.Components.Analog;
using RomanPort.LibSDR.Components.Filters.IIR;
using RomanPort.LibSDR.Components.General;
using RomanPort.LibSDR.Components.IO.WAV;
using RomanPort.LibSDR.Components.Filters.FIR.Real;
using RomanPort.LibSDR.Components.Filters.FIR.ComplexFilter;

namespace RomanPort.LibSDR.Demodulators.Analog.Broadcast
{
    public delegate void StereoDetectedEventArgs(bool stereoDetected);
    public unsafe delegate void MpxDataEmitted(float* ptr, int count);
    public unsafe delegate void MpxSampleRateChangedEventArgs(float sampleRate);

    public unsafe class WbFmDemodulator : IAudioDemodulator, IDisposable
    {
        private const int MIN_AUDIO_FREQ = 20;
        private const int MAX_AUDIO_FREQ = 17000;
        private const int STEREO_PILOT_FREQ = 19000;
        private const int STEREO_PILOT_MAX_ERROR = 5;

        public WbFmDemodulator(BackgroundWorker worker = null)
        {
            //Create parts
            fm = new FmBasebandDemodulator();
            deemphasisL = new DeemphasisProcessor();
            deemphasisR = new DeemphasisProcessor();
            rdsDemodulator = new RDSDecoder();

            //Apply defaults
            this.worker = worker;
            DeemphasisTime = 75; //Configured for America
            //DeemphasisTime = 50; //Configured for rest of the world
        }

        //Public accessors
        public bool StereoEnabled { get; set; } = true;
        public bool StereoDetected { get => stereoPilotPll.IsLocked; }
        public float DeemphasisTime
        {
            get => deemphasisL.Time;
            set
            {
                deemphasisL.Time = value;
                deemphasisR.Time = value;
            }
        }
        public float MpxSampleRate { get => sampleRate; }
        public RDSDecoder Rds { get => rdsDemodulator; }

        //Public events
        public event StereoDetectedEventArgs OnStereoDetected;
        public event MpxDataEmitted OnMpxSamplesEmitted;
        public event MpxSampleRateChangedEventArgs OnMpxSampleRateChanged;

        //Private bits
        private BackgroundWorker worker;
        private FmBasebandDemodulator fm;
        private DeemphasisProcessor deemphasisL;
        private DeemphasisProcessor deemphasisR;
        private IRealFirFilter channelAFilter;
        private IRealFirFilter channelBFilter;
        private IRealFirFilter stereoMpxFilter;
        private PLL stereoPilotPll;
        private IComplexFirFilter stereoPilotFilter;
        private RDSDecoder rdsDemodulator;

        //Private vars
        private float sampleRate;
        private float decimatedSampleRate;
        private float audioSampleRate;
        private int audioDecimationRate = 1;
        private bool wasStereoDetected;

        //Buffers
        private UnsafeBuffer mpxBuffer;
        private float* mpxBufferPtr;
        private UnsafeBuffer pilotBuffer;
        private Complex* pilotBufferPtr;

        public float Configure(int bufferSize, float sampleRate, float targetOutputRate)
        {
            //Set
            this.sampleRate = sampleRate;
            OnMpxSampleRateChanged?.Invoke(sampleRate);

            //Configure base FM
            fm.Configure(bufferSize, sampleRate);            

            //Create buffer
            if (mpxBuffer == null || mpxBuffer.Length != bufferSize)
            {
                mpxBuffer?.Dispose();
                mpxBuffer = UnsafeBuffer.Create(bufferSize, out mpxBufferPtr);
            }
            if (pilotBuffer == null || pilotBuffer.Length != bufferSize)
            {
                pilotBuffer?.Dispose();
                pilotBuffer = UnsafeBuffer.Create(bufferSize, out pilotBufferPtr);
            }

            //Configure RDS, as it is pre decimation
            rdsDemodulator.Configure(sampleRate);

            //Create MPX filter. This will filter and decimate the data required to recover stereo
            /*var stereoMpxFilterParams = new LowPassFilterBuilder(sampleRate, 59000)
                .SetAutomaticTapCount(4000, 60)
                .SetWindow();
            stereoMpxFilter = new FloatFirFilter(stereoMpxFilterParams, stereoMpxFilterParams.GetDecimation(out decimatedSampleRate));*/
            decimatedSampleRate = sampleRate; //skip for now...this seems to cause some problems

            //Create audio filters
            var audioFilterBuilder = new BandPassFilterBuilder(decimatedSampleRate, MIN_AUDIO_FREQ, MAX_AUDIO_FREQ)
                .SetAutomaticTapCount(1500, 60)
                .SetWindow(WindowType.BlackmanHarris7);
            audioDecimationRate = audioFilterBuilder.GetDecimation(out audioSampleRate);
            channelAFilter = RealFirFilter.CreateFirFilter(audioFilterBuilder, audioDecimationRate);
            channelBFilter = RealFirFilter.CreateFirFilter(audioFilterBuilder, audioDecimationRate);

            //Create stereo pilot filter
            var stereoPilotFilterParamsComplex = new BandPassFilterBuilder(decimatedSampleRate, STEREO_PILOT_FREQ - 1000, STEREO_PILOT_FREQ + 1000)
               .SetAutomaticTapCount(1500, 60)
               .SetWindow();
            stereoPilotFilter = ComplexFirFilter.CreateFirFilter(stereoPilotFilterParamsComplex);

            //Create PLL for the stereo pilot
            stereoPilotPll = new PLL(decimatedSampleRate, 0.2f, 0.0001f, STEREO_PILOT_FREQ + STEREO_PILOT_MAX_ERROR, STEREO_PILOT_FREQ - STEREO_PILOT_MAX_ERROR);

            //Configure Deemphasis
            deemphasisL.SampleRate = audioSampleRate;
            deemphasisR.SampleRate = audioSampleRate;

            return audioSampleRate;
        }

        public int Demodulate(Complex* iq, float* audio, int count)
        {
            return DemodulateStereo(iq, audio, null, count);
        }

        public int DemodulateStereo(Complex* iq, float* left, float* right, int count)
        {
            //Demodulate FM into the MPX buffer
            fm.Demodulate(iq, mpxBufferPtr, count);

            //Emit MPX
            OnMpxSamplesEmitted?.Invoke(mpxBufferPtr, count);

            //Process RDS
            rdsDemodulator.Process(mpxBufferPtr, count);

            //Decimate and filter to what we need to recover stereo audio. This is what we'll use for the remainder of this
            //count = stereoMpxFilter.Process(mpxBufferPtr, count);

            //Decimate and filter L+R
            int audioLength = channelAFilter.Process(mpxBufferPtr, left, count);

            //Filter the stereo pilot
            for (int i = 0; i < count; i++)
                pilotBufferPtr[i] = mpxBufferPtr[i];
            stereoPilotFilter.Process(pilotBufferPtr, count);

            //Demodulate L-R using the stereo pilot
            Complex pilotRef;
            for (var i = 0; i < count; i++)
            {
                //PLL pilot
                stereoPilotPll.Process(pilotBufferPtr[i]);
                pilotRef = stereoPilotPll.Ref;

                //Update
                mpxBufferPtr[i] *= (pilotRef * pilotRef).Real;
            }

            //Send events if we've now detected stereo
            if(wasStereoDetected != StereoDetected)
            {
                wasStereoDetected = StereoDetected;
                OnStereoDetected?.Invoke(wasStereoDetected);
            }

            //If we should use stereo, process it entirely
            if(StereoDetected && StereoEnabled && right != null)
            {
                //If we've detected stereo, filter and decimate L-R
                channelBFilter.Process(mpxBufferPtr, right, count);

                //Recover L and R audio channels
                float add;
                float sub;
                for (var i = 0; i < audioLength; i++)
                {
                    add = left[i];
                    sub = 2f * right[i];
                    left[i] = (add + sub);
                    right[i] = (add - sub);
                }
            } else if (right != null)
            {
                //Just copy from the left (L+R) buffer into the right buffer
                Utils.Memcpy(right, left, audioLength * sizeof(float));
            }

            //Process deemp
            deemphasisL.Process(left, audioLength);
            if(right != null)
                deemphasisR.Process(right, audioLength);

            return audioLength;
        }

        public void Dispose()
        {
            mpxBuffer.Dispose();
        }
    }
}
