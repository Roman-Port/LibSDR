using RomanPort.LibSDR.NRSC5.Framework.Layer1.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.PidsDecoder.Messages
{
    public class PidsMessageStationNameShort : PidsMessageBase
    {
        public char[] callsign;
        public string suffix {
            get
            {
                if (extension == StationNameExtension.APPEND_FM)
                    return "-FM";
                return "";
            }
        }
        public string name { get => new string(callsign) + suffix; }
        public StationNameExtension extension;

        public override void Decode(PidsReader reader)
        {
            callsign = reader.ReadChar5Array(4);
            extension = (StationNameExtension)reader.ReadUInt(2);
        }

        public override void ProcessParser(PidsParser parser, PidsMessageBaseContext context)
        {
            parser.StationId = new Data.PidsStationIdData(this);
        }

        public enum StationNameExtension : byte
        {
            NO_EXTENSION = 0b00,
            APPEND_FM = 0b01,
            //Reserved
            //Reserved
        }
    }
}
