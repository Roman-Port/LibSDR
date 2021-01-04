using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.AudioDecoder.FAAD2.Entities
{
    [StructLayout(LayoutKind.Sequential, Size= 86)]
    public unsafe struct NeAACDecFrameInfo
    {
        public int bytesconsumed;
        public int samples;
        public byte channels;
        public byte error;
        public int samplerate;
        public byte sbr;
        public byte object_type;
        public byte header_type;
        public byte num_front_channels;
        public byte num_side_channels;
        public byte num_back_channels;
        public byte num_lfe_channels;
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        //public byte[] channel_position;
        //public byte ps;
    }
}
