using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RomanPort.LibSDR.Components
{
    /// <summary>
    /// Fast Functions is a method of using highly optimized (but less convenient) C compiled code instead of working with native CSharp. It is fully optional, but it makes a significant difference in speed.
    /// 
    /// This is split up into a few parts. Assuming function name YYY, we'd have multiple functions:
    /// * YYY - The only public function here. Decides which of the following to call, depending on STATUS...
    /// * YYY_Native - The C# equivalent
    /// * YYY_Extern - Calls to an external SO binary
    /// </summary>
    public static unsafe class FastFunctions
    {
        private const string EXTERN_SO = "libsdrff.so";

        private static NativeStatus status;

        static FastFunctions()
        {
            //Attempt to load the binary
            status = NativeStatus.NATIVE;
            try
            {
                //Check version and load
                if (GetExternVersion() < 3)
                    throw new Exception("Can't load LibSdrFastFunctions: The library libsdrff.so exists, but it is not up to date. Compile/obtain the latest version.");

                //OK!
                status = NativeStatus.EXTERN;
            } catch (Exception ex)
            {

            }
        }

        public static bool IsFastSupported()
        {
            return status == NativeStatus.EXTERN;
        }

        [DllImport(EXTERN_SO, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetVersion")]
        private static extern uint GetExternVersion();

        #region ApplyFilterFloat

        public static int ApplyFilterFloat(float* inPtr, float* outPtr, int count, int channels, float* coeffs, int taps, float* buffer, ref int offset, int decimation, ref int decimationIndex)
        {
            switch (status)
            {
                case NativeStatus.EXTERN: return ApplyFilterFloat_Extern(inPtr, outPtr, count, channels, coeffs, taps, buffer, ref offset, decimation, ref decimationIndex);
                default: return ApplyFilterFloat_Native(inPtr, outPtr, count, channels, coeffs, taps, buffer, ref offset, decimation, ref decimationIndex);
            }
        }

        private static int ApplyFilterFloat_Native(float* inPtr, float* outPtr, int count, int channels, float* coeffs, int taps, float* buffer, ref int offset, int decimation, ref int decimationIndex)
        {
            float* coeff;
            float* bufferOffset;
            float* coefficientsEnd = coeffs + taps;
            int processed = 0;
            float sample;
            for (int i = 0; i < count; i++)
            {
                //Write current value to the buffer
                buffer[offset] = inPtr[0];

                //Check if we should process this
                if (decimationIndex++ == 0)
                {
                    //Get ready to filter
                    coeff = coeffs;
                    bufferOffset = buffer + offset;
                    sample = 0;

                    //Perform the filtering...
                    while (bufferOffset >= buffer)
                        sample += *bufferOffset-- * *coeff++;

                    bufferOffset = buffer + taps - 1;
                    while (coeff < coefficientsEnd)
                        sample += *bufferOffset-- * *coeff++;

                    //Write sample
                    *outPtr = sample;
                    outPtr += channels;

                    //Update state
                    processed++;
                }

                //Check if state needs to be reset
                if (decimationIndex >= decimation)
                    decimationIndex = 0;

                //Reset buffer loop if we go over
                if (++offset >= taps)
                    offset = 0;

                //Update pointer
                inPtr += channels;
            }
            return processed;
        }

        [DllImport(EXTERN_SO, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ApplyFilterFloat")]
        private static extern int ApplyFilterFloat_Extern(float* inPtr, float* outPtr, int count, int channels, float* coeffs, int taps, float* buffer, ref int offset, int decimation, ref int decimationIndex);

        #endregion

        #region ApplyFilterComplex

        public static int ApplyFilterComplex(Complex* inPtr, Complex* outPtr, int count, int channels, Complex* coeffs, int tapsComplex, Complex* buffer, ref int offset, int decimation, ref int decimationIndex)
        {
            switch (status)
            {
                case NativeStatus.EXTERN: return ApplyFilterComplex_Extern(inPtr, outPtr, count, channels, coeffs, tapsComplex, buffer, ref offset, decimation, ref decimationIndex);
                default: return ApplyFilterComplex_Native(inPtr, outPtr, count, channels, coeffs, tapsComplex, buffer, ref offset, decimation, ref decimationIndex);
            }
        }

        [DllImport(EXTERN_SO, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ApplyFilterComplex")]
        private static extern int ApplyFilterComplex_Extern(Complex* inPtr, Complex* outPtr, int count, int channels, Complex* coeffs, int tapsComplex, Complex* buffer, ref int offset, int decimation, ref int decimationIndex);

        private static int ApplyFilterComplex_Native(Complex* inPtr, Complex* outPtr, int count, int channels, Complex* coeffs, int tapsComplex, Complex* buffer, ref int offset, int decimation, ref int decimationIndex)
        {
            //Enter loop
            int processed = 0;
            for (int i = 0; i < count; i++)
            {
                //Write current value to the buffer
                buffer[offset] = *inPtr;
                inPtr += channels;

                //Check if we're allowed to go. We use a switch because it will be (VERY) slightly faster
                if (decimationIndex++ == 0)
                {
                    //Reset
                    float sum00 = 0;
                    float sum01 = 0;
                    float sum10 = 0;
                    float sum11 = 0;

                    //Get pointers
                    float* fCoeffs = (float*)(coeffs + offset - 1);
                    float* fBuffer = (float*)(buffer);

                    //Process sample
                    for (int j = 0; j < tapsComplex; j += 2)
                    {
                        sum00 += fBuffer[0] * fCoeffs[0] - fBuffer[1] * fCoeffs[1];
                        sum01 += fBuffer[0] * fCoeffs[1] + fBuffer[1] * fCoeffs[0];
                        sum10 += fBuffer[2] * fCoeffs[2] - fBuffer[3] * fCoeffs[3];
                        sum11 += fBuffer[2] * fCoeffs[3] + fBuffer[3] * fCoeffs[2];
                        fCoeffs -= 4;
                        fBuffer += 4;
                    }

                    //Merge
                    outPtr->Real = sum00 + sum10;
                    outPtr->Imag = sum01 + sum11;

                    //Update state
                    processed++;
                    outPtr += channels;
                }

                //Check if state needs to be reset
                if (decimationIndex >= decimation)
                    decimationIndex = 0;

                //Reset buffer loop if we go over
                if (++offset >= tapsComplex)
                    offset = 0;
            }
            return processed;
        }

        #endregion

        enum NativeStatus
        {
            NATIVE,
            EXTERN
        }
    }
}
