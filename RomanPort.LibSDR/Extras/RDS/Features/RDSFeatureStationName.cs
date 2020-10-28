using RomanPort.LibSDR.Extras.RDS.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Extras.RDS.Features
{
    public class RDSFeatureStationName
    {
        /// <summary>
        /// The buffer used, may be invalid
        /// </summary>
        public char[] stationNameBuffer;

        /// <summary>
        /// The full, current station name
        /// </summary>
        public string stationName;

        /// <summary>
        /// The most common event; Notifies users when a new name is fully recieved
        /// </summary>
        public event RDSFeatureStationName_StationNameUpdatedEventArgs RDSFeatureStationName_StationNameUpdatedEvent;

        /// <summary>
        /// Notifies users when a part of the buffer is updated
        /// </summary>
        public event RDSFeatureStationName_StationBufferUpdatedEventArgs RDSFeatureStationName_StationBufferUpdatedEvent;

        /// <summary>
        /// Has the first chunk of the station name been decoded?
        /// </summary>
        private bool _firstChunkDecoded;

        public RDSFeatureStationName(RDSClient session)
        {
            stationNameBuffer = new char[8];
            for (int i = 0; i < 8; i++)
                stationNameBuffer[i] = ' ';
            _firstChunkDecoded = false;
            session.RDSFrameReceivedEvent += Session_RDSFrameReceivedEvent;
            session.RDSSessionResetEvent += Session_RDSSessionResetEvent;
        }

        private void Session_RDSSessionResetEvent(RDSClient session)
        {
            //Reset
            stationName = null;
            _firstChunkDecoded = false;

            //Clear buffer
            for (int i = 0; i < 8; i++)
                stationNameBuffer[i] = ' ';

            //Send events
            RDSFeatureStationName_StationBufferUpdatedEvent?.Invoke(stationNameBuffer, 0);
        }

        private void Session_RDSFrameReceivedEvent(RDSCommand frame, RDSClient session)
        {
            //Get type
            if (frame.GetType() != typeof(BasicDataRDSCommand))
                return;
            BasicDataRDSCommand cmd = (BasicDataRDSCommand)frame;

            //Set data in buffer
            stationNameBuffer[cmd.stationNameIndex] = cmd.letterA;
            stationNameBuffer[cmd.stationNameIndex + 1] = cmd.letterB;

            //Set chunk flag
            if (cmd.stationNameIndex == 0)
                _firstChunkDecoded = true;

            //Update final station name, if any
            if (cmd.stationNameIndex == 6 && _firstChunkDecoded)
            {
                stationName = new string(stationNameBuffer);
                RDSFeatureStationName_StationNameUpdatedEvent?.Invoke(stationName);
            }

            //Send event
            RDSFeatureStationName_StationBufferUpdatedEvent?.Invoke(stationNameBuffer, cmd.stationNameIndex);
        }
    }

    public delegate void RDSFeatureStationName_StationNameUpdatedEventArgs(string name);
    public delegate void RDSFeatureStationName_StationBufferUpdatedEventArgs(char[] buffer, int updatePos);
}
