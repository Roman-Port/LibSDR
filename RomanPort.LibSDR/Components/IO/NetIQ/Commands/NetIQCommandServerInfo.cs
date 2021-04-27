using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.IO.NetIQ.Commands
{
    public class NetIQCommandServerInfo : BaseNetIQCommand
    {
        public NetIQCommandServerInfo(byte[] data) : base(data)
        {
        }

        public NetIQCommandServerInfo() : base(new byte[LENGTH])
        {
            Opcode = NetIQOpcode.SERVER_INFO;
        }

        public const int LENGTH = HEADER_LEN + 8;

        public ushort ServerVersionMinor
        {
            get => ReadUShort(0);
            set => WriteUShort(value, 0);
        }

        public ushort ServerVersionMajor
        {
            get => ReadUShort(2);
            set => WriteUShort(value, 2);
        }

        public uint SampleRate
        {
            get => ReadUInt(4);
            set => WriteUInt(value, 4);
        }
    }
}
