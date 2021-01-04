using RomanPort.LibSDR.Framework.Util;
using RomanPort.LibSDR.NRSC5.Framework.AudioDecoder.FAAD2.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.AudioDecoder.FAAD2
{
    /// <summary>
    /// Imports FAAD2 as a DLL. Will download it if we can
    /// </summary>
    public class Faad2AudioDecoder : IAudioDecoder
    {
        public Faad2AudioDecoder()
        {
            //Make sure the DLL exists
            if (!File.Exists(DLL_NAME))
                throw new DllNotFoundException($"FAAD2 was not found at \"{DLL_NAME}\". Obtain it manually, or call Faad2AudioDecoder.DownloadLibrary() to automatically obtain it.");
        }

        /// <summary>
        /// Downloads the library (or uses a cached version) and then creates the decoder. You can call this as many times as you'd like, as the lib is downloaded after the first time.
        /// </summary>
        /// <returns></returns>
        public static Faad2AudioDecoder DownloadLibrary()
        {
            //Download it if it it wasn't found
            if(!File.Exists(DLL_NAME))
            {
                //Identify platform
                throw new NotImplementedException();
            }

            //Prepare
            return new Faad2AudioDecoder();
        }

        private const string DLL_NAME = "lib_faad2.dll";

        private IntPtr ctx;
        
        public void CloseDecoder()
        {
            throw new NotImplementedException();
        }

        public unsafe void OpenDecoder(long samplerate)
        {
            fixed(IntPtr* ptr = &ctx)
                NeAACDecInitHDC(ptr, &samplerate);
        }

        public unsafe int Process(byte* compressed, int compressedCount, short* outBuffer, int outBufferCount)
        {
            //Allocate memory for our info struct
            UnsafeBuffer infoP = UnsafeBuffer.Create(1, sizeof(NeAACDecFrameInfo));
            NeAACDecFrameInfo* info = (NeAACDecFrameInfo*)infoP;

            //Process
            short* data = NeAACDecDecode(ctx, (IntPtr)infoP.Address, compressed, compressedCount);

            //Read what we need from the struct and then clean up
            int error = info->error;
            int samples = info->samples;
            infoP.Dispose();
            info = null;

            //Validate that there were no errors
            if (error > 0)
                throw new Exception($"FAAD reported an error while decoding: " + NeAACDecGetErrorMessageManaged(info->error));

            //Validate we have space to copy
            if (samples > outBufferCount)
                throw new Exception($"Output buffer was not large enough for the number of incoming samples, {info->samples}!");

            //Copy
            Utils.Memcpy(outBuffer, data, samples * sizeof(short));

            return samples;
        }

        private static unsafe string NeAACDecGetErrorMessageManaged(byte error)
        {
            //Get string pointer
            char* ptr = NeAACDecGetErrorMessage((char)error);

            //Marshal
            return Marshal.PtrToStringAnsi((IntPtr)ptr);
        }

        //DLL IMPORTS
        [DllImport(DLL_NAME)]
        private static extern unsafe char NeAACDecInitHDC(IntPtr* ptr, long* samplerate);
        [DllImport(DLL_NAME)]
        private static extern unsafe char* NeAACDecGetErrorMessage(char error);
        [DllImport(DLL_NAME)]
        private static extern unsafe short* NeAACDecDecode(IntPtr ctx, IntPtr info, byte* buffer, long bufferSize);
    }
}
