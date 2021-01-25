using RomanPort.LibSDR.Demodulators;
using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Util;
using RomanPort.LibSDR.Radio.Framework;
using RomanPort.LibSDR.Radio.Modules;
using RomanPort.LibSDR.Sources;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RomanPort.LibSDR.Radio
{
    public unsafe class SDRRadio : SDRMessageReceiver
    {
        public SDRRadio(SDRRadioConfig cfg)
        {
            //Set from config
            BufferSize = cfg.BufferSize;
            IsRealtime = cfg.IsRealtime;
        }

        //Public facing
        public int BufferSize { get; private set; }
        public bool IsRealtime { get; private set; }
        public SDRRadioState RadioState { get; private set; }
        public float SampleRate { get; private set; }

        //Internal misc
        private ISource source;
        private List<SDRRadioModule> modules = new List<SDRRadioModule>();

        //Public API

        /// <summary>
        /// Starts the radio and delays for a short time while the thread begins.
        /// </summary>
        public bool StartRadio(ISource source)
        {
            //If we're already in a running state, do nothing
            if (RadioState != SDRRadioState.STOPPED)
                return false;

            //Set state
            RadioState = SDRRadioState.RUNNING;

            //Bind events
            source.OnSampleRateChanged += OnSampleRateChanged;
            source.OnSamplesAvailable += OnSamplesAvailable;

            //Configure
            this.source = source;
            source.Open(BufferSize);
            source.BeginStreaming();

            return true;
        }

        /// <summary>
        /// Stops the radio and delays for a short time while the thread stops.
        /// </summary>
        public bool StopRadio()
        {
            //If we're not running, do nothing
            if (RadioState != SDRRadioState.RUNNING)
                return false;

            //Set state
            RadioState = SDRRadioState.STOPPING;

            //Unbind events
            source.OnSampleRateChanged -= OnSampleRateChanged;
            source.OnSamplesAvailable -= OnSamplesAvailable;

            //Request stop
            source.EndStreaming();

            return true;
        }

        /// <summary>
        /// Adds a module to the radio for processing.
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public SDRRadioModule AddModule(SDRRadioModule module)
        {
            SendThreadedMessage(new CommandAddModule(this, module));
            return module;
        }

        /// <summary>
        /// Adds a demodulator to this, with the bandwidth specified. Output sample rate will match what is specified
        /// </summary>
        /// <returns></returns>
        public StereoAudioDemodulatorModule AddAudioDemodulator(IAudioDemodulator demodulator, float bandwidth, float outputSampleRate)
        {
            StereoAudioDemodulatorModule module = new StereoAudioDemodulatorModule(demodulator, bandwidth, outputSampleRate);
            AddModule(module);
            return module;
        }

        //Internal

        private void OnSampleRateChanged(float sampleRate)
        {
            //Set
            SampleRate = sampleRate;

            //Configure all modules
            foreach (var m in modules)
                m.Configure(BufferSize, sampleRate);
        }

        private void OnSamplesAvailable(Complex* samples, int count)
        {
            //Process threaded commands
            HandleQueuedMessages();

            //Send to modules
            foreach (var m in modules)
                m.ProcessSamples(samples, count);
        }

        //Multithreaded commands

        private class CommandAddModule : ISDRMessage
        {
            public CommandAddModule(SDRRadio radio, SDRRadioModule module)
            {
                this.radio = radio;
                this.module = module;
            }

            private SDRRadio radio;
            private SDRRadioModule module;
            
            public void Process()
            {
                module.Configure(radio.BufferSize, radio.SampleRate);
                radio.modules.Add(module);
            }
        }
    }
}
