using RomanPort.LibSDR.Demodulators;
using RomanPort.LibSDR.Framework.Extras.RDS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RomanPort.LibSDR.Extras.RDS
{
    /// <summary>
    /// RDS client using spec based off of http://www.interactive-radio-system.com/docs/EN50067_RDS_Standard.pdf
    /// </summary>
    public class RDSClient
    {
        private const int HEADER_BITS = 11;
        private const int HEADER_OFFSET = 16 - HEADER_BITS - 1;
        
        /// <summary>
        /// Set to true when ANY valid RDS frame is found
        /// </summary>
        public bool rdsSupported;

        /// <summary>
        /// The program identification code. Set if rdsSupported is true
        /// </summary>
        public ushort piCode;
        public event RDSEventPiCodeUpdated OnPiCodeUpdated;

        /// <summary>
        /// Set to true if the station has claimed to send traffic data. Uses the traffic program code. Set if rdsSupported is true
        /// </summary>
        public bool trafficSupported;
        public event RDSEventTrafficUpdated OnTrafficSupportedUpdated;

        /// <summary>
        /// The program type specified by the PTY. Set if rdsSupported is true
        /// </summary>
        public byte programTypeCode;
        public event RDSEventProgramTypeUpdated OnProgramTypeUpdated;

        public event RDSEventResetArgs OnReset;

        /* PROGRAM SERVICE NAME */

        /// <summary>
        /// Set to true when we get a complete program service name
        /// </summary>
        public bool psComplete;

        /// <summary>
        /// The current, complete, program service name
        /// </summary>
        public string psName;

        /// <summary>
        /// The in-progress program service name. May be incomplete
        /// </summary>
        public char[] psBuffer;

        public event RDSEventPsNameUpdated OnPsNameUpdated;
        public event RDSEventPsBufferUpdated OnPsBufferUpdated;

        /* RADIO TEXT */

        /// <summary>
        /// Set to true when we get a complete Radio Text frame
        /// </summary>
        public bool rtComplete;

        /// <summary>
        /// The current, complete, Radio Text
        /// </summary>
        public string rtText;

        /// <summary>
        /// The in-progress RadioText. May be incomplete.
        /// </summary>
        public char[] rtBuffer;

        private bool _rtLastAbFlag;

        public event RDSEventRtTextUpdated OnRtTextUpdated;
        public event RDSEventRtBufferUpdated OnRtBufferUpdated;
        public event RDSEventRtBufferCleared OnRtBufferCleared;

        /* TIME */

        /// <summary>
        /// Set to true when we get a valid time packet
        /// </summary>
        public bool timeComplete;

        /// <summary>
        /// The last time we've got
        /// </summary>
        public DateTime timeLast;

        /// <summary>
        /// Local offset
        /// </summary>
        public TimeSpan timeOffset;

        public event RDSEventTimeUpdated OnTimeUpdated;

        public RDSClient()
        {
            //Prepare Progam Serivce name
            psComplete = false;
            psName = "        ";
            psBuffer = new char[8];

            //Prepare Radio Text
            rtComplete = false;
            rtText = "";
            rtBuffer = new char[64];
        }

        public void ProcessFrameBigEndian(ushort inGroupA, ushort inGroupB, ushort inGroupC, ushort inGroupD)
        {
            //Convert from big endian systems (UNTESTED)
            if (!BitConverter.IsLittleEndian)
            {
                inGroupA = ((ushort)((inGroupA >> 8) | (inGroupA << 8)));
                inGroupB = ((ushort)((inGroupB >> 8) | (inGroupB << 8)));
                inGroupC = ((ushort)((inGroupC >> 8) | (inGroupC << 8)));
                inGroupD = ((ushort)((inGroupD >> 8) | (inGroupD << 8)));
            }

            //Process frame
            ProcessFrame(new RDSFrame
            {
                a = inGroupA,
                b = inGroupB,
                c = inGroupC,
                d = inGroupD
            });
        }

        public void ProcessFrame(RDSFrame frame)
        {
            //Get the PI code from the first block. We flip this because that's how it's familiar
            ushort piCode = (ushort)((frame.a >> 8) | (frame.a << 8));

            //Decode header data that will always exist
            byte groupTypeCode = (byte)((frame.b >> (16 - 5)) & 0b0000000000011111);
            bool trafficSupported = 1 == ((frame.b >> (16 - 6)) & 0b0000000000000001);
            byte programTypeCode = (byte)((frame.b >> (16 - 11)) & 0b0000000000011111);

            //Send updated events for these
            if(this.piCode != piCode)
            {
                this.piCode = piCode;
                OnPiCodeUpdated?.Invoke(this, piCode);
            }
            if (this.trafficSupported != trafficSupported)
            {
                this.trafficSupported = trafficSupported;
                OnTrafficSupportedUpdated?.Invoke(this, trafficSupported);
            }
            if (this.programTypeCode != programTypeCode)
            {
                this.programTypeCode = programTypeCode;
                OnProgramTypeUpdated?.Invoke(this, programTypeCode);
            }

            //We'll now handle decoding of this frame
            switch(groupTypeCode)
            {
                case 0b00000: ProcessFramePayload_BasicInfo(frame); break;
                case 0b00001: ProcessFramePayload_BasicInfo(frame); break;
                case 0b00100: ProcessFramePayload_RadioTextA(frame); break;
                case 0b00101: ProcessFramePayload_RadioTextB(frame); break;
                case 0b01000: ProcessFramePayload_ClockTime(frame); break;
            }
        }

        /// <summary>
        /// Processes 0A and 0B "basic tuning and switching" frame types 0b00000 and 0b00001
        /// </summary>
        private void ProcessFramePayload_BasicInfo(RDSFrame frame)
        {
            //Read the header flags
            bool flagTa = 1 == ((frame.b >> (HEADER_OFFSET - 0)) & 0b0000000000000001);
            bool flagMs = 1 == ((frame.b >> (HEADER_OFFSET - 1)) & 0b0000000000000001);
            bool flagDi = 1 == ((frame.b >> (HEADER_OFFSET - 2)) & 0b0000000000000001);

            //Get the code bits. These are just used for the text for the most part just for the PS name
            byte decoderControlCode = (byte)((frame.b >> (HEADER_OFFSET - 4)) & 0b0000000000000011);

            //Get the two PS code characters
            char psA = (char)((frame.d >> 8) & 0x00FF);
            char psB = (char)((frame.d >> 0) & 0x00FF);

            //Write to the PS buffer
            psBuffer[(decoderControlCode * 2) + 0] = psA;
            psBuffer[(decoderControlCode * 2) + 1] = psB;

            //Check if we've completed it
            if(decoderControlCode == 3)
            {
                //Completed!
                //Update state
                psName = new string(psBuffer);
                psComplete = true;

                //Fire events
                OnPsNameUpdated?.Invoke(this, psName);
            }

            //Send updated events
            OnPsBufferUpdated?.Invoke(this, psBuffer);
        }

        /// <summary>
        /// Processes a 2A "RadioText" frame type 0b00100
        /// </summary>
        /// <param name="frame"></param>
        private void ProcessFramePayload_RadioTextA(RDSFrame frame)
        {
            //Read the header flags
            bool flagAb = 1 == ((frame.b >> (HEADER_OFFSET - 0)) & 0b0000000000000001);

            //Get the index and multiply it by 4, as that is the spacing
            int addressIndex = (frame.b & 0b0000000000001111) * 4;

            //Check if we should reset
            if (flagAb != _rtLastAbFlag && addressIndex == 0)
            {
                //Clear buffer
                for (int i = 0; i < rtBuffer.Length; i++)
                    rtBuffer[i] = (char)0x00;

                //Update state
                _rtLastAbFlag = flagAb;

                //Send event
                OnRtBufferCleared?.Invoke(this);
            }

            //Write all
            rtBuffer[addressIndex + 0] = (char)((frame.c >> 8) & 0x00FF);
            rtBuffer[addressIndex + 1] = (char)((frame.c >> 0) & 0x00FF);
            rtBuffer[addressIndex + 2] = (char)((frame.d >> 8) & 0x00FF);
            rtBuffer[addressIndex + 3] = (char)((frame.d >> 0) & 0x00FF);

            //Search this string to see if we reached the end
            int endIndex = -1;
            for(int i = addressIndex; i < addressIndex + 4; i++)
            {
                //Official spec claims that the character 0x0A should be used to indicate the end, but some stations in my area don't do that.
                //We also check \r for this reason. Cumulus Media stations are using \r instead.
                if(rtBuffer[i] == (char)0x0A || rtBuffer[i] == '\r')
                {
                    endIndex = i;
                    break;
                }
            }

            //Send buffer updated events
            OnRtBufferUpdated?.Invoke(this, rtBuffer);

            //Handle ending if we did
            if(endIndex != -1)
            {
                //Make sure this is really the end. We should have no null characters up to this point
                for (int i = 0; i < endIndex; i++)
                    if (rtBuffer[i] == (char)0x00)
                        return;
                
                //Convert to string and update state
                rtText = new string(rtBuffer, 0, endIndex);
                rtComplete = true;

                //Send event
                OnRtTextUpdated?.Invoke(this, rtText);
            }
        }

        /// <summary>
        /// Processes a 2B "RadioText" frame type 0b00101
        /// </summary>
        /// <param name="frame"></param>
        private void ProcessFramePayload_RadioTextB(RDSFrame frame)
        {
            //Read the header flags
            bool flagAb = 1 == ((frame.b >> (HEADER_OFFSET - 0)) & 0b0000000000000001);

            //Get the index and multiply it by 2, as that is the spacing
            int addressIndex = (frame.b & 0b0000000000001111) * 2;

            //Check if we should reset
            if (flagAb != _rtLastAbFlag && addressIndex == 0)
            {
                //Clear buffer
                for (int i = 0; i < rtBuffer.Length; i++)
                    rtBuffer[i] = (char)0x00;

                //Update state
                _rtLastAbFlag = flagAb;

                //Send event
                OnRtBufferCleared?.Invoke(this);
            }

            //Write all
            rtBuffer[addressIndex + 0] = (char)((frame.d >> 8) & 0x00FF);
            rtBuffer[addressIndex + 1] = (char)((frame.d >> 0) & 0x00FF);

            //Search this string to see if we reached the end
            int endIndex = -1;
            for (int i = addressIndex; i < addressIndex + 4; i++)
            {
                if (rtBuffer[i] == (char)0x0A)
                {
                    endIndex = i;
                    break;
                }
            }

            //Send buffer updated events
            OnRtBufferUpdated?.Invoke(this, rtBuffer);

            //Handle ending if we did
            if (endIndex != -1)
            {
                //Convert to string and update state
                rtText = new string(rtBuffer, 0, endIndex);
                rtComplete = true;

                //Send event
                OnRtTextUpdated?.Invoke(this, rtText);
            }
        }

        /// <summary>
        /// Processes a 4A "Clock-time and date" frame 0b01000
        /// </summary>
        private void ProcessFramePayload_ClockTime(RDSFrame frame)
        {
            //Read julian day code
            uint dayCode = (uint)((frame.c >> 1) & 0b0111111111111111);
            dayCode |= (uint)(((frame.b >> (HEADER_OFFSET - 4)) & 0b0000000000000011) << 15);

            //Decode day code according to Annex G of the linked document
            //This is in UTC
            int year = (int)((dayCode - 15078.2f) / 365.25f);
            int month = (int)((dayCode - 14956.1f - (int)(year * 365.25f)) / 30.6001f);
            int day = (int)dayCode - 14956 - ((int)(year * 365.25f)) - ((int)(month * 30.6001f));
            int k;
            if (month == 14 || month == 15)
                k = 1;
            else
                k = 0;
            year += 1900 + k;
            month -= 1 - (k * 12);

            //Get the hour
            int localHour = ((frame.d >> 12) & 0b0000000000001111);
            localHour |= ((frame.c & 0b0000000000000001) << 4);

            //Get the minute
            int localMinute = ((frame.d >> 6) & 0b0000000000111111);

            //Get the local time offset, in half hours. Also calculate the sign
            int localOffsetParts = (frame.d & 0b0000000000011111);
            if ((frame.d & 0b0000000000100000) != 0)
                localOffsetParts = -localOffsetParts;

            DateTime time;
            TimeSpan localOffset;
            DateTime localTime;
            try
            {
                //Create a UTC DateTime from the UTC portion
                time = new DateTime(year, month, day, 0, 0, 0, 0, DateTimeKind.Local);

                //Convert the local hour and minute to a TimeSpan we'll save
                localOffset = new TimeSpan(0, 30 * localOffsetParts, 0);

                //Convert to local time
                localTime = time.AddMinutes((localHour * 60) + (localOffsetParts * 30) + localMinute);
            } catch
            {
                //Ignore. If this was a corrupted RDS frame, this would've thrown an exception
                return;
            }

            //Set
            this.timeLast = localTime;
            this.timeOffset = localOffset;
            this.timeComplete = true;

            //Send event
            OnTimeUpdated?.Invoke(this, localTime, localOffset);
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

        public static bool TryGetTrackInfo(string rt, out string trackTitle, out string trackArtist, out string stationName)
        {
            try
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
                if (cumulusMedia)
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
                if (entercom)
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
            }
            catch
            {
                trackTitle = null;
                trackArtist = null;
                stationName = null;
            }
            return false;
        }

        internal void SubscribeToRdsDemodulator(RdsDemodulator demod)
        {
            demod.OnRDSFrameReceived += Demod_OnRDSFrameReceived;
        }

        private void Demod_OnRDSFrameReceived(ushort frameA, ushort frameB, ushort frameC, ushort frameD)
        {
            ProcessFrameBigEndian(frameA, frameB, frameC, frameD);
        }

        /// <summary>
        /// Clears out information, but will not stop events. This should be called when a new station is tuned to
        /// </summary>
        public void Reset()
        {
            rdsSupported = false;
            piCode = 0;
            psComplete = false;
            rtComplete = false;
            timeComplete = false;
            psBuffer = new char[8];
            rtBuffer = new char[64];
            OnReset?.Invoke(this);
        }
    }
}
