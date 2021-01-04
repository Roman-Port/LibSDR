using RomanPort.LibSDR.NRSC5.Framework.Layer1.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.PidsDecoder.Messages
{
    /// <summary>
    /// NOTE: This relies on other frames to form a full message.
    /// NOTE: The NRSC5 docs claim that this is depreciated and should no longer be used.
    /// </summary>
    public class PidsMessageStationNameLong : PidsMessageBase
    {
        public byte lastFrameNumber;
        public byte currentFrameNumber;
        public char[] stationNameChunk; //7 chars
        public byte sequenceNumber;
        
        public override void Decode(PidsReader reader)
        {
            lastFrameNumber = (byte)reader.ReadUInt(3);
            currentFrameNumber = (byte)reader.ReadUInt(3);
            stationNameChunk = reader.ReadChar7Array(7);
            sequenceNumber = (byte)reader.ReadUInt(3);
        }

        public override void ProcessParser(PidsParser parser, PidsMessageBaseContext context)
        {
            //This is depreciated, so it is not supported
        }
    }
}
