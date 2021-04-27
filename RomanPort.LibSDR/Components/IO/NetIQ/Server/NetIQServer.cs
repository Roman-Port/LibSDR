using RomanPort.LibSDR.Components.IO.NetIQ.Commands;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RomanPort.LibSDR.Components.IO.NetIQ.Server
{
    public class NetIQServer
    {
        public NetIQServer(IPEndPoint endpoint, uint sampleRate)
        {
            this.sampleRate = sampleRate;
            
            sock = new Socket(SocketType.Stream, ProtocolType.Tcp);
            sock.Bind(endpoint);
            sock.Listen(4);
            sock.BeginAccept(OnClientConnect, null);
        }

        private Socket sock;
        private List<NetIQServerStream> streams = new List<NetIQServerStream>();
        private uint sampleRate;

        private void OnClientConnect(IAsyncResult ar)
        {
            //Get socket
            Socket client = sock.EndAccept(ar);

            //Create client context
            NetIQServerClient ctx = new NetIQServerClient(client);

            //Create and send info packet
            NetIQCommandServerInfo info = new NetIQCommandServerInfo();
            info.ServerVersionMinor = NetIQUtil.PROTO_VERSION_MINOR;
            info.ServerVersionMajor = NetIQUtil.PROTO_VERSION_MAJOR;
            info.SampleRate = sampleRate;
            info.SendOnSocket(client);

            //Begin listening on this
            client.BeginReceive(ctx.buffer, 0, ctx.buffer.Length, SocketFlags.None, OnClientReceiveData, ctx);

            //Look for another new client
            sock.BeginAccept(OnClientConnect, null);
        }

        private void OnClientReceiveData(IAsyncResult ar)
        {
            //Get data
            NetIQServerClient ctx = (NetIQServerClient)ar.AsyncState;
            int read = ctx.sock.EndReceive(ar);

            //Handle
            try
            {
                OnClientCommand(ctx, ctx.buffer, read);
            } catch (Exception ex)
            {
                //Unknown error. Close connection
                ctx.sock.Close();
                OnClientClosed(ctx);
                return;
            }

            //Listen for next
            ctx.sock.BeginReceive(ctx.buffer, 0, ctx.buffer.Length, SocketFlags.None, OnClientReceiveData, ctx);
        }

        private void OnClientCommand(NetIQServerClient ctx, byte[] data, int len)
        {
            //Get opcode
            NetIQOpcode op = (NetIQOpcode)BitConverter.ToUInt16(data, 0);

            //Switch
            switch(op)
            {
                case NetIQOpcode.OPEN_STREAM:
                    //Decode command
                    NetIQCommandOpenStream cmd = new NetIQCommandOpenStream(data);

                    //Get endpoint to send to 
                    IPAddress addr = ((IPEndPoint)ctx.sock.RemoteEndPoint).Address;
                    IPEndPoint ep = new IPEndPoint(addr, cmd.StreamPort);

                    //Make stream
                    NetIQServerStream stream = new NetIQServerStream(ep, cmd.BufferSize, cmd.SampleFormat);

                    //Apply to list
                    lock(streams)
                    {
                        //Remove existing stream, if any
                        if (ctx.stream != null)
                            streams.Remove(ctx.stream);

                        //Add
                        streams.Add(stream);
                        ctx.stream = stream;
                    }
                    break;
                default:
                    throw new Exception("Unknown opcode!");
            }
        }

        private void OnClientClosed(NetIQServerClient ctx)
        {
            //Remove existing stream, if any
            if(ctx.stream != null)
            {
                lock (streams)
                    streams.Remove(ctx.stream);
            }
        }

        public unsafe void SendSamples(Complex* ptr, int count)
        {
            lock(streams)
            {
                foreach (var s in streams)
                    s.Write(ptr, count);
            }
        }
    }
}
