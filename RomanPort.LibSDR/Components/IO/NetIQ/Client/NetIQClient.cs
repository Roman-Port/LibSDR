using RomanPort.LibSDR.Components.IO.NetIQ.Commands;
using RomanPort.LibSDR.Sources;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace RomanPort.LibSDR.Components.IO.NetIQ.Client
{
    public unsafe delegate void NetIQClient_OnSamples(NetIQClient ctx, Complex* ptr, int count);
    public unsafe delegate void NetIQClient_OnSampleRateChanged(NetIQClient ctx, uint sampleRate);

    public unsafe class NetIQClient
    {
        public NetIQClient(IPEndPoint endpoint)
        {
            this.endpoint = endpoint;
            buffer = new byte[4096];
        }

        public event NetIQClient_OnSamples OnSamples;
        public event NetIQClient_OnSampleRateChanged OnSampleRateChanged;

        private IPEndPoint endpoint;
        private Socket sock;
        private byte[] buffer;

        private Socket stream;
        private UnsafeBuffer sampleBuffer;
        private Complex* sampleBufferPtr;
        private NetIQSampleFormat streamSampleFormat;

        private byte[] streamBuffer;
        private GCHandle streamBufferHandle;
        private byte* streamBufferPtrByte;
        private short* streamBufferPtrShort;
        private float* streamBufferPtrFloat;

        public void Connect()
        {
            sock = new Socket(SocketType.Stream, ProtocolType.Tcp);
            sock.Connect(endpoint);
            sock.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, OnMessage, null);
        }

        public void OpenStream(ushort bufferSize, NetIQSampleFormat format)
        {
            //Validate
            int maxBufferSize = NetIQUtil.GetMaxBufferSize(format);
            if (bufferSize > maxBufferSize)
                throw new Exception($"Max buffer size is {maxBufferSize}. Please lower the buffer size or make the sample format smaller.");

            //Set
            streamSampleFormat = format;
            
            //Open the incoming buffer
            streamBuffer = new byte[((int)format / 8) * 2 * bufferSize];
            streamBufferHandle = GCHandle.Alloc(streamBuffer, GCHandleType.Pinned);
            streamBufferPtrByte = (byte*)streamBufferHandle.AddrOfPinnedObject();
            streamBufferPtrShort = (short*)streamBufferHandle.AddrOfPinnedObject();
            streamBufferPtrFloat = (float*)streamBufferHandle.AddrOfPinnedObject();

            //Open the outgoing buffer
            sampleBuffer = UnsafeBuffer.Create(bufferSize, out sampleBufferPtr);

            //Get port
            int port = endpoint.Port + 1;

            //Open socket
            stream = new Socket(SocketType.Dgram, ProtocolType.Udp);
            stream.Bind(new IPEndPoint(IPAddress.Any, port));
            stream.BeginReceive(streamBuffer, 0, streamBuffer.Length, SocketFlags.None, OnStreamPacket, null);

            //Send command to start streaming
            NetIQCommandOpenStream cmd = new NetIQCommandOpenStream();
            cmd.StreamPort = (ushort)port;
            cmd.BufferSize = bufferSize;
            cmd.SampleFormat = format;
            cmd.SendOnSocket(sock);
        }

        private void OnMessage(IAsyncResult ar)
        {
            //Get data
            int count = sock.EndReceive(ar);

            //Get opcode
            NetIQOpcode op = (NetIQOpcode)BitConverter.ToUInt16(buffer, 0);

            //Handle
            switch(op)
            {
                case NetIQOpcode.SERVER_INFO:
                    var cmd = new NetIQCommandServerInfo(buffer);
                    OnSampleRateChanged?.Invoke(this, cmd.SampleRate);
                    break;
            }

            //Listen
            sock.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, OnMessage, null);
        }

        private void OnStreamPacket(IAsyncResult ar)
        {
            //Get data
            int count = stream.EndReceive(ar);
            int samples = count / (int)streamSampleFormat;

            //Convert samples
            float* ptr = (float*)sampleBufferPtr;
            switch (streamSampleFormat)
            {
                case NetIQSampleFormat.Float:
                    //This is a float. We can directly copy the samples
                    Utils.Memcpy(ptr, streamBufferPtrFloat, samples * sizeof(float));
                    break;
                case NetIQSampleFormat.Short:
                    //This is a short. We'll use the lookup table
                    for (int i = 0; i < samples; i++)
                    {
                        //To conserve space, we only store the positive values in the lookup table. Flip the sign if it is negative
                        if (streamBufferPtrShort[i] >= 0)
                            ptr[i] = ConversionLookupTable.LOOKUP_INT16[streamBufferPtrShort[i]];
                        else
                            ptr[i] = -ConversionLookupTable.LOOKUP_INT16[-streamBufferPtrShort[i]];
                    }
                    break;
                case NetIQSampleFormat.Byte:
                    //This is a byte. Use our lookup table
                    for (int i = 0; i < samples; i++)
                        ptr[i] = ConversionLookupTable.LOOKUP_INT8[streamBufferPtrByte[i]];
                    break;
                default:
                    throw new Exception("Unknown data format.");
            }

            //Send events
            OnSamples?.Invoke(this, sampleBufferPtr, samples / 2);

            //Continue reading
            stream.BeginReceive(streamBuffer, 0, streamBuffer.Length, SocketFlags.None, OnStreamPacket, null);
        }
    }
}
