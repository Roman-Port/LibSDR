using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace RomanPort.LibSDR.Components.IO.NetIQ.Server
{
    class NetIQServerClient
    {
        public NetIQServerClient(Socket sock)
        {
            this.sock = sock;
            buffer = new byte[4096];
        }

        public Socket sock;
        public byte[] buffer;

        public NetIQServerStream stream;
    }
}
