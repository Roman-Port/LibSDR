using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Digital.RDS.Data.Commands
{
    public class RdsRadioTextCommand : RdsCommand
    {
        public RdsRadioTextCommand(ulong frame) : base(frame)
        {
        }

        /* Depending on if this is a version A/B RadioText packet, we have a different amount of characters (and thus a different offset).
         * Version A uses both group C and D to send four characters. Offset per segment is 4.
         * Version B uses only group D to send two characters. The PI code is sent again in group C. Offset per segment is 2.
         * 
         * Yuck.
         */

        public bool TextAB { get => ReadBool(OFFSET_GROUP_B_USER + 0); }
        public byte SegmentCharacterAddress { get => (byte)ReadInteger(OFFSET_GROUP_B_USER + 1, 4); }
        public byte SegmentSize { get => (byte)(GroupVersionB ? 2 : 4); }
        public int AbsoluteCharacterAddress { get => SegmentCharacterAddress * SegmentSize; }
        public int ReadCharacters(char[] buffer, int offset)
        {
            //Check length
            if (buffer.Length - offset < SegmentSize)
                throw new Exception("Buffer is not large enough to hold segment!");

            //Read
            if(GroupVersionB)
            {
                //Only two characters 
                buffer[offset + 0] = ReadChar(OFFSET_GROUP_D + 0);
                buffer[offset + 1] = ReadChar(OFFSET_GROUP_D + 8);
            } else
            {
                //Four characters 
                buffer[offset + 0] = ReadChar(OFFSET_GROUP_C + 0);
                buffer[offset + 1] = ReadChar(OFFSET_GROUP_C + 8);
                buffer[offset + 2] = ReadChar(OFFSET_GROUP_D + 0);
                buffer[offset + 3] = ReadChar(OFFSET_GROUP_D + 8);
            }

            return SegmentSize;
        }

        public override string DescribeCommand()
        {
            char[] s = new char[SegmentSize];
            ReadCharacters(s, 0);
            return $"RT [{AbsoluteCharacterAddress}] = " + new string(s);
        }
    }
}
