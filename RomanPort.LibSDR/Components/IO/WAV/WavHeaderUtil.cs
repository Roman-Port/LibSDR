using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RomanPort.LibSDR.Components.IO.WAV
{
    public static class WavHeaderUtil
    {
        public const int HEADER_LENGTH = 44;
        
        public static byte[] CreateHeader(WavFileInfo info)
        {
            //Allocate
            byte[] buffer = new byte[HEADER_LENGTH];
            int offset = 0;
            
            //Calculate
            short blockAlign = (short)(info.channels * (info.bitsPerSample / 8));
            int avgBytesPerSec = info.sampleRate * (int)blockAlign;

            //Write
            WriteTag(buffer, ref offset, "RIFF");
            WriteSignedInt(buffer, ref offset, -1); //Length
            WriteTag(buffer, ref offset, "WAVE");
            WriteTag(buffer, ref offset, "fmt ");
            WriteSignedInt(buffer, ref offset, 16);
            WriteSignedShort(buffer, ref offset, 1); //Format tag
            WriteSignedShort(buffer, ref offset, info.channels);
            WriteSignedInt(buffer, ref offset, info.sampleRate);
            WriteSignedInt(buffer, ref offset, avgBytesPerSec);
            WriteSignedShort(buffer, ref offset, blockAlign);
            WriteSignedShort(buffer, ref offset, info.bitsPerSample);
            WriteTag(buffer, ref offset, "data");
            WriteSignedInt(buffer, ref offset, -1); //Length

            return buffer;
        }

        public static void UpdateLength(Stream s, int audioLength)
        {
            //Allocate
            byte[] buffer = new byte[4];
            int offset;
            
            //Update file length
            s.Position = 4;
            offset = 0;
            WriteSignedInt(buffer, ref offset, audioLength + 8);
            s.Write(buffer, 0, 4);

            //Update data length
            s.Position = 40;
            offset = 0;
            WriteSignedInt(buffer, ref offset, audioLength);
            s.Write(buffer, 0, 4);
        }

        private static void WriteTag(byte[] buffer, ref int offset, string tag)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(tag);
            WriteBytes(bytes, buffer, ref offset);
        }

        private static void WriteSignedInt(byte[] buffer, ref int offset, int value)
        {
            WriteEndianBytes(BitConverter.GetBytes(value), buffer, ref offset);
        }

        private static void WriteSignedShort(byte[] buffer, ref int offset, short value)
        {
            WriteEndianBytes(BitConverter.GetBytes(value), buffer, ref offset);
        }

        private static void WriteEndianBytes(byte[] data, byte[] buffer, ref int offset)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(data);
            WriteBytes(data, buffer, ref offset);
        }

        private static void WriteBytes(byte[] src, byte[] data, ref int offset)
        {
            Array.Copy(src, 0, data, offset, src.Length);
            offset += src.Length;
        }
    }
}
