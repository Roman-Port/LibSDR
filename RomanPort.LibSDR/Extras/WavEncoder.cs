using RomanPort.LibSDR.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RomanPort.LibSDR.Extras
{
    public class WavEncoder : WavHeaderEncoder
    {
        private byte[] fileBuffer;
        private int fileBufferSampleCount;

        public WavEncoder(Stream underlyingStream, int sampleRate, short channels, short bitsPerSample, int bufferCount = 1024) : base(underlyingStream, sampleRate, bitsPerSample, channels)
        {
            //Validate
            if (bitsPerSample != 8 && bitsPerSample != 16 && bitsPerSample != 32)
                throw new Exception("Only PCM8, PCM16, and Float32 bitsPerSample types are supported!");

            //Create buffer
            fileBufferSampleCount = bufferCount;
            fileBuffer = new byte[fileBufferSampleCount * (bitsPerSample / 8)];

            //Write header
            WriteHeader();
        }

        public unsafe void Write(InterleavedStereoAudio* samples, int sampleCount)
        {
            //Write this as if it were complexes, as they are identical
            Write((Complex*)samples, sampleCount);
        }

        public unsafe void Write(Complex* samples, int sampleCount)
        {
            //We're gonna just write this as if it were floats (because it is)
            //We require two channels for this, however
            if (channels != 2)
                throw new Exception("Writing complexes requires there to be two samples set.");

            //Cast this to a float pointer
            float* floatSamples = (float*)samples;

            //Write this as if it were floats
            Write(floatSamples, sampleCount * 2);
        }

        public unsafe void Write(float* samples, int sampleCount)
        {
            WriteChannels(sampleCount, samples);
        }

        public unsafe void Write(float* channelLeft, float* channelRight, int sampleCount)
        {
            WriteChannels(sampleCount, channelLeft, channelRight);
        }

        /// <summary>
        /// Writes channels interleved together. All channels must have the same number of samples to write.
        /// </summary>
        /// <param name="channels"></param>
        /// <param name="sampleCount"></param>
        public unsafe void WriteChannels(int sampleCount, params float*[] channels)
        {
            //Write all samples
            int samplesRemaining = sampleCount * channels.Length;
            int samplesWrittenPerChannel = 0;
            int lastChannel = 0;
            while (samplesRemaining > 0)
            {
                //Get sample count to write
                int samplesToWrite = Math.Min(samplesRemaining, fileBufferSampleCount);

                //Encode and interleave samples
                for (int i = 0; i < samplesToWrite; i++)
                {
                    if(channels[lastChannel] == null)
                        EncodeSample(fileBuffer, i * (bitsPerSample / 8), 0);
                    else
                        EncodeSample(fileBuffer, i * (bitsPerSample / 8), channels[lastChannel][samplesWrittenPerChannel]);
                    lastChannel++;
                    if (lastChannel >= channels.Length)
                    {
                        samplesWrittenPerChannel++;
                        lastChannel = 0;
                    }
                }

                //Write
                underlyingStream.Write(fileBuffer, 0, samplesToWrite * (bitsPerSample / 8));

                //Update
                audioLength += samplesToWrite * (bitsPerSample / 8);
                samplesRemaining -= samplesToWrite;
            }

            //Update length
            UpdateLength();
        }

        private void EncodeSample(byte[] buffer, int bufferPos, float sample)
        {
            if(bitsPerSample == 32)
            {
                //Float to float. Makes my life easy.
                BitConverter.GetBytes(sample).CopyTo(buffer, bufferPos);
            } else if (bitsPerSample == 16)
            {
                //Convert to short
                BitConverter.GetBytes((short)(sample * 32768)).CopyTo(buffer, bufferPos);
            } else if (bitsPerSample == 8)
            {
                //Convert to byte
                buffer[bufferPos] = (byte)((sample * 128) + 128);
            } else
            {
                throw new Exception("Unknown format.");
            }
        }
    }
}
