using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace RomanPort.LibSDR.Components.IO.WAV
{
    public unsafe class WavFileWriter : WavFile
    {
        protected int bufferSamplesSize;

        private int[] WRITE_SAME_MULTS = { 2, 2 };
        private int[] WRITE_DIFF_MULTS = { 1, 1 };
        private int[] WRITE_SINGLE_MULTS = { 1 };

        public WavFileWriter(string path, FileMode mode, int sampleRate, short channels, short bitsPerSample, int bufferSize) : this(new FileStream(path, mode), sampleRate, channels, bitsPerSample, bufferSize)
        {

        }

        public WavFileWriter(Stream underlyingStream, int sampleRate, short channels, short bitsPerSample, int bufferSize) : base(underlyingStream, channels * (bitsPerSample / 8) * bufferSize)
        {
            //Create info
            info = new WavFileInfo
            {
                bitsPerSample = bitsPerSample,
                channels = channels,
                sampleRate = sampleRate
            };

            //Calculate buffer size
            switch (BitsPerSample)
            {
                case 32: bufferSamplesSize = bufferSizeBytes / channels / sizeof(float); break;
                case 16: bufferSamplesSize = bufferSizeBytes / channels / sizeof(short); break;
                case 8: bufferSamplesSize = bufferSizeBytes / channels / sizeof(byte); break;
                default: throw new Exception("Only PCM8, PCM16, and Float32 bitsPerSample types are supported!");
            }

            //Write header
            byte[] header = WavHeaderUtil.CreateHeader(info);
            underlyingStream.Position = 0;
            underlyingStream.Write(header, 0, header.Length);
        }

        public void Write(Complex* ptr, int count)
        {
            float* fPtr = (float*)ptr;
            WriteChannels(count, WRITE_SAME_MULTS, fPtr, fPtr + 1);
        }

        public void Write(float* left, float* right, int count)
        {
            WriteChannels(count, WRITE_DIFF_MULTS, left, right);
        }
        
        public void Write(float* ptr, int count)
        {
            WriteChannels(count, WRITE_SINGLE_MULTS, ptr);
        }

        public void FinalizeFile()
        {
            long pos = underlyingStream.Position;
            WavHeaderUtil.UpdateLength(underlyingStream, (int)(pos - WavHeaderUtil.HEADER_LENGTH));
            underlyingStream.Position = pos;
        }

        private void WriteChannels(int countPerChannel, int[] perChannelIndexMultiplier, params float*[] channels)
        {
            int offset = 0;
            while(countPerChannel > 0)
            {
                //Calculate transferrable
                int block = Math.Min(countPerChannel, bufferSamplesSize);

                //Copy to buffer
                int bytesCopied;
                switch(BitsPerSample)
                {
                    case 32:
                        float* fBuf = bufferPtrFloat;
                        for (int i = 0; i < countPerChannel; i++)
                        {
                            for (int c = 0; c < channels.Length; c++)
                            {
                                *fBuf = channels[c][(i + offset) * perChannelIndexMultiplier[c]];
                                fBuf++;
                            }
                        }
                        bytesCopied = countPerChannel * channels.Length * sizeof(float);
                        break;
                    case 16:
                        short* sBuf = bufferPtrShort;
                        for (int i = 0; i < countPerChannel; i++)
                        {
                            for (int c = 0; c < channels.Length; c++)
                            {
                                *sBuf = (short)(channels[c][(i + offset) * perChannelIndexMultiplier[c]] * short.MaxValue);
                                sBuf++;
                            }
                        }
                        bytesCopied = countPerChannel * channels.Length * sizeof(short);
                        break;
                    case 8:
                        byte* bBuf = bufferPtrByte;
                        for (int i = 0; i < countPerChannel; i++)
                        {
                            for (int c = 0; c < channels.Length; c++)
                            {
                                *bBuf = (byte)((channels[c][(i + offset) * perChannelIndexMultiplier[c]] * 127.5f) + 127.5f);
                                bBuf++;
                            }
                        }
                        bytesCopied = countPerChannel * channels.Length * sizeof(byte);
                        break;
                    default:
                        throw new Exception("Only PCM8, PCM16, and Float32 bitsPerSample types are supported!");
                }

                //Write to stream
                underlyingStream.Write(buffer, 0, bytesCopied);

                //Update state
                offset += block;
                countPerChannel -= block;
            }
        }
    }
}
