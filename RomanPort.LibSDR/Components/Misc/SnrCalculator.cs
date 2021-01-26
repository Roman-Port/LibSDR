using RomanPort.LibSDR.Components.FFT;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Misc
{
    public unsafe class SnrCalculator
    {
        public SnrCalculator()
        {
            //Create FFT
            fft = new FFTInstance(SNR_FFT_SIZE);

            //Create buffers
            buffer = UnsafeBuffer.Create(SNR_BUFFER_SIZE, out bufferPtr);
            powerBuffer = UnsafeBuffer.Create(SNR_FFT_SIZE_HALF, out powerBufferPtr);
        }

        private FFTInstance fft;
        private UnsafeBuffer buffer;
        private float* bufferPtr;
        private int bufferIndex;
        private bool bufferReady;

        private SnrReading[] recentReadings = new SnrReading[4];
        private int recentReadingsPosition;
        private int recentReadingsUsage;

        private UnsafeBuffer powerBuffer;
        private double* powerBufferPtr;

        private const int SNR_FFT_SIZE = 256;
        private const int SNR_FFT_SIZE_HALF = SNR_FFT_SIZE / 2;
        private const int SNR_BUFFERS = 32;
        private const int SNR_BUFFER_SIZE = SNR_FFT_SIZE * SNR_BUFFERS;

        public SnrReading CalculateAverageSnr()
        {
            //If we aren't yet ready, return 0
            if (!bufferReady)
                return new SnrReading(0, 0);

            //Read and add to the average
            SnrReading reading = CalculateInstantSnr();
            recentReadings[recentReadingsPosition] = reading;
            recentReadingsPosition = (recentReadingsPosition + 1) % recentReadings.Length;
            if (recentReadingsPosition > recentReadingsUsage)
                recentReadingsUsage = recentReadingsPosition;

            //Calculate average
            double avgCeil = 0;
            double avgFloor = 0;
            for(int i = 0; i<recentReadingsUsage; i++)
            {
                avgCeil += recentReadings[i].ceiling;
                avgFloor += recentReadings[i].floor;
            }

            return new SnrReading((float)(avgFloor / recentReadingsUsage), (float)(avgCeil / recentReadingsUsage));
        }

        public SnrReading CalculateInstantSnr()
        {
            //If we aren't yet ready, return 0
            if (!bufferReady)
                return new SnrReading(0, 0);

            //Clear
            for (int i = 0; i < SNR_FFT_SIZE_HALF; i++)
                powerBufferPtr[i] = 0;

            //Calculate FFT average of the buffers we have
            for (int b = 0; b < SNR_BUFFERS; b++)
            {
                //Process the FFT and offset pointer
                float* power = fft.ProcessSampleBlock(bufferPtr + (SNR_FFT_SIZE * b)) + SNR_FFT_SIZE_HALF;

                //Add
                for (int i = 0; i < SNR_FFT_SIZE_HALF; i++)
                    powerBufferPtr[i] += power[i];
            }

            //Average
            for (int i = 0; i < SNR_FFT_SIZE_HALF; i++)
                powerBufferPtr[i] /= SNR_BUFFERS;

            //Determine min/max
            double ceiling = -1000f;
            double floor = 0;
            for(int i = 1; i<SNR_FFT_SIZE_HALF - 1; i++)
            {
                ceiling = Math.Max(ceiling, powerBufferPtr[i]);
                floor = Math.Min(floor, powerBufferPtr[i]);
            }

            return new SnrReading((float)floor, (float)ceiling);
        }

        public void AddSamples(float* samples, int count)
        {
            //Prepare to copy the last part of this
            int transferrable = Math.Min(count, SNR_BUFFER_SIZE);
            samples += count - transferrable;

            //Copy
            for(int i = 0; i<transferrable; i++)
            {
                bufferPtr[bufferIndex] = samples[i];
                bufferIndex++;
                if(bufferIndex >= SNR_BUFFER_SIZE)
                {
                    bufferReady = true;
                    bufferIndex = 0;
                }
            }
        }
    }
}
