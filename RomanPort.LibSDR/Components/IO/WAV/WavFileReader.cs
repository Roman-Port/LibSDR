using RomanPort.LibSDR.Sources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace RomanPort.LibSDR.Components.IO.WAV
{
    public unsafe class WavFileReader : WavFile
    {
        private const int BUFFER_SIZE = 32768;

        public WavFileReader(Stream underlyingStream) : base(underlyingStream, BUFFER_SIZE)
        {
            //Read WAV header
            byte[] header = new byte[WAV_HEADER_LENGTH];
            underlyingStream.Read(header, 0, WAV_HEADER_LENGTH);
            info = ParseWavHeader(header);
        }

        public int Read(Complex* iq, int count)
        {
            return Read((float*)iq, count * 2) / 2;
        }

        public int Read(float* ptr, int count)
        {
            //Validate
            if (disposed)
                throw new ObjectDisposedException("WavFileReader");

            //Read
            int totalRead = 0;
            int read;
            do
            {
                //Read from file
                int readable = Math.Min(count * BytesPerSample, BUFFER_SIZE);
                read = underlyingStream.Read(buffer, 0, readable) / BytesPerSample;

                //Convert samples
                switch (BitsPerSample)
                {
                    case 32:
                        //This is a float. We can directly copy the samples
                        Utils.Memcpy(ptr, bufferPtrFloat, read * sizeof(float));
                        break;
                    case 16:
                        //This is a short. We'll use the lookup table
                        for (int i = 0; i < read; i++)
                        {
                            //To conserve space, we only store the positive values in the lookup table. Flip the sign if it is negative
                            if (bufferPtrShort[i] >= 0)
                                ptr[i] = ConversionLookupTable.LOOKUP_INT16[bufferPtrShort[i]];
                            else
                                ptr[i] = -ConversionLookupTable.LOOKUP_INT16[-bufferPtrShort[i]];
                        }
                        break;
                    case 8:
                        //This is a byte. Use our lookup table
                        for (int i = 0; i < read; i++)
                            ptr[i] = ConversionLookupTable.LOOKUP_INT8[bufferPtrByte[i]];
                        break;
                    default:
                        throw new Exception("Unknown data format.");
                }

                //Update state
                count -= read;
                ptr += read;
                totalRead += read;
            } while (read != 0);
            return totalRead;
        }

        /* Header reader */

        public const int WAV_HEADER_LENGTH = 44;

        public static WavFileInfo ParseWavHeader(byte[] header)
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
                throw new Exception("Malformed or unsuppported WAV header.");

            //Create
            return new WavFileInfo
            {
                bitsPerSample = bitsPerSample,
                channels = channels,
                sampleRate = fileSampleRate
            };
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
