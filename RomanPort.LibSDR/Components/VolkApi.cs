using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RomanPort.LibSDR.Components
{
    public unsafe static class VolkApi
    {
        private const string DLL_NAME = "libvolk.so";
        private const int MIN_VOLK_LIBSDR_VERSION = 2;

        public static readonly bool volkSupported;
        public static bool showVolkWarning = true;

        public static void WarnVolk()
        {
            if (showVolkWarning && !volkSupported)
                Console.WriteLine("LibSDR: VOLK is not currently being used. While it isn't required, VOLK will immensely speed up filtering. It is highly recommended. To disable this warning, set RomanPort.LibSDR.Components.VolkApi.showVolkWarning to false.");
            showVolkWarning = false;
        }

        static VolkApi()
        {
            int version;
            try
            {
                version = libsdr_version();
            }
            catch
            {
                volkSupported = false;
                return;
            }
            volkSupported = version >= MIN_VOLK_LIBSDR_VERSION;
            if (!volkSupported)
                throw new Exception($"LibSDR VOLK was detected, but is running an outdated version ({version}). Please upgrade it to >={MIN_VOLK_LIBSDR_VERSION} or remove it.");
        }

        [DllImport(DLL_NAME)]
        private static extern int libsdr_version();

        #region libsdr_filter_real

        [DllImport(DLL_NAME)]
        public static extern libsdr_filter_real_data* libsdr_filter_real_create(int taps, int decimation);
        [DllImport(DLL_NAME)]
        public static extern int libsdr_filter_real_process(libsdr_filter_real_data* ctx, float* input, float* output, int channels, int count);
        [DllImport(DLL_NAME)]
        public static extern void libsdr_filter_real_free(libsdr_filter_real_data* ctx);

        [StructLayout(LayoutKind.Sequential)]
        public struct libsdr_filter_real_data
        {
            public float* tempBufferPtr;
            public float* coeffsBufferPtr;
            public float* insampBufferPtr;
            public float* insampBufferPtrOffset;

            public int taps;
            public int decimation;
            public int decimationIndex;
            public int offset;
        }

        #endregion

        #region libsdr_filter_complex

        [DllImport(DLL_NAME)]
        public static extern libsdr_filter_complex_data* libsdr_filter_complex_create(int taps, int decimation);
        [DllImport(DLL_NAME)]
        public static extern int libsdr_filter_complex_process(libsdr_filter_complex_data* ctx, Complex* input, Complex* output, int channels, int count);
        [DllImport(DLL_NAME)]
        public static extern void libsdr_filter_complex_free(libsdr_filter_complex_data* ctx);

        [StructLayout(LayoutKind.Sequential)]
        public struct libsdr_filter_complex_data
        {
            public Complex* tempBufferPtr;
            public Complex* coeffsBufferPtr;
            public Complex* insampBufferPtr;
            public Complex* insampBufferPtrOffset;

            public int taps;
            public int decimation;
            public int decimationIndex;
            public int offset;
        }

        #endregion
    }
}
