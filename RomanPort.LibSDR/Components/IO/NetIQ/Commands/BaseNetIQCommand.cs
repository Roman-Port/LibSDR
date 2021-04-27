using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace RomanPort.LibSDR.Components.IO.NetIQ.Commands
{
    public abstract class BaseNetIQCommand
    {
        public BaseNetIQCommand(byte[] data)
        {
            this.data = data;
        }

        public const int HEADER_LEN = 2;

        private byte[] data;

        public NetIQOpcode Opcode
        {
            get => (NetIQOpcode)ReadUShort(-HEADER_LEN + 0);
            set => WriteUShort((ushort)value, -HEADER_LEN + 0);
        }

        public void SendOnSocket(Socket sock)
        {
            sock.Send(data);
        }

        protected void WriteUShort(ushort value, int offset)
        {
            BitConverter.GetBytes(value).CopyTo(data, HEADER_LEN + offset);
        }

        protected void WriteShort(short value, int offset)
        {
            BitConverter.GetBytes(value).CopyTo(data, HEADER_LEN + offset);
        }

        protected void WriteUInt(uint value, int offset)
        {
            BitConverter.GetBytes(value).CopyTo(data, HEADER_LEN + offset);
        }

        protected void WriteInt(int value, int offset)
        {
            BitConverter.GetBytes(value).CopyTo(data, HEADER_LEN + offset);
        }

        protected ushort ReadUShort(int offset)
        {
            return BitConverter.ToUInt16(data, HEADER_LEN + offset);
        }

        protected short ReadShort(int offset)
        {
            return BitConverter.ToInt16(data, HEADER_LEN + offset);
        }

        protected uint ReadUInt(int offset)
        {
            return BitConverter.ToUInt32(data, HEADER_LEN + offset);
        }

        protected int ReadInt(int offset)
        {
            return BitConverter.ToInt32(data, HEADER_LEN + offset);
        }
    }
}
