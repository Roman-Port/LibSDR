using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Misc
{
    public struct SnrReading
    {
        public float floor;
        public float ceiling;
        public float snr;

        public SnrReading(float floor, float ceiling)
        {
            this.floor = floor;
            this.ceiling = ceiling;
            this.snr = ceiling - floor;
        }
    }
}
