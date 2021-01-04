using RomanPort.LibSDR.NRSC5.Framework.Layer1.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.PidsDecoder.Messages
{
    public class PidsMessageStationMsg : PidsMessageBase
    {
        public bool isFirst { get => frameNumber == 0; }
        public byte frameNumber;
        public byte sequence;
        public byte[] payload;

        //If first frame
        public bool isPriority;
        public StationMsgTextEncoding textEncoding;
        public byte length;
        public byte textChecksum;
        
        public override void Decode(PidsReader reader)
        {
            //Read constant header
            frameNumber = (byte)reader.ReadUInt(5);
            sequence = (byte)reader.ReadUInt(2);

            //Switch
            if(isFirst)
            {
                isPriority = reader.ReadBitBool();
                textEncoding = (StationMsgTextEncoding)reader.ReadUInt(3);
                length = (byte)reader.ReadUInt(8);
                textChecksum = (byte)reader.ReadUInt(7);
                payload = reader.ReadBytes(4);
            } else
            {
                payload = reader.ReadBytes(6);
            }
        }

        public static string ParsePayload(PidsMessageStationMsg firstMsg, byte[] payload)
        {
            if (!firstMsg.isFirst)
                throw new Exception("First message required!");
            if (firstMsg.textEncoding == StationMsgTextEncoding.TEXT_8BIT)
                return Encoding.UTF8.GetString(payload);
            else if (firstMsg.textEncoding == StationMsgTextEncoding.TEXT_16BIT)
                return Encoding.Unicode.GetString(payload);
            else
                throw new Exception("Unknown text formatting type.");
        }

        public override void ProcessParser(PidsParser parser, PidsMessageBaseContext context)
        {
            //Get real context
            MsgContext ctx = context.GetContext(new MsgContext());

            //If this is the first message, reset
            if(isFirst)
            {
                ctx.firstMsg = this;
                ctx.sequenceMsgs = new PidsMessageStationMsg[32];
            }

            //If we have no first message applied, abort
            if (ctx.firstMsg == null)
                return;

            //If this sequence number does not match, it's likely that we missed the first message for it. Don't do anything
            if (ctx.firstMsg.sequence != sequence)
                return;

            //Set in sequence
            ctx.sequenceMsgs[frameNumber] = this;

            //Determine if we have all of the packets needed
            int totalBytesRead = 0;
            for(int i = 0; totalBytesRead < ctx.firstMsg.length; i++)
            {
                if (ctx.sequenceMsgs[i] == null)
                    return; //Not enough data yet
                totalBytesRead += ctx.sequenceMsgs[i].payload.Length;
            }

            //Assemble byte array of data
            byte[] payload = new byte[totalBytesRead];
            int copyIndex = 0;
            for(int i = 0; copyIndex < ctx.firstMsg.length; i++)
            {
                Array.Copy(ctx.sequenceMsgs[i].payload, 0, payload, copyIndex, ctx.sequenceMsgs[i].payload.Length);
                copyIndex += ctx.sequenceMsgs[i].payload.Length;
            }

            //Decode to string
            string text = ParsePayload(ctx.firstMsg, payload);

            //Set
            parser.StationMessage = text;
        }

        class MsgContext
        {
            public PidsMessageStationMsg firstMsg;
            public PidsMessageStationMsg[] sequenceMsgs;
        }

        public enum StationMsgTextEncoding
        {
            TEXT_8BIT = 0b000,
            //Reserved (...)
            TEXT_16BIT = 0b100
            //Reserved (...)
        }
    }
}
