using RomanPort.LibSDR.NRSC5.Framework.PidsDecoder.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.PidsDecoder.Data
{
    public struct PidsStationIdData
    {
        public string FullName { get => Prefix + CallLetters + Suffix; }
        public string CallLetters { get; private set; }
        public string Prefix { get => ""; }
        public string Suffix { get; private set; }

        public PidsStationIdData(PidsMessageStationNameShort msg)
        {
            CallLetters = new string(msg.callsign);
            Suffix = msg.suffix;
        }
    }
}
