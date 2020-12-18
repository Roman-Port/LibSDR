using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RomanPort.LibSDR.UI.Framework
{
    [StructLayout(LayoutKind.Explicit, Size = 4, CharSet = CharSet.Ansi)]
    public struct UnsafeColor
    {
        [FieldOffset(3)]
        public byte a;
        [FieldOffset(2)]
        public byte r;
        [FieldOffset(1)]
        public byte g;
        [FieldOffset(0)]
        public byte b;

        public static readonly UnsafeColor WHITE = new UnsafeColor(byte.MaxValue, byte.MaxValue, byte.MaxValue);

        public UnsafeColor(byte r, byte g, byte b, byte a = byte.MaxValue)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
    }
}
