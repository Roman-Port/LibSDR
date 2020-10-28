using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR
{
    public class SDRRadioConfig
    {
        /// <summary>
        /// Internal buffer size used for all calculations
        /// </summary>
        public int bufferSize = 4096;

        /// <summary>
        /// Output demodulated audio rate
        /// </summary>
        public float outputAudioSampleRate = 48000;

        /// <summary>
        /// When true, throttles the incoming data so it's forced to run in realtime. Caps this at 1x speed
        /// </summary>
        public bool realtime = false;
    }
}
