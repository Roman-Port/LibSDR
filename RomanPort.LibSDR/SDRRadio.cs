using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Extras;
using RomanPort.LibSDR.Framework.FFT;
using RomanPort.LibSDR.Framework.Radio;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RomanPort.LibSDR
{
    public unsafe class SDRRadio
    {
        public event SDRRadioIqSamplesEventArgs OnIqSamplesAvailable;
        public event SDRRadioAudioSamplesEventArgs OnAudioSamplesAvailable;
        public event SDRRadioOpenedEventArgs OnRadioOpened;
        public event SDRRadioClosedEventArgs OnRadioClosing;
        public event SDRRadioClosedEventArgs OnRadioClosed;

        //Modules. These may be null and are created using the "EnableX" functions
        public ComplexFftView fft;

        //Misc
        private float demodBandwidth;
        private Throttle throttle;
        private SDRRadioStatus status;
        private List<IRadioSampleReceiver> demodSampleReceivers;

        //Config
        private int bufferSize;
        private float outputSampleRate;
        private bool realtime;

        //Demodulation stuff
        private RadioDemodulationSession demodulator;

        private int audioResampledBufferLength;
        private UnsafeBuffer audioResampledBufferL;
        private UnsafeBuffer audioResampledBufferR;
        private float* audioResampledBufferLPtr;
        private float* audioResampledBufferRPtr;

        //IQ Stuff
        private UnsafeBuffer iqBuffer;
        private Complex* iqBufferPtr;
        private UnsafeBuffer demodIqBuffer;
        private Complex* demodIqBufferPtr;

        public IIQSource source;
        public float iqSampleRate;
        private Thread workerThread;

        public SDRRadio(SDRRadioConfig cfg)
        {
            this.bufferSize = cfg.bufferSize;
            this.outputSampleRate = cfg.outputAudioSampleRate;
            this.realtime = cfg.realtime;
            this.demodSampleReceivers = new List<IRadioSampleReceiver>();

            //Open buffers
            iqBuffer = UnsafeBuffer.Create(bufferSize, sizeof(Complex));
            iqBufferPtr = (Complex*)iqBuffer;
            demodIqBuffer = UnsafeBuffer.Create(bufferSize, sizeof(Complex));
            demodIqBufferPtr = (Complex*)demodIqBuffer;
        }

        public ComplexFftView EnableFFT(int fftBinSize = 2048, int fftAveragingSize = 10)
        {
            if(fft == null)
                fft = new ComplexFftView(fftBinSize, fftAveragingSize);
            return fft;
        }

        public void AddDemodReceiver(IRadioSampleReceiver r)
        {
            lock(demodSampleReceivers)
            {
                //Add
                demodSampleReceivers.Add(r);

                //Open
                r.Open(outputSampleRate, bufferSize);
            }
        }

        public void OpenRadio(IIQSource source)
        {
            //Stop if needed
            StopRadio();
            
            //Open source
            this.source = source;
            iqSampleRate = source.Open(bufferSize);

            //Configure
            status = SDRRadioStatus.RUNNING;
            throttle = new Throttle(iqSampleRate, -1f * iqSampleRate);

            //Send events
            OnRadioOpened?.Invoke(iqSampleRate);

            //Start worker thread
            workerThread = new Thread(RunWorkerThread);
            workerThread.Name = "LibSDR Radio Worker Thread";
            workerThread.IsBackground = true;
            workerThread.Start();
        }

        public void StopRadio()
        {
            //Make sure it's running
            if (status == SDRRadioStatus.STOPPED)
                return;

            //Request stop
            status = SDRRadioStatus.STOPPING;

            //Send events
            OnRadioClosing?.Invoke();

            //Wait
            while (status != SDRRadioStatus.STOPPED)
                Thread.Sleep(10);

            //Send events
            OnRadioClosed?.Invoke();
        }

        public void SetDemodulator(IDemodulator demodulator, float demodBandwidth)
        {
            //Create and set
            var m = new RadioDemodulationSession(demodulator, bufferSize, iqSampleRate, demodBandwidth, outputSampleRate);

            //Create demod buffers
            audioResampledBufferLength = bufferSize;//this.demodulator.CalculateAudioBufferSize();
            audioResampledBufferL = UnsafeBuffer.Create(audioResampledBufferLength, sizeof(float));
            audioResampledBufferR = UnsafeBuffer.Create(audioResampledBufferLength, sizeof(float));
            audioResampledBufferLPtr = (float*)audioResampledBufferL;
            audioResampledBufferRPtr = (float*)audioResampledBufferR;

            //Set
            this.demodulator = m;
        }

        private void RunWorkerThread()
        {
            //Run
            try
            {
                while (status == SDRRadioStatus.RUNNING)
                {
                    //Read from source
                    int read = source.Read(iqBufferPtr, bufferSize);

                    //Process
                    ProcessIQ(read);

                    //Throttle
                    if (realtime)
                        throttle.Work(read);
                }
            } catch (Exception ex)
            {
                Console.WriteLine("SDRRadio: Error in worker thread " + ex.Message + ex.StackTrace);
            }

            //Clean up and stop
            if (source != null)
            {
                source.Close();
                source.Dispose();
            }
            if(demodulator != null)
            {
                demodulator.Dispose();
            }
            if(fft != null)
            {
                fft.Dispose();
            }
            status = SDRRadioStatus.STOPPED;
        }

        private void ProcessIQ(int read)
        {
            //Broadcast events
            OnIqSamplesAvailable?.Invoke(iqBufferPtr, read);

            //Process FFT
            if(fft != null)
                fft.ProcessSamples(iqBufferPtr);

            //Process demodulator (do this last)
            if (demodulator != null)
                Demodulate(read);
        }

        //THIS IS A DESTRUCTIVE ACTION! Only run this LAST on the IQ samples
        private void Demodulate(int read)
        {
            //Process, demodulate, and resample
            int audioRead = demodulator.Process(iqBufferPtr, audioResampledBufferLPtr, audioResampledBufferRPtr, read);

            //Broadcast events
            OnAudioSamplesAvailable?.Invoke(audioResampledBufferLPtr, audioResampledBufferRPtr, audioRead);

            //Send to demodSampleReceivers
            lock(demodSampleReceivers)
            {
                foreach (var d in demodSampleReceivers)
                    d.OnSamples(audioResampledBufferLPtr, audioResampledBufferRPtr, audioRead);
            }
        }
    }

    public unsafe delegate void SDRRadioIqSamplesEventArgs(Complex* buffer, int samplesRead);
    public unsafe delegate void SDRRadioAudioSamplesEventArgs(float* left, float* right, int samplesRead);
    public unsafe delegate void SDRRadioOpenedEventArgs(float iqSampleRate);
    public unsafe delegate void SDRRadioClosedEventArgs();
}
