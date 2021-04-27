using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RomanPort.LibSDR.Components.IO.WAV
{
    public static class WavHeaderUtil
    {
        public const int HEADER_LENGTH = 44;
        
        public static byte[] CreateHeader(WavFileInfo info, int length = -1)
        {
            //Allocate
            byte[] buffer = new byte[HEADER_LENGTH];
            int offset = 0;
            
            //Calculate
            short blockAlign = (short)(info.channels * (info.bitsPerSample / 8));
            int avgBytesPerSec = info.sampleRate * (int)blockAlign;

            //Write
            WriteTag(buffer, ref offset, "RIFF");
            WriteSignedInt(buffer, ref offset, length); //Length
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
            WriteSignedInt(buffer, ref offset, length); //Length

            return buffer;
        }

        public static bool ParseWavHeader(Stream s, out WavFileInfo info)
        {
            byte[] data = new byte[WavHeaderUtil.HEADER_LENGTH];
            s.Read(data, 0, data.Length);
            return ParseWavHeader(data, out info);
        }

        public static bool ParseWavHeader(byte[] header, out WavFileInfo info)
        {
            //Parse
            int pos = 0;
            string fileTag = ReadWavTag(header, ref pos);
            int fileLen = ReadWavInt32(header, ref pos);
            string wavTag = ReadWavTag(header, ref pos);
            string fmtTag = ReadWavTag(header, ref pos);
            ReadWavInt32(header, ref pos); //Unknown, 16
            short formatTag = ReadWavInt16(header, ref pos);
            short channels = ReadWavInt16(header, ref pos);
            int fileSampleRate = ReadWavInt32(header, ref pos);
            int avgBytesPerSec = ReadWavInt32(header, ref pos);
            short blockAlign = ReadWavInt16(header, ref pos);
            short bitsPerSample = ReadWavInt16(header, ref pos);
            string dataTag = ReadWavTag(header, ref pos);
            int dataLen = ReadWavInt32(header, ref pos);

            //Validate
            if (fileTag != "RIFF" || wavTag != "WAVE" || fmtTag != "fmt " || dataTag != "data")
            {
                info = new WavFileInfo();
                return false;
            }

            //Create
            info = new WavFileInfo
            {
                bitsPerSample = bitsPerSample,
                channels = channels,
                sampleRate = fileSampleRate
            };
            return true;
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

        private static string ReadWavTag(byte[] header, ref int offset)
        {
            string v = Encoding.ASCII.GetString(header, offset, 4);
            offset += 4;
            return v;
        }

        private static int ReadWavInt32(byte[] header, ref int offset)
        {
            int v = BitConverter.ToInt32(header, offset);
            offset += 4;
            return v;
        }

        private static short ReadWavInt16(byte[] header, ref int offset)
        {
            short v = BitConverter.ToInt16(header, offset);
            offset += 2;
            return v;
        }
    }
}
