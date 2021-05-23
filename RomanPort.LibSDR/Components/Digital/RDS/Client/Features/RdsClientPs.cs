using RomanPort.LibSDR.Components.Digital.RDS.Data.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Digital.RDS.Client.Features
{
    public delegate void RdsClientPs_PartialTextReceivedEventArgs(RdsClient ctx, char[] buffer, int offset);
    public delegate void RdsClientPs_FullTextReceivedEventArgs(RdsClient ctx, string text);

    public class RdsClientPs : IRdsClientFeature
    {
        private char[] buffer = new char[8];
        private string completeText;

        public char[] PartialBuffer { get => buffer; }
        public string CompleteText { get => completeText; }

        public event RdsClientPs_PartialTextReceivedEventArgs OnPartialTextReceived;
        public event RdsClientPs_FullTextReceivedEventArgs OnFullTextReceived;

        public void ProcessCommand(RdsClient ctx, RdsCommand command)
        {
            //Ignore commands that aren't for PS
            if (!command.TryAsCommand(out RdsBasicTuningCommand psCommand))
                return;

            //Read into buffer
            int offset = psCommand.AbsoluteCharacterAddress;
            buffer[offset + 0] = psCommand.PsCharA;
            buffer[offset + 1] = psCommand.PsCharB;

            //Send event
            OnPartialTextReceived?.Invoke(ctx, buffer, offset);

            //Check if it is complete
            if (offset == 6)
                TextFullyCompleted(ctx);
        }

        private void TextFullyCompleted(RdsClient ctx)
        {
            //Set complete text
            completeText = new string(buffer);

            //Send event
            OnFullTextReceived?.Invoke(ctx, completeText);
        }

        public void Reset(RdsClient ctx)
        {
            //Clear buffers
            for (int i = 0; i < 8; i++)
                buffer[i] = ' ';
            completeText = new string(buffer);

            //Send events
            OnPartialTextReceived?.Invoke(ctx, buffer, 0);
            OnFullTextReceived?.Invoke(ctx, completeText);
        }
    }
}
