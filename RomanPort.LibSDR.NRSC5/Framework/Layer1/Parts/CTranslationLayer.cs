using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.Layer1.Parts
{
    public unsafe class CTranslationLayer
    {
        public const int SAMPLE_RATE = 1488375;
        // FFT length in samples
        public const int FFT = 2048;
        // cyclic preflex length in samples
        public const int CP = 112;
        public const int FFTCP = (FFT + CP);
        // OFDM symbols per L1 block
        public const int BLKSZ = 32;
        // symbols processed by each invocation of acquire_process
        public const int ACQUIRE_SYMBOLS = (BLKSZ * 2);
        // index of first lower sideband subcarrier
        public const int LB_START = ((FFT / 2) - 546);
        // index of last upper sideband subcarrier
        public const int UB_END = ((FFT / 2) + 546);
        // bits per P1 frame
        public const int P1_FRAME_LEN = 146176;
        // bits per encoded P1 frame
        public const int P1_FRAME_LEN_ENCODED = (P1_FRAME_LEN * 5 / 2);
        // bits per PIDS frame
        public const int PIDS_FRAME_LEN = 80;
        // bits per encoded PIDS frame
        public const int PIDS_FRAME_LEN_ENCODED = (PIDS_FRAME_LEN * 5 / 2);
        // bits per P3 frame
        public const int P3_FRAME_LEN = 4608;
        // bits per encoded P3 frame
        public const int P3_FRAME_LEN_ENCODED = (P3_FRAME_LEN * 2);
        // bits per L2 PCI
        public const int PCI_LEN = 24;
        // bytes per L2 PDU (max)
        public const int MAX_PDU_LEN = ((P1_FRAME_LEN - PCI_LEN) / 8);
        // number of programs (max)
        public const int MAX_PROGRAMS = 8;

        public static readonly Complex I = new Complex(0, 1);
        public const float M_PI = 3.14159265358979323846f;

        public static void memset(void* ptr, byte value, int size)
        {
            byte* bPtr = (byte*)ptr;
            for (int i = 0; i < size; i++)
                bPtr[i] = value;
        }

        public static void memset(byte[] ptr, byte value, int size)
        {
            for (int i = 0; i < size; i++)
                ptr[i] = value;
        }

        public static void memmove(void* dst, void* src, int count)
        {
            Utils.Memcpy(dst, src, count);
        }

        public float normf(Complex v)
        {
            float realf = v.Real;
            float imagf = v.Imag;
            return realf * realf + imagf * imagf;
        }

        public Complex conjf(Complex v)
        {
            return new Complex(v.Real, -v.Imag);
        }

        public float cargf(Complex v)
        {
            return v.Argument();
        }

        public Complex cexpf(Complex v)
        {
            float temp_factor = (float)Math.Exp(v.Real);
            float result_re = temp_factor * (float)Math.Cos(v.Imag);
            float result_im = temp_factor * (float)Math.Sin(v.Imag);
            return new Complex(result_re, result_im);
        }

        public float cabsf(Complex v)
        {
            float c = Math.Abs(v.Real);
            float d = Math.Abs(v.Imag);

            if (c > d)
            {
                double r = d / c;
                return c * (float)Math.Sqrt(1.0 + r * r);
            }
            else if (d == 0.0)
            {
                return c;  // c is either 0.0 or NaN
            }
            else
            {
                double r = c / d;
                return d * (float)Math.Sqrt(1.0 + r * r);
            }
        }

        public float roundf(float f)
        {
            return (float)Math.Round(f);
        }

        public Complex CMPLXF(float x, float y)
        {
            //return new Complex(x, y);
            return x + I * y;
        }

        public static int __builtin_parity(int x)
        {
            int parity = 0;
            while (x != 0)
            {
                parity ^= x;
                x >>= 1;
            }
            return (parity & 0x1);
        }

        public static int __builtin_parity(uint x)
        {
            uint parity = 0;
            while (x != 0)
            {
                parity ^= x;
                x >>= 1;
            }
            return ((int)(parity & 0x1));
        }

        public static bool __builtin_parity_bool(int n)
        {
            return __builtin_parity(n) != 0;
        }

        public static bool __builtin_parity_bool(uint n)
        {
            return __builtin_parity(n) != 0;
        }

        public static int memcmp(void* a, void* b, int num)
        {
            return memcmp((byte*)a, (byte*)b, num);
        }

        public static int memcmp(byte* a, byte* b, int num)
        {
            for(int i = 0; i<num; i++)
            {
                int c = a[i] - b[i];
                if (c != 0)
                    return c;
            }
            return 0;
        }

        public enum SYNC_STATE
        {
            SYNC_STATE_NONE,
            SYNC_STATE_COARSE,
            SYNC_STATE_FINE
        }
    }
}
