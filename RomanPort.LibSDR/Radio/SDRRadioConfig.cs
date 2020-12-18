using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Radio
{
    public class SDRRadioConfig
    {
        /// <summary>
        /// Higher buffer sizes are better.
        /// </summary>
        public int BufferSize;

        /// <summary>
        /// If set to true, the radio will be throttled to the sample rate. If it's not set to true, it will run at maximum speed. Hardware sources are unaffected.
        /// </summary>
        public bool IsRealtime = false;
    }
}
