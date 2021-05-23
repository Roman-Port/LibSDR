using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Digital.RDS.Data.Commands
{
    public class RdsCommand
    {
        public RdsCommand(ulong frame)
        {
            this.frame = frame;
        }

        private ulong frame;

        public ulong Raw { get => frame; }

        protected int OFFSET_GROUP_C = 32;
        protected int OFFSET_GROUP_D = 48;
        protected int OFFSET_END = 64;
        protected int OFFSET_GROUP_B_USER = 27;

        private static ulong ReadIntegerBits(ulong input, int bitOffset, int bitLength)
        {
            //Flip bits and read
            ulong output = 0;
            for (int j = bitLength - 1; j >= 0; j--)
                output |= ((input >> bitOffset++) & 1) << j;
            return output;
        }

        protected long ReadSignedInteger(int bitOffset, int bitLength)
        {
            long value = (long)ReadInteger(bitOffset + 1, bitLength - 1);
            if (ReadInteger(bitOffset, 1) == 1)
                value = -value;
            return value;
        }

        protected ulong ReadInteger(int bitOffset, int bitLength)
        {
            return ReadIntegerBits(frame, bitOffset, bitLength);
        }

        protected bool ReadBool(int bitOffset)
        {
            return ReadInteger(bitOffset, 1) == 1;
        }

        protected char ReadChar(int bitOffset)
        {
            return (char)ReadInteger(bitOffset, 8);
        }

        public ushort PiCode { get => (ushort)ReadInteger(0, 16); }
        public byte GroupType { get => (byte)ReadInteger(16, 4); }
        public bool GroupVersionB { get => ReadInteger(20, 1) == 1; }
        public bool TrafficProgram { get => ReadInteger(21, 1) == 1; }
        public byte ProgramType { get => (byte)ReadInteger(22, 5); }
        public string GroupName { get => GroupType + (GroupVersionB ? "B" : "A"); }

        public virtual string DescribeCommand()
        {
            return $"Unsupported.";
        }

        public bool TryAsCommand<T>(out T command) where T : RdsCommand
        {
            if(GetType() == typeof(T))
            {
                command = (T)this;
                return true;
            } else
            {
                command = null;
                return false;
            }
        }

        public static RdsCommand DecodeCommand(ulong frame)
        {
            //Determine the group type and version
            ulong groupType = ReadIntegerBits(frame, 16, 4);
            bool groupVersionB = ReadIntegerBits(frame, 19, 1) == 1;

            //Determine special types
            if (groupType == 0)
                return new RdsBasicTuningCommand(frame);
            if (groupType == 2)
                return new RdsRadioTextCommand(frame);
            if (groupType == 4 && !groupVersionB)
                return new RdsClockTimeCommand(frame);

            //Return generic
            return new RdsCommand(frame);
        }
    }
}
