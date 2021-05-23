using RomanPort.LibSDR.Components.Digital.RDS.Data.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Digital.RDS.Client.Features
{
    public delegate void RdsClientRt_PartialTextReceivedEventArgs(RdsClient ctx, char[] buffer, int offset);
    public delegate void RdsClientRt_FullTextReceivedEventArgs(RdsClient ctx, string text);
    public delegate void RdsClientRt_ResetEventArgs(RdsClient ctx);

    public class RdsClientRt : IRdsClientFeature
    {
        private char[] buffer = new char[64];
        private string completeText;
        private bool abFlag;

        public char[] PartialBuffer { get => buffer; }
        public string CompleteText { get => completeText; }

        public event RdsClientRt_PartialTextReceivedEventArgs OnPartialTextReceived;
        public event RdsClientRt_FullTextReceivedEventArgs OnFullTextReceived;
        public event RdsClientRt_ResetEventArgs OnReset;

        public void ProcessCommand(RdsClient ctx, RdsCommand command)
        {
            //Ignore commands that aren't for PS
            if (!command.TryAsCommand(out RdsRadioTextCommand rtCommand))
                return;

            //Check if the A/B flag was reset
            if(abFlag != rtCommand.TextAB)
            {
                //Clear buffers
                for (int i = 0; i < 64; i++)
                    buffer[i] = ' ';

                //Send command
                OnReset?.Invoke(ctx);
            }
            abFlag = rtCommand.TextAB;

            //Read into buffer
            int offset = rtCommand.AbsoluteCharacterAddress;
            rtCommand.ReadCharacters(buffer, offset);

            //Send event
            OnPartialTextReceived?.Invoke(ctx, buffer, offset);

            //Search this string to see if we reached the end
            int endIndex = -1;
            for (int i = offset; i < offset + 4; i++)
            {
                //Official spec claims that the character 0x0A should be used to indicate the end, but some stations in my area don't do that.
                //We also check \r for this reason. Cumulus Media stations are using \r instead.
                if (buffer[i] == (char)0x0A || buffer[i] == '\r')
                {
                    endIndex = i;
                    break;
                }
            }

            //Check if it is complete
            if (endIndex != -1)
            {
                //Set complete text
                completeText = new string(buffer, 0, endIndex);

                //Send event
                OnFullTextReceived?.Invoke(ctx, completeText);
            }
        }

        public void Reset(RdsClient ctx)
        {
            //Clear buffers
            for (int i = 0; i < 64; i++)
                buffer[i] = ' ';
            completeText = "";

            //Send events
            OnPartialTextReceived?.Invoke(ctx, buffer, 0);
            OnFullTextReceived?.Invoke(ctx, completeText);
        }
    }
}
