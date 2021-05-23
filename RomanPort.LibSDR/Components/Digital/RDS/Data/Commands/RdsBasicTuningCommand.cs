using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Digital.RDS.Data.Commands
{
    public class RdsBasicTuningCommand : RdsCommand
    {
        public RdsBasicTuningCommand(ulong frame) : base(frame)
        {
        }

        public bool TrafficAnnouncement { get => ReadBool(OFFSET_GROUP_B_USER + 0); }
        public bool Music { get => ReadBool(OFFSET_GROUP_B_USER + 1); }
        public bool Di { get => ReadBool(OFFSET_GROUP_B_USER + 2); }
        public byte SegmentCharacterAddress { get => (byte)ReadInteger(30, 2); }
        public char PsCharA { get => ReadChar(OFFSET_GROUP_D); }
        public char PsCharB { get => ReadChar(OFFSET_GROUP_D + 8); }

        //Version A messages will also have alternative frequencies in group C. With verion B, it's just the PI code repeated

        public int AbsoluteCharacterAddress { get => SegmentCharacterAddress * 2; }

        public override string DescribeCommand()
        {
            return $"PS [{AbsoluteCharacterAddress}] = {PsCharA}{PsCharB}";
        }
    }
}
