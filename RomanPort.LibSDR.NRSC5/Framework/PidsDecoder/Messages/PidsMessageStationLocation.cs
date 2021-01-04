using RomanPort.LibSDR.NRSC5.Framework.Layer1.Util;
using RomanPort.LibSDR.NRSC5.Framework.PidsDecoder.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.PidsDecoder.Messages
{
    public class PidsMessageStationLocation : PidsMessageBase
    {
        public float pos;
        public float altitude; //in meters
        public bool isLatitude; //If false, this is longitude instead
        
        public override void Decode(PidsReader reader)
        {
            isLatitude = reader.ReadBitBool();
            pos = (float)reader.ReadInt(22) / 8192f; //Converts this to a float
            altitude = (float)reader.ReadUInt(4) * 16;
        }

        public override void ProcessParser(PidsParser parser, PidsMessageBaseContext context)
        {
            //Get real context
            var ctx = context.GetContext(new LocationContext());

            //Apply
            if (isLatitude)
                ctx.latitude = pos;
            else
                ctx.longitude = pos;

            //If complete, update
            if (ctx.longitude.HasValue && ctx.latitude.HasValue)
                parser.StationLocation = new PidsLocationData
                {
                    Altitude = altitude,
                    Latitude = ctx.latitude.Value,
                    Longitude = ctx.longitude.Value
                };
        }

        class LocationContext
        {
            public float? latitude;
            public float? longitude;
        }
    }
}
