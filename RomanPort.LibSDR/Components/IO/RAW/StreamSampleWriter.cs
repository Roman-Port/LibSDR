using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RomanPort.LibSDR.Components.IO.RAW
{
    public class StreamSampleWriter : StreamSampleIO
    {
        public StreamSampleWriter(Stream underlyingStream, SampleFormat format, int sampleRate, int headerLength, int channels, int bufferSize) : base(underlyingStream, format, sampleRate, headerLength, channels, bufferSize)
        {
        }
        
        public unsafe void Write(Complex* ptr, int count)
        {
            Write((float*)ptr, count * 2);
        }

        public unsafe void Write(float* ptr, int count)
        {
            //Clamp
            for (int i = 0; i < count; i++)
                ptr[i] = ptr[i] > 1 ? 1 : ptr[i];
            for (int i = 0; i < count; i++)
                ptr[i] = ptr[i] < -1 ? -1 : ptr[i];

            //Loop
            while (count > 0)
            {
                //Calculate the number we can copy
                int block = Math.Min(count, bufferSizeSamples);

                //Transform to bytes
                switch(Format)
                {
                    case SampleFormat.Float32:
                        //Directly copy the samples
                        Utils.Memcpy(bufferPtrFloat, ptr, block * sizeof(float));
                        break;
                    case SampleFormat.Short16:
                        //Convert to short 16
                        for (int i = 0; i < block; i++)
                            bufferPtrShort[i] = (short)(ptr[i] * short.MaxValue);
                        break;
                    case SampleFormat.Byte:
                        //Convert to byte
                        for (int i = 0; i < block; i++)
                            bufferPtrByte[i] = (byte)((ptr[i] * 127.5f) + 128);
                        break;
                    default:
                        throw new Exception("Unknown sample format!");
                }

                //Write bytes
                underlyingStream.Write(buffer, 0, block * BytesPerSample);

                //Update state
                count -= block;
                ptr += block;
            }
        }
    }
}
