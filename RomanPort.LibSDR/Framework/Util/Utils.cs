using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RomanPort.LibSDR.Framework.Util
{
    public unsafe static class Utils
    {
        public static void ManagedMemcpy(void* dest, void* src, int len)
        {
            var d = (byte*)dest;
            var s = (byte*)src;
            if (len >= 16)
            {
                do
                {
                    ((int*)d)[0] = ((int*)s)[0];
                    ((int*)d)[1] = ((int*)s)[1];
                    ((int*)d)[2] = ((int*)s)[2];
                    ((int*)d)[3] = ((int*)s)[3];
                    d += 16;
                    s += 16;
                } while ((len -= 16) >= 16);
            }
            if (len > 0)
            {
                if ((len & 8) != 0)
                {
                    ((int*)d)[0] = ((int*)s)[0];
                    ((int*)d)[1] = ((int*)s)[1];
                    d += 8;
                    s += 8;
                }
                if ((len & 4) != 0)
                {
                    ((int*)d)[0] = ((int*)s)[0];
                    d += 4;
                    s += 4;
                }
                if ((len & 2) != 0)
                {
                    ((short*)d)[0] = ((short*)s)[0];
                    d += 2;
                    s += 2;
                }
                if ((len & 1) != 0)
                    *d = *s;
            }
        }

        private const string Libc = "msvcrt.dll";

        [DllImport(Libc, EntryPoint = "memmove", CallingConvention = CallingConvention.Cdecl)]
        public static extern void* Memmove(void* dest, void* src, int len);

        [DllImport(Libc, EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl)]
        public static extern void* Memcpy(void* dest, void* src, int len);

        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
        public static extern uint TimeBeginPeriod(uint uMilliseconds);

        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]
        public static extern uint TimeEndPeriod(uint uMilliseconds);
    }
}
