using RomanPort.LibSDR.NRSC5.Framework.Layer1.Frames;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.Layer1.Util
{
    public class PidsReader
    {
        public PidsReader(FramePids frame)
        {
            this.frame = frame;
        }

        private static readonly char[] CHARSET_5BIT = "ABCDEFGHIJKLMNOPQRSTUVWXYZ ?-*$ ".ToCharArray();

        private FramePids frame;
        private int index;

        public int Index { get => index; }
        public int Remaining { get => Length - Index; }
        public int Length { get => frame.bits.Length; }

        public void SkipBits(int count)
        {
            index += count;
        }

        public byte[] ReadBytes(int byteCount)
        {
            byte[] b = new byte[byteCount];
            for (int i = 0; i < byteCount; i++)
                b[i] = (byte)ReadUInt(8);
            return b;
        }

        public bool ReadBitBool()
        {
            bool bit = frame.bits[index] == 1;
            index++;
            return bit;
        }

        public byte ReadBit()
        {
            byte bit = frame.bits[index];
            index++;
            return bit;
        }

        public uint ReadUInt(int length)
        {
            uint result = 0;
            for (int i = 0; i < length; i++)
            {
                result <<= 1;
                result |= frame.bits[index++];
            }
            return result;
        }

        public int ReadInt(int length)
        {
            int result = (int)ReadUInt(length);
            return (result & (1 << (length - 1))) != 0 ? result - (1 << length) : result;
        }

        public char ReadChar5()
        {
            return CHARSET_5BIT[ReadUInt(5)];
        }

        public char[] ReadChar5Array(int count)
        {
            char[] c = new char[count];
            for (int i = 0; i < count; i++)
                c[i] = ReadChar5();
            return c;
        }

        public char ReadChar7()
        {
            return (char)ReadInt(7);
        }

        public char[] ReadChar7Array(int count)
        {
            char[] c = new char[count];
            for (int i = 0; i < count; i++)
                c[i] = ReadChar7();
            return c;
        }

        public string ReadUTF8String(int len)
        {
            throw new NotImplementedException();
        }
    }
}
