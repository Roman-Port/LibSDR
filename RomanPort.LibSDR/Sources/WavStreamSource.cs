using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RomanPort.LibSDR.Sources
{
    public unsafe class WavStreamSource : IIQSource
    {
        private Stream source;
        private bool keepSourceOpen;

        private short formatTag;
        public short channels;
        private int fileSampleRate;
        private int avgBytesPerSec;
        private short blockAlign;
        public short bitsPerSample;

        private byte[] dataBuffer;
        public int bytesPerSample;

        private float? queuedSeek; //If not null, we'll jump to these seconds into the recording when we next read samples

        public WavStreamSource(Stream source, bool keepSourceOpen = false, float startSeconds = 0)
        {
            this.source = source;
            this.keepSourceOpen = keepSourceOpen;
            queuedSeek = startSeconds;
        }
        
        public override float Open(int bufferLength)
        {
            //Read WAV header
            source.Position = 0;
            string fileTag = ReadTag();
            int fileLen = ReadInt32();
            string wavTag = ReadTag();
            string fmtTag = ReadTag();
            ReadInt32(); //Unknown, 16
            formatTag = ReadInt16();
            channels = ReadInt16();
            fileSampleRate = ReadInt32();
            avgBytesPerSec = ReadInt32();
            blockAlign = ReadInt16();
            bitsPerSample = ReadInt16();
            string dataTag = ReadTag();
            int dataLen = ReadInt32();

            //Validate
            if (fileTag != "RIFF" || wavTag != "WAVE" || fmtTag != "fmt " || dataTag != "data")
                throw new Exception("This is not a valid WAV file. Tags were not set correctly.");

            //Make sure this is an IQ file
            if (channels != 2)
                throw new Exception($"This is not an IQ file. Expected 2 channels, got {channels}.");

            //Make sure this is a supported type
            if (bitsPerSample != 8 && bitsPerSample != 16 && bitsPerSample != 32)
                throw new Exception("Unsupported format. Only 8, 16, and 32 bits per sample are supported.");

            //Open buffer
            bytesPerSample = bitsPerSample / 8;
            dataBuffer = new byte[bufferLength * bytesPerSample * 2];

            return fileSampleRate;
        }

        public float GetPositionSeconds()
        {
            return (float)(source.Position - 44) / fileSampleRate / channels / bytesPerSample;
        }

        public float GetLengthSeconds()
        {
            return (float)(source.Length - 44) / fileSampleRate / channels / bytesPerSample;
        }

        public void SkipSamples(long offset)
        {
            //Not thread safe!
            source.Position += offset * bytesPerSample * channels;
        }

        public void SafeSkipToSeconds(float pos)
        {
            queuedSeek = pos;
        }

        public int ReadSeek(Complex* iq, int bufferLength, float startSeconds)
        {
            queuedSeek = startSeconds;
            return Read(iq, bufferLength);
        }

        public override int Read(Complex* iq, int bufferLength)
        {
            //Seek if needed
            if(queuedSeek.HasValue)
            {
                source.Position = 44 + (long)(queuedSeek.Value * channels * fileSampleRate * bytesPerSample);
                queuedSeek = null;
            }

            //Validate
            if (bufferLength * bytesPerSample * 2 > dataBuffer.Length)
                throw new Exception("Buffer was not large enough!");
            
            //Read from file
            int complexesRead = source.Read(dataBuffer, 0, bufferLength * bytesPerSample * 2) / bytesPerSample / 2;

            //Read samples
            for(int i = 0; i<complexesRead; i++)
            {
                iq[i] = new Complex(
                    ReadSampleFromBuffer(i * bytesPerSample * 2),
                    ReadSampleFromBuffer((i * bytesPerSample * 2) + bytesPerSample)
                    );
            }

            return complexesRead;
        }

        private float ReadSampleFromBuffer(int byteIndex)
        {
            float sample;
            if(bitsPerSample == 32)
            {
                //Makes my life easy.
                sample = BitConverter.ToSingle(dataBuffer, byteIndex);
            } else if (bitsPerSample == 16)
            {
                //Short. Convert to a float
                sample = BitConverter.ToInt16(dataBuffer, byteIndex);
                sample /= 32768f;
            } else if (bitsPerSample == 8)
            {
                //Byte. Convert to a float
                sample = (float)dataBuffer[byteIndex];
                sample -= 128;
                sample /= 128;
            } else
            {
                throw new Exception("Unknown data type.");
            }
            return sample;
        }

        public override void Close()
        {
            //Close file
            if (!keepSourceOpen)
                source.Close();
        }

        private string ReadTag(int length = 4)
        {
            byte[] buffer = new byte[length];
            source.Read(buffer, 0, length);
            return Encoding.ASCII.GetString(buffer);
        }

        private int ReadInt32()
        {
            byte[] buffer = new byte[4];
            source.Read(buffer, 0, 4);
            return BitConverter.ToInt32(buffer, 0);
        }

        private short ReadInt16()
        {
            byte[] buffer = new byte[2];
            source.Read(buffer, 0, 2);
            return BitConverter.ToInt16(buffer, 0);
        }

        public override void Dispose()
        {
            if (!keepSourceOpen)
                source.Dispose();
        }
    }
}
