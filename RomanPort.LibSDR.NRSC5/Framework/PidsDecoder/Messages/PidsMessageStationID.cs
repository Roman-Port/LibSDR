using RomanPort.LibSDR.NRSC5.Framework.Layer1.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.PidsDecoder.Messages
{
    public class PidsMessageStationID : PidsMessageBase
    {
        public char[] countryCode;
        public uint facilityId;

        public override void Decode(PidsReader reader)
        {
            countryCode = reader.ReadChar5Array(2);
            reader.SkipBits(3); //Reserved
            facilityId = reader.ReadUInt(19);
        }

        public override void ProcessParser(PidsParser parser, PidsMessageBaseContext context)
        {
            parser.StationCountry = new string(countryCode);
            parser.StationFacility = facilityId;
        }
    }
}
