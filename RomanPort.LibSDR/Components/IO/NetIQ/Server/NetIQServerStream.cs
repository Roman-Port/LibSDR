using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace RomanPort.LibSDR.Components.IO.NetIQ.Server
{
    public unsafe class NetIQServerStream
    {
        public NetIQServerStream(IPEndPoint endpoint, int bufferSize, NetIQSampleFormat format)
        {
            //Set
            this.endpoint = endpoint;
            this.bufferSize = bufferSize;
            this.format = format;

            //Validate
            if (endpoint.Port < 9000)
                throw new Exception("Port must be >= 9000!");
            if (bufferSize < 64 || bufferSize > NetIQUtil.GetMaxBufferSize(format))
                throw new Exception("Buffer size is invalid.");
            if (format != NetIQSampleFormat.Byte && format != NetIQSampleFormat.Short && format != NetIQSampleFormat.Float)
                throw new Exception("Invalid format.");

            //Make socket
            sock = new Socket(SocketType.Dgram, ProtocolType.Udp);

            //Create buffer and get pointers to it
            buffer = new byte[((int)format / 8) * 2 * bufferSize];
            bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            bufferPtrByte = (byte*)bufferHandle.AddrOfPinnedObject();
            bufferPtrShort = (short*)bufferHandle.AddrOfPinnedObject();
            bufferPtrFloat = (float*)bufferHandle.AddrOfPinnedObject();
        }

        private IPEndPoint endpoint;
        private int bufferSize;
        private NetIQSampleFormat format;

        private Socket sock;
        private int bufferUsage;

        private byte[] buffer;
        private GCHandle bufferHandle;
        private byte* bufferPtrByte;
        private short* bufferPtrShort;
        private float* bufferPtrFloat;

        public void Write(Complex* ptr, int count)
        {
            while(count > 0)
            {
                //Write to buffer
                int index = bufferUsage++ * 2;
                switch (format)
                {
                    case NetIQSampleFormat.Float:
                        bufferPtrFloat[index++] = ptr->Real;
                        bufferPtrFloat[index++] = ptr->Imag;
                        break;
                    case NetIQSampleFormat.Short:
                        bufferPtrShort[index++] = (short)(ptr->Real * short.MaxValue);
                        bufferPtrShort[index++] = (short)(ptr->Imag * short.MaxValue);
                        break;
                    case NetIQSampleFormat.Byte:
                        bufferPtrByte[index++] = (byte)((ptr->Real * 127.5f) + 128);
                        bufferPtrByte[index++] = (byte)((ptr->Imag * 127.5f) + 128);
                        break;
                }

                //Update state
                ptr++;
                count--;

                //Check if the buffer is full
                if(bufferUsage == bufferSize)
                {
                    //Send on wire
                    int sent = sock.SendTo(buffer, buffer.Length, SocketFlags.None, endpoint);
                    if (buffer.Length != sent)
                        throw new Exception("Unexpected number of bytes sent!");

                    //Reset state
                    bufferUsage = 0;
                }
            }
        }
    }
}
