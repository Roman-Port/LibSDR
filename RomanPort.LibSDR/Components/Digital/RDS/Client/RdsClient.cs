using RomanPort.LibSDR.Components.Digital.RDS.Client.Features;
using RomanPort.LibSDR.Components.Digital.RDS.Data.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Digital.RDS.Client
{
    public delegate void RdsClient_CommandEventArgs(RdsClient client, RdsCommand command);
    public delegate void RdsClient_ResetEventArgs(RdsClient client);

    public class RdsClient
    {
        public RdsClient()
        {
            features = new IRdsClientFeature[]
            {
                ProgramService,
                ProgramType,
                PiCode,
                RadioText
            };
        }

        public event RdsClient_CommandEventArgs OnCommand;
        public event RdsClient_ResetEventArgs OnReset;

        public RdsClientPs ProgramService { get; } = new RdsClientPs();
        public RdsClientPty ProgramType { get; } = new RdsClientPty();
        public RdsClientPi PiCode { get; } = new RdsClientPi();
        public RdsClientRt RadioText { get; } = new RdsClientRt();

        private IRdsClientFeature[] features;

        public void ProcessFrame(ulong frame)
        {
            ProcessCommand(RdsCommand.DecodeCommand(frame));
        }

        public void ProcessCommand(RdsCommand command)
        {
            foreach (var f in features)
                f.ProcessCommand(this, command);
            OnCommand?.Invoke(this, command);
        }
        
        public void Reset()
        {
            foreach (var f in features)
                f.Reset(this);
            OnReset?.Invoke(this);
        }
    }
}
