using RomanPort.LibSDR.NRSC5.Framework.Layer1.Frames;
using RomanPort.LibSDR.NRSC5.Framework.Layer1.Util;
using RomanPort.LibSDR.NRSC5.Framework.PidsDecoder.Data;
using RomanPort.LibSDR.NRSC5.Framework.PidsDecoder.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.PidsDecoder
{
    public class PidsParser
    {
        public PidsParser()
        {
            pidsContexts = new Dictionary<PidsMessageOpcode, PidsMessageBaseContext>();
        }

        private Dictionary<PidsMessageOpcode, PidsMessageBaseContext> pidsContexts; //Passed to each message when we handle it for storing data cross-frames. Unique to each opcode

        private PidsStationIdData? _stationId;
        private uint? _stationFacility;
        private string _stationCountry;
        private string _stationMessage;
        private PidsLocationData? _stationLocation;

        public PidsStationIdData? StationId { get => _stationId; set { _stationId = value; if (value.HasValue) { OnStationCallsignUpdated?.Invoke(this, value.Value); } } }
        public uint? StationFacility { get => _stationFacility; set { _stationFacility = value; if (value.HasValue) { OnStationFacilityUpdated?.Invoke(this, value.Value); } } }
        public string StationCountry { get => _stationCountry; set { _stationCountry = value; OnStationCountryUpdated?.Invoke(this, value); } }
        public PidsLocationData? StationLocation { get => _stationLocation; set { _stationLocation = value; if (value.HasValue) { OnStationLocationUpdated?.Invoke(this, value.Value); } } }
        public string StationMessage { get => _stationMessage; set { _stationMessage = value; OnStationMessageUpdated?.Invoke(this, value); } }

        public event PidsParserUpdatedEvent<PidsStationIdData> OnStationCallsignUpdated;
        public event PidsParserUpdatedEvent<uint> OnStationFacilityUpdated;
        public event PidsParserUpdatedEvent<string> OnStationCountryUpdated;
        public event PidsParserUpdatedEvent<PidsLocationData> OnStationLocationUpdated;
        public event PidsParserUpdatedEvent<string> OnStationMessageUpdated;

        public void Process(FramePids frame)
        {
            //Open reader
            var reader = frame.GetReader();

            //Validate
            if (reader.ReadBitBool() != false)
                return;

            //Read payloads count
            int payloadsCount = reader.ReadBit() + 1;

            //Read payloads
            for (int i = 0; i < payloadsCount; i++)
            {
                if (!ReadPayload(reader))
                    return; //Since we didn't determine the type of message, we can't read the next one because we don't know the length
            }
        }

        private bool ReadPayload(PidsReader reader)
        {
            //Get PIDS opcode
            PidsMessageOpcode opcode = (PidsMessageOpcode)reader.ReadUInt(4);

            //Get a PIDS message from the opcode
            PidsMessageBase msg = GetPidsMessageFromOpcode(opcode);
            if (msg == null)
                return false;

            //Decode
            msg.Decode(reader);

            //Get context
            if (!pidsContexts.ContainsKey(opcode))
                pidsContexts.Add(opcode, new PidsMessageBaseContext());
            PidsMessageBaseContext msgContext = pidsContexts[opcode];

            //Process
            msg.ProcessParser(this, msgContext);

            return true;
        }

        public static PidsMessageBase GetPidsMessageFromOpcode(PidsMessageOpcode op)
        {
            switch(op)
            {
                case PidsMessageOpcode.STATION_ID: return new PidsMessageStationID();
                case PidsMessageOpcode.STATION_NAME_SHORT: return new PidsMessageStationNameShort();
                case PidsMessageOpcode.STATION_NAME_LONG: return new PidsMessageStationNameLong();
                case PidsMessageOpcode.STATION_LOCATION: return new PidsMessageStationLocation();
                case PidsMessageOpcode.STATION_MESSAGE: return new PidsMessageStationMsg();
            }
            return null;
        }
    }
}
