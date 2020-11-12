using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Radio;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RomanPort.LibSDR.Extras
{
    /// <summary>
    /// Acts as a rewind buffer. You can also begin recording from this
    /// </summary>
    public unsafe class RewindRecorder<T> : IDisposable where T : unmanaged
    {
        public RewindRecorderState state;
        public long recordedSamples;

        private int rewindBufferReadingRemaining; //Set when we start recording so we know how much we need to read from the rewind buffer to get caught up
        private RewindRecorderOutput<T> output;
        private Thread workerThread;

        private const int CHUNK_SIZE = 2048;

        private UnsafeBuffer buffer;
        private T* bufferPtr;
        private CircularBuffer<T> rewindBuffer; //A buffer that is constantly written to that stores the last some number of seconds
        private CircularBuffer<T> recordingBuffer; //A buffer that is written to only while recording

        public RewindRecorder(int rewindBufferLength, int recordingBufferLength)
        {
            buffer = UnsafeBuffer.Create(CHUNK_SIZE, sizeof(T));
            bufferPtr = (T*)buffer;
            rewindBuffer = new CircularBuffer<T>(rewindBufferLength);
            recordingBuffer = new CircularBuffer<T>(recordingBufferLength);
        }

        public void OnSamples(T* data, int count)
        {
            //Write to the rewind buffer, always
            if(rewindBufferReadingRemaining == 0 || state == RewindRecorderState.STOPPED)
                rewindBuffer.Write(data, count, true);

            //If we're recording, write to the recording buffer
            if(state == RewindRecorderState.RECORDING)
            {
                //Write
                int written = recordingBuffer.Write(data, count);
            }
        }

        public void StartRecording(RewindRecorderOutput<T> output, bool writeRewind = true)
        {
            //Check
            if (state != RewindRecorderState.STOPPED)
                throw new Exception("Already recording.");
            
            //Set up
            this.output = output;
            state = RewindRecorderState.RECORDING;
            rewindBufferReadingRemaining = rewindBuffer.GetAvailable();

            //Open
            output.Open();

            //Copy a small part of the rewind buffer to the recording buffer to keep it seamless
            ProcessWriteChunk();

            //Begin worker thread
            workerThread = new Thread(RunWorkerThread);
            workerThread.IsBackground = true;
            workerThread.Start();
        }

        private void RunWorkerThread()
        {
            while(state == RewindRecorderState.RECORDING)
            {
                ProcessWriteChunk();
            }
        }

        private void ProcessWriteChunk()
        {
            //Determine where to read from
            int read;
            if (rewindBufferReadingRemaining != 0)
            {
                //Read from rewind buffer until we are caught up
                int readable = Math.Min(rewindBufferReadingRemaining, CHUNK_SIZE);
                read = rewindBuffer.Read(bufferPtr, readable);
                rewindBufferReadingRemaining -= read;
            }
            else
            {
                //Read from recording buffer
                read = recordingBuffer.Read(bufferPtr, CHUNK_SIZE);
            }

            //Write to output
            output.OnSamples(bufferPtr, read);
            recordedSamples += read;
        }

        public void Dispose()
        {
            rewindBuffer.Dispose();
            recordingBuffer.Dispose();
            buffer.Dispose();
        }
    }

    public interface RewindRecorderOutput<T> where T : unmanaged
    {
        void Open();
        unsafe void OnSamples(T* data, int count);
        void Close();
    }

    public enum RewindRecorderState
    {
        STOPPED,
        RECORDING,
        SAVING
    }
}
