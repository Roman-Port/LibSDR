using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.IO.NetIQ
{
    public class NetIQUtil
    {
        public const int PROTO_VERSION_MAJOR = 0;
        public const int PROTO_VERSION_MINOR = 1;

        public static int GetMaxBufferSize(NetIQSampleFormat fmt)
        {
            int bytesPerSample = ((int)fmt / 8) * 2;
            return 65500 / bytesPerSample; //65527 is the max number of bytes a UDP frame can hold. We round that down a tad bit
        }
    }
}
