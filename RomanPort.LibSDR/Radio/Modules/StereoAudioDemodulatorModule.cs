using RomanPort.LibSDR.Demodulators;
using RomanPort.LibSDR.Components.Decimators;
using RomanPort.LibSDR.Components.Resamplers.Arbitrary;
using RomanPort.LibSDR.Radio.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using RomanPort.LibSDR.Components;

namespace RomanPort.LibSDR.Radio.Modules
{
    public unsafe delegate void StereoAudioDemodulatorModule_AudioAvailable(float* left, float* right, int count);
    public unsafe delegate void StereoAudioDemodulatorModule_OutputSampleRateChanged(float sampleRate);

    public unsafe class StereoAudioDemodulatorModule : SDRRadioModule
    {
        public StereoAudioDemodulatorModule(IAudioDemodulator demodulator, float bandwidth, float outputAudioRate)
        {
            OutputSampleRate = outputAudioRate;
            this.demodulator = demodulator;
            this.bandwidth = bandwidth;
        }

        public virtual float OutputSampleRate { get; private set; }
        public IAudioDemodulator Demodulator
        {
            get => demodulator;
            set
            {
                SendThreadedMessage(new SetDemodulatorCommand(value, this));
            }
        }
        public float Bandwidth
        {
            get => bandwidth;
            set
            {
                SendThreadedMessage(new SetBandwidthCommand(value, this));
            }
        }

        public event StereoAudioDemodulatorModule_AudioAvailable OnAudioAvailable;

        protected int bufferSize = -1;
        protected float inputSampleRate = -1;

        private Complex* iqBufferPtr;
        private float* audioBufferA;
        private float* audioBufferB;

        private ComplexDecimator iqDecimator;
        private int iqDecimationRate;
        private float iqDecimatedSampleRate;

        private IAudioDemodulator demodulator;
        private float bandwidth;

        private int audioDecimationRate;
        private ArbitraryFloatResampler audioResamplerA;
        private ArbitraryFloatResampler audioResamplerB;

        protected void ConfigureSaved()
        {
            if(bufferSize != -1)
                Configure(bufferSize, inputSampleRate);
        }

        public override void Configure(int bufferSize, float inputSampleRate)
        {
            //Set
            this.bufferSize = bufferSize;
            this.inputSampleRate = inputSampleRate;
            
            //Make buffers
            iqBufferPtr = RequestBuffer(bufferSize, iqBufferPtr);
            audioBufferA = RequestBuffer(bufferSize, audioBufferA);
            audioBufferB = RequestBuffer(bufferSize, audioBufferB);

            //Make decimator
            iqDecimationRate = DecimationUtil.CalculateDecimationRate(inputSampleRate, bandwidth, out float iqDecimatedSampleRate);
            iqDecimator = new ComplexDecimator(inputSampleRate, bandwidth, iqDecimationRate, 30, bandwidth * 0.05f);

            //Configure the demodulator
            audioDecimationRate = DecimationUtil.CalculateDecimationRate(iqDecimatedSampleRate, OutputSampleRate, out float decimatedAudioRate);
            demodulator.Configure(bufferSize, iqDecimatedSampleRate, audioDecimationRate);

            //Create resamplers for the decimated audio
            audioResamplerA = new ArbitraryFloatResampler(decimatedAudioRate, OutputSampleRate, bufferSize);
            audioResamplerB = new ArbitraryFloatResampler(decimatedAudioRate, OutputSampleRate, bufferSize);
        }

        public override void ProcessSamples(Complex* ptr, int read)
        {
            //Handle
            HandleQueuedMessages();
            
            //Decimate IQ
            read = iqDecimator.Process(ptr, iqBufferPtr, read);

            //Demodulate + decimate
            read = demodulator.DemodulateStereo(iqBufferPtr, audioBufferA, audioBufferB, read);

            //Add to resamplers
            audioResamplerA.Input(audioBufferA, read, 1);
            audioResamplerB.Input(audioBufferB, read, 1);

            //Output from resamplers
            do
            {
                //Process
                audioResamplerA.Output(audioBufferA, bufferSize, 1);
                read = audioResamplerB.Output(audioBufferB, bufferSize, 1);

                //Send
                DispatchOutput(audioBufferA, audioBufferB, read);
            } while (read == bufferSize);
        }

        protected virtual void DispatchOutput(float* audioA, float* audioB, int count)
        {
            OnAudioAvailable?.Invoke(audioA, audioB, count);
        }

        private class SetDemodulatorCommand : ISDRMessage
        {
            public SetDemodulatorCommand(IAudioDemodulator demodulator, StereoAudioDemodulatorModule module)
            {
                this.demodulator = demodulator;
                this.module = module;
            }

            private IAudioDemodulator demodulator;
            private StereoAudioDemodulatorModule module;
            
            public void Process()
            {
                module.demodulator = demodulator;
                module.ConfigureSaved();
            }
        }

        private class SetBandwidthCommand : ISDRMessage
        {
            public SetBandwidthCommand(float bandwidth, StereoAudioDemodulatorModule module)
            {
                this.bandwidth = bandwidth;
                this.module = module;
            }

            private float bandwidth;
            private StereoAudioDemodulatorModule module;

            public void Process()
            {
                module.bandwidth = bandwidth;
                module.ConfigureSaved();
            }
        }
    }
}
