using RomanPort.LibSDR.Demodulators;
using RomanPort.LibSDR.Extras.RDS.Commands;
using RomanPort.LibSDR.Extras.RDS.Features;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Extras.RDS
{
    public class RDSClient
    {
        /// <summary>
        /// The program identification code
        /// </summary>
        public ushort? piCode;

        /// <summary>
        /// Set to true when ANY valid RDS frame is found
        /// </summary>
        public bool rdsSupported;

        /// <summary>
        /// The station name type
        /// </summary>
        public RDSFeatureStationName featureStationName;

        /// <summary>
        /// The station radio text
        /// </summary>
        public RDSFeatureRadioText featureRadioText;

        /// <summary>
        /// Called when we get any frame
        /// </summary>
        public event RDSFrameReceivedEventArgs RDSFrameReceivedEvent;

        /// <summary>
        /// Called when we reset the RDS session. Features should clear data
        /// </summary>
        public event RDSSessionResetEventArgs RDSSessionResetEvent;

        /// <summary>
        /// Called when the PI code updates
        /// </summary>
        public event RDSSessionPiCodeEventArgs OnPiCodeUpdated;

        public RDSClient()
        {
            featureStationName = new RDSFeatureStationName(this);
            featureRadioText = new RDSFeatureRadioText(this);
        }

        public static bool TryGetCallsign(ushort code, out string callsign)
        {
            code = ((ushort)((code & 0x00FF) << 8 | (code & 0xFF00) >> 8)); //Swap upper and lower bits, as this was originally written for the other way around
            char[] callLetters = new char[4];
            if (code > 21671)
            {
                callLetters[0] = 'W';
                code -= 21672;
            }
            else
            {
                callLetters[0] = 'K';
                code -= 4096;
            }
            int call2 = code / 676;
            code -= (ushort)(676 * call2);
            int call3 = code / 26;
            int call4 = code - (26 * call3);
            callLetters[1] = (char)(call2 + 65);
            callLetters[2] = (char)(call3 + 65);
            callLetters[3] = (char)(call4 + 65);
            callsign = new string(callLetters);
            return callLetters[0] == 'K' || callLetters[0] == 'W';
        }

        public RDSCommand DecodeFrame(ushort inGroupA, ushort inGroupB, ushort inGroupC, ushort inGroupD)
        {
            //Decode
            RDSCommand cmd = RDSCommand.ReadRdsFrame(inGroupA, inGroupB, inGroupC, inGroupD);

            //Update PI code and supported flag
            if (piCode != cmd.programIdentificationCode)
                OnPiCodeUpdated?.Invoke(this, cmd.programIdentificationCode);
            piCode = cmd.programIdentificationCode;
            rdsSupported = true;

            //Send events
            RDSFrameReceivedEvent?.Invoke(cmd, this);

            return cmd;
        }

        internal void SubscribeFmDemodulator(WbFmDemodulator demod)
        {
            demod.UseRdsDemodulator();
            demod.OnRdsFrame += Demod_OnRdsFrame;
        }

        private void Demod_OnRdsFrame(ushort frameA, ushort frameB, ushort frameC, ushort frameD, WbFmDemodulator demodulator)
        {
            DecodeFrame(frameA, frameB, frameC, frameD);
        }

        /// <summary>
        /// Clears out information, but will not stop events. This should be called when a new station is tuned to
        /// </summary>
        public void Reset()
        {
            piCode = null;
            rdsSupported = false;
            RDSSessionResetEvent?.Invoke(this);
        }
    }

    public delegate void RDSFrameReceivedEventArgs(RDSCommand frame, RDSClient session);
    public delegate void RDSSessionResetEventArgs(RDSClient session);
    public delegate void RDSSessionPiCodeEventArgs(RDSClient session, ushort piCode);
}
