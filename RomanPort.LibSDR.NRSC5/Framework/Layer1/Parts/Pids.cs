using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.Layer1.Parts
{
    delegate void PidsBitsEventArgs(byte[] bits);
    
    unsafe class Pids : Nrsc5Layer1Part
    {
        public event PidsBitsEventArgs OnPidsFrame;

        public void pids_frame_push(byte[] bits)
        {
            int i;
            byte[] reversed = new byte[PIDS_FRAME_LEN];

            for (i = 0; i < PIDS_FRAME_LEN; i++)
            {
                reversed[i] = bits[((i >> 3) << 3) + 7 - (i & 7)];
            }
            if (check_crc12(reversed, out ushort crcCalculated, out ushort crcExpected))
            {
                OnPidsFrame?.Invoke(reversed);
            }
        }

        static ushort crc12(byte[] bits)
        {
            ushort poly = 0xD010;
            ushort reg = 0x0000;
            int i, lowbit;

            for (i = 67; i >= 0; i--)
            {
                lowbit = reg & 1;
                reg >>= 1;
                reg ^= (ushort)(bits[i] << 15);
                if (lowbit != 0) reg ^= poly;
            }
            for (i = 0; i < 16; i++)
            {
                lowbit = reg & 1;
                reg >>= 1;
                if (lowbit != 0) reg ^= poly;
            }
            reg ^= 0x955;
            return (ushort)(reg & 0xfff);
        }

        static bool check_crc12(byte[] bits, out ushort calculated, out ushort expected)
        {
            expected = 0;
            int i;

            for (i = 68; i < 80; i++)
            {
                expected <<= 1;
                expected |= (ushort)bits[i];
            }
            calculated = crc12(bits);
            return expected == calculated;
        }
    }
}
