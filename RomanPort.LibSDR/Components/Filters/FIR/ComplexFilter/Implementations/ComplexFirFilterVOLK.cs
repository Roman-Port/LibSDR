using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters.FIR.ComplexFilter.Implementations
{
    public unsafe class ComplexFirFilterVOLK : IComplexFirFilter
    {
        internal ComplexFirFilterVOLK(Complex[] coeffs, int decimation)
        {
            ctx = VolkApi.libsdr_filter_complex_create(coeffs.Length, decimation);
            for (int i = 0; i < coeffs.Length; i++)
                ctx->coeffsBufferPtr[i] = coeffs[i];
            for (int i = 0; i < coeffs.Length * 2; i++)
                ctx->insampBufferPtr[i] = 0;
        }

        public int Process(Complex* input, Complex* output, int count, int channels = 1)
        {
            return VolkApi.libsdr_filter_complex_process(ctx, input, output, channels, count);
        }

        public int Process(Complex* ptr, int count, int channels = 1)
        {
            return Process(ptr, ptr, count, channels);
        }

        private VolkApi.libsdr_filter_complex_data* ctx;

        public void Dispose()
        {
            VolkApi.libsdr_filter_complex_free(ctx);
        }
    }
}
