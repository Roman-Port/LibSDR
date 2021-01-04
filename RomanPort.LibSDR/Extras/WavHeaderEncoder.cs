using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RomanPort.LibSDR.Extras
{
    public class WavHeaderEncoder
    {
        public const int WAV_HEADER_SIZE = 44;

        public int sampleRate;
        public short bitsPerSample = 16;
        public short channels = 2;
        public int audioLength;

        protected Stream underlyingStream;
        protected long fileSizeOffs;
        protected long dataSizeOffs;

        public WavHeaderEncoder(Stream underlyingStream, int sampleRate, short bitsPerSample, short channels, int audioLength = 0)
        {
            this.underlyingStream = underlyingStream;
            this.sampleRate = sampleRate;
            this.bitsPerSample = bitsPerSample;
            this.channels = channels;
            this.audioLength = audioLength;
        }

        public void WriteHeader()
        {
            //Calculate
            short blockAlign = (short)(channels * (bitsPerSample / 8));
            int avgBytesPerSec = sampleRate * (int)blockAlign;

            //Write
            WriteTag("RIFF");
            fileSizeOffs = 4;
            WriteSignedInt(audioLength);
            WriteTag("WAVE");
            WriteTag("fmt ");
            WriteSignedInt(16);
            WriteSignedShort(1); //Format tag
            WriteSignedShort(channels);
            WriteSignedInt(sampleRate);
            WriteSignedInt(avgBytesPerSec);
            WriteSignedShort(blockAlign);
            WriteSignedShort(bitsPerSample);
            WriteTag("data");
            dataSizeOffs = 40;
            WriteSignedInt(audioLength);
        }

        public unsafe void WriteRawData(byte* ptr, int size)
        {
            byte[] buffer = new byte[32];
            while(size > 0)
            {
                int readable = Math.Min(size, buffer.Length);
                fixed (byte* bufferPtr = buffer)
                    Utils.Memcpy(bufferPtr, ptr, readable);
                size -= readable;
                ptr += readable;
                audioLength += readable;
                underlyingStream.Write(buffer, 0, readable);
            }
            UpdateLength();
        }

        public void UpdateLength()
        {
            //Save current position
            long pos = this.underlyingStream.Position;

            //Update file length
            this.underlyingStream.Position = fileSizeOffs;
            WriteSignedInt(audioLength + 8);

            //Update data length
            this.underlyingStream.Position = dataSizeOffs;
            WriteSignedInt(audioLength);

            //Jump back
            this.underlyingStream.Position = pos;
        }

        private void WriteTag(string tag)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(tag);
            underlyingStream.Write(bytes, 0, bytes.Length);
        }

        private void WriteSignedInt(int value)
        {
            WriteEndianBytes(BitConverter.GetBytes(value));
        }

        private void WriteSignedShort(short value)
        {
            WriteEndianBytes(BitConverter.GetBytes(value));
        }

        private void WriteEndianBytes(byte[] data)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(data);
            underlyingStream.Write(data, 0, data.Length);
        }
    }
}
