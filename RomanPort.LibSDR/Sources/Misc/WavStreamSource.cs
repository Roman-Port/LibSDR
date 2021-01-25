using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Components.IO.WAV;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using RomanPort.LibSDR.Components.IO;

namespace RomanPort.LibSDR.Sources.Misc
{
    public unsafe class WavStreamSource : ISource
    {
        public event SamplesAvailableEventArgs OnSamplesAvailable;
        public event SampleRateChangedEventArgs OnSampleRateChanged;

        private string path;
        private FileStream file;
        private bool keepFileOpen;

        private WavFileReader reader;
        private int bufferSize;
        private volatile StreamStatus streamStatus;
        private Thread streamThread;
        private UnsafeBuffer buffer;
        private Complex* bufferPtr;
        private SampleThrottle throttle;

        public int Channels { get => reader.Channels; }
        public int SampleRate { get => reader.SampleRate; }

        public WavStreamSource(string path)
        {
            this.path = path;
            keepFileOpen = false;
        }

        public WavStreamSource(FileStream file, bool keepFileOpen = false)
        {
            this.file = file;
            this.keepFileOpen = keepFileOpen;
        }

        public void Open(int bufferSize)
        {
            //Open FileStream if needed
            if(file == null)
                file = new FileStream(path, FileMode.Open, FileAccess.Read);

            //Open reader
            reader = new WavFileReader(file);
            this.bufferSize = bufferSize;

            //Validate
            if (reader.Channels != 2)
                throw new Exception("This is not an IQ file. Two channels are required.");

            //Make throttle
            throttle = new SampleThrottle(reader.SampleRate);

            //Send events
            OnSampleRateChanged?.Invoke(reader.SampleRate);
        }

        public void Close()
        {
            //Stop streaming if needed
            EndStreaming();

            //Close reader
            reader.Dispose();

            //Close file if needed
            if (!keepFileOpen)
                file.Close();
            file = null;
        }

        public void BeginStreaming()
        {
            //Check flag
            if (streamStatus != StreamStatus.STOPPED)
                return;

            //Open buffer
            buffer = UnsafeBuffer.Create(bufferSize, sizeof(Complex));
            bufferPtr = (Complex*)buffer;

            //Begin streaming
            streamStatus = StreamStatus.RUNNING;
            streamThread = new Thread(StreamWorker);
            streamThread.IsBackground = true;
            streamThread.Name = "WAV Stream Thread";
            streamThread.Start();
        }

        private void StreamWorker()
        {
            while(streamStatus == StreamStatus.RUNNING)
            {
                int read = reader.Read(bufferPtr, bufferSize);
                throttle.SamplesProcessed(read);
                OnSamplesAvailable?.Invoke(bufferPtr, read);
                throttle.Throttle();
            }
            streamStatus = StreamStatus.STOPPED;
        }

        public void EndStreaming()
        {
            //Check flag
            if (streamStatus != StreamStatus.RUNNING)
                return;

            //Stop
            streamStatus = StreamStatus.STOPPING;
            while (streamStatus == StreamStatus.STOPPING) ;

            //Dispose of buffers
            buffer.Dispose();
        }

        enum StreamStatus
        {
            STOPPED,
            STOPPING,
            RUNNING
        }
    }
}
