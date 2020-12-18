using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Util;
using RomanPort.LibSDR.Radio.Framework;
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

        //Config options
        public int BufferSize { get; private set; }
        public bool IsRealtime { get; private set; }

        //Public misc
        public SDRRadioState RadioState { get; private set; }

        //Internal buffers
        private Complex* iqBufferAPtr;
        private Complex* iqBufferBPtr;

        //Internal misc
        private IIQSource source;
        private Thread workerThread;
        private List<SDRRadioModule> modules = new List<SDRRadioModule>();
        private List<UnsafeBuffer> managedBuffers = new List<UnsafeBuffer>(); //Buffers from RequestBuffer

        //Public API

        /// <summary>
        /// Starts the radio and delays for a short time while the thread begins.
        /// </summary>
        public void StartRadio()
        {
            //If we're already in a running state, do nothing
            if (RadioState != SDRRadioState.STOPPED)
                return;

            //Set state
            RadioState = SDRRadioState.STARTING;

            //Launch worker thread
            workerThread = new Thread(RunWorker);
            workerThread.IsBackground = true;
            workerThread.Name = "LibSDR Radio Worker";
            workerThread.Start();

            //Wait
            while (RadioState == SDRRadioState.STARTING) ;
        }

        /// <summary>
        /// Stops the radio and delays for a short time while the thread stops.
        /// </summary>
        public void StopRadio()
        {
            //If we're not running, do nothing
            if (RadioState != SDRRadioState.RUNNING)
                return;

            //Request stop
            RadioState = SDRRadioState.STOPPING;

            //Wait
            while (RadioState == SDRRadioState.STOPPING) ;
        }

        //Internal

        /// <summary>
        /// Creates a new buffer of T, with the size of BufferSize
        /// </summary>
        /// <returns></returns>
        protected T* RequestBuffer<T>() where T : unmanaged
        {
            //Open
            UnsafeBuffer buffer = UnsafeBuffer.Create(BufferSize, sizeof(T));

            //Add
            managedBuffers.Add(buffer);

            //Return pointer
            return (T*)buffer;
        }

        /// <summary>
        /// The main loop, ran as a worker
        /// </summary>
        private void RunWorker()
        {
            while(RadioState == SDRRadioState.STARTING || RadioState == SDRRadioState.RUNNING)
            {
                //Handle queued worker commands
                HandleQueuedMessages();

                //Update state
                RadioState = SDRRadioState.RUNNING;

                //Read
                int read = source.Read(iqBufferAPtr, BufferSize);

                //Dispatch to modules
                foreach(var m in modules)
                {
                    //If this is a destructive module, clone the IQ data into another buffer
                    Complex* ptr = iqBufferAPtr;
                    if(m.IsDestructive)
                    {
                        Utils.Memcpy(iqBufferBPtr, iqBufferAPtr, read * sizeof(Complex));
                        ptr = iqBufferBPtr;
                    }

                    //Run
                    m.OnIncomingSamples(ptr, read);
                }
            }
        }
    }
}
