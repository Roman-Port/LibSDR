using RomanPort.LibSDR.Sources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace RomanPort.LibSDR.Components.IO.RAW
{
    public unsafe class StreamSampleReader : StreamSampleIO, ISampleReader
    {
        public StreamSampleReader(Stream underlyingStream, SampleFormat format, int sampleRate, int headerLength, int channels, int bufferSize) : base(underlyingStream, format, sampleRate, headerLength, channels, bufferSize)
        {
        }

        public int Read(Complex* iq, int count)
        {
            return Read((float*)iq, count * 2) / 2;
        }

        public int Read(float* ptr, int count)
        {
            //Validate
            if (disposed)
                throw new ObjectDisposedException("StreamSampleReader");

            //Read
            int totalRead = 0;
            int read;
            do
            {
                //Read from file
                int readable = Math.Min(count * BytesPerSample, bufferSizeBytes);
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
    }
}
