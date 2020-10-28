using RomanPort.LibSDR.Extras.RDS.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Extras.RDS.Features
{
    public class RDSFeatureRadioText
    {
        /// <summary>
        /// The internal working buffer
        /// </summary>
        public char[] textBuffer;

        /// <summary>
        /// The latest full buffer
        /// </summary>
        public string radioText;

        /// <summary>
        /// The most common event. Used when we finish reading a radio text
        /// </summary>
        public event RDSFeatureRadioText_RadioTextUpdatedEventArgs RDSFeatureRadioText_RadioTextUpdatedEvent;

        /// <summary>
        /// Used when we update a segment of the buffer
        /// </summary>
        public event RDSFeatureRadioText_RadioTextBufferUpdatedEventArgs RDSFeatureRadioText_RadioTextBufferUpdatedEvent;

        /// <summary>
        /// Used when we clear the display
        /// </summary>
        public event RDSFeatureRadioText_RadioTextClearedUpdatedEventArgs RDSFeatureRadioText_RadioTextClearedUpdatedEvent;

        /// <summary>
        /// Has the first chunk of the station name been decoded?
        /// </summary>
        private bool _firstChunkDecoded;

        /// <summary>
        /// The last A/B clear flag sent. Null if unknown
        /// </summary>
        private bool? _lastClearFlag;

        public RDSFeatureRadioText(RDSClient session)
        {
            textBuffer = new char[64];
            for (var i = 0; i < textBuffer.Length; i++)
                textBuffer[i] = ' ';
            _firstChunkDecoded = false;
            _lastClearFlag = null;
            session.RDSFrameReceivedEvent += Session_RDSFrameReceivedEvent;
            session.RDSSessionResetEvent += Session_RDSSessionResetEvent;
        }

        public static bool TryGetTrackInfo(string rt, out string trackTitle, out string trackArtist, out string stationName)
        {
            //Set outputs to null
            trackTitle = null;
            trackArtist = null;
            stationName = null;
            
            //Try to identify the type
            bool cumulusMedia = rt.Contains(" - ") && rt.Contains(" On "); //Nothing Else Matters - METALLICA On KQRS
            bool entercom = rt.StartsWith("Now playing ") && rt.Contains(" by ") && rt.Contains(" on "); //Now playing Hysteria by Def Leppard on JACK FM

            //If we activated multiple, then abort
            if (cumulusMedia && entercom)
                return false;

            //Run
            if(cumulusMedia)
            {
                //Get segments
                int segArtist = rt.LastIndexOf(" - ");
                int segStation = rt.LastIndexOf(" On ");

                //Pull info
                trackTitle = rt.Substring(0, segArtist);
                trackArtist = rt.Substring(segArtist + 3, segStation - (segArtist + 3));
                stationName = rt.Substring(segStation + 4);
                return true;
            }
            if(entercom)
            {
                //Get segments
                int segTitle = "Now playing ".Length;
                int segArtist = rt.LastIndexOf(" by ");
                int segStation = rt.LastIndexOf(" on ");

                //Pull info
                trackTitle = rt.Substring(segTitle, segArtist - segTitle);
                trackArtist = rt.Substring(segArtist + 4, segStation - (segArtist + 4));
                stationName = rt.Substring(segStation + 4);
                return true;
            }
            return false;
        }

        private void Session_RDSSessionResetEvent(RDSClient session)
        {
            //Clear
            radioText = null;
            _firstChunkDecoded = false;

            //Clear the buffer
            for (var i = 0; i < textBuffer.Length; i++)
                textBuffer[i] = ' ';

            //Send events
            RDSFeatureRadioText_RadioTextClearedUpdatedEvent?.Invoke();
            RDSFeatureRadioText_RadioTextBufferUpdatedEvent?.Invoke(textBuffer, 0, 0);
        }

        private void Session_RDSFrameReceivedEvent(RDSCommand frame, RDSClient session)
        {
            //Get type
            if (frame.GetType() != typeof(RadioTextRDSCommand))
                return;
            RadioTextRDSCommand cmd = (RadioTextRDSCommand)frame;

            //Set if the clear flag has been changed
            //http://www.interactive-radio-system.com/docs/EN50067_RDS_Standard.pdf defines that a screen clear should happen
            if (_lastClearFlag != cmd.clear)
            {
                //Clear the buffer
                for (var i = 0; i < textBuffer.Length; i++)
                    textBuffer[i] = ' ';

                //Set the buffer
                _lastClearFlag = cmd.clear;
                _firstChunkDecoded = false;

                //Send event
                RDSFeatureRadioText_RadioTextClearedUpdatedEvent?.Invoke();
            }

            //Set data in buffer
            textBuffer[cmd.offset + 0] = cmd.letterA;
            textBuffer[cmd.offset + 1] = cmd.letterB;
            textBuffer[cmd.offset + 2] = cmd.letterC;
            textBuffer[cmd.offset + 3] = cmd.letterD;

            //Set chunk flag
            if (cmd.offset == 0)
                _firstChunkDecoded = true;

            //Update final station name, if any
            //http://www.interactive-radio-system.com/docs/EN50067_RDS_Standard.pdf defines that the final message must end with \r. We use that to determine when the message is fully written
            if ((cmd.letterA == '\r' || cmd.letterB == '\r' || cmd.letterC == '\r' || cmd.letterD == '\r') && _firstChunkDecoded)
            {
                radioText = new string(textBuffer);
                RDSFeatureRadioText_RadioTextUpdatedEvent?.Invoke(radioText);
            }

            //Send event
            RDSFeatureRadioText_RadioTextBufferUpdatedEvent?.Invoke(textBuffer, cmd.offset, 4);
        }

        public delegate void RDSFeatureRadioText_RadioTextUpdatedEventArgs(string text);
        public delegate void RDSFeatureRadioText_RadioTextBufferUpdatedEventArgs(char[] buffer, int updatePos, int updateCount);
        public delegate void RDSFeatureRadioText_RadioTextClearedUpdatedEventArgs();
    }
}
