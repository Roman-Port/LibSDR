using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Sources.Hardware.AirSpy.Internal
{
    public static class ConversionFilters
    {
        private static readonly float[] Kernel_Dec16_110dB = new float[7]
        {
            -0.03183508f,
            0.0f,
            0.2818315f,
            0.5000073f,
            0.2818315f,
            0.0f,
            -0.03183508f
        };
        private static readonly float[] Kernel_Dec8_100dB = new float[11]
        {
            0.006633401f,
            0.0f,
            -0.05103552f,
            0.0f,
            0.2944033f,
            0.4999975f,
            0.2944033f,
            0.0f,
            -0.05103552f,
            0.0f,
            0.006633401f
        };
        private static readonly float[] Kernel_Dec4_90dB = new float[15]
        {
            -0.002474189f,
            0.0f,
            0.01696575f,
            0.0f,
            -0.0676806f,
            0.0f,
            0.3031806f,
            0.500017f,
            0.3031806f,
            0.0f,
            -0.0676806f,
            0.0f,
            0.01696575f,
            0.0f,
            -0.002474189f
        };
        private static readonly float[] Kernel_Dec2_80dB = new float[47]
        {
            -0.0001980066f,
            0.0f,
            0.0005768538f,
            0.0f,
            -0.001352191f,
            0.0f,
            0.002729177f,
            0.0f,
            -0.004988194f,
            0.0f,
            0.008499503f,
            0.0f,
            -0.01378858f,
            0.0f,
            0.02171314f,
            0.0f,
            -0.03398001f,
            0.0f,
            0.05494487f,
            0.0f,
            -0.1006575f,
            0.0f,
            0.3164574f,
            0.5f,
            0.3164574f,
            0.0f,
            -0.1006575f,
            0.0f,
            0.05494487f,
            0.0f,
            -0.03398001f,
            0.0f,
            0.02171314f,
            0.0f,
            -0.01378858f,
            0.0f,
            0.008499503f,
            0.0f,
            -0.004988194f,
            0.0f,
            0.002729177f,
            0.0f,
            -0.001352191f,
            0.0f,
            0.0005768538f,
            0.0f,
            -0.0001980066f
        };
        public static readonly float[][] FirKernels100dB = new float[4][]
        {
            ConversionFilters.Kernel_Dec2_80dB,
            ConversionFilters.Kernel_Dec4_90dB,
            ConversionFilters.Kernel_Dec8_100dB,
            ConversionFilters.Kernel_Dec16_110dB
        };
    }
}
