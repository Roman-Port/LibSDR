using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.IO.NetIQ.Commands
{
    public class NetIQCommandOpenStream : BaseNetIQCommand
    {
        public NetIQCommandOpenStream(byte[] data) : base(data)
        {
        }

        public NetIQCommandOpenStream() : base(new byte[LENGTH])
        {
            Opcode = NetIQOpcode.OPEN_STREAM;
        }

        public const int LENGTH = HEADER_LEN + 6;

        public ushort StreamPort
        {
            get => ReadUShort(0);
            set => WriteUShort(value, 0);
        }

        public ushort BufferSize
        {
            get => ReadUShort(2);
            set => WriteUShort(value, 2);
        }

        public NetIQSampleFormat SampleFormat
        {
            get => (NetIQSampleFormat)ReadUShort(4);
            set => WriteUShort((ushort)value, 4);
        }
    }
}
