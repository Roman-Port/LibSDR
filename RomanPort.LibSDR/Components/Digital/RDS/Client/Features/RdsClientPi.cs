using RomanPort.LibSDR.Components.Digital.RDS.Data;
using RomanPort.LibSDR.Components.Digital.RDS.Data.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Digital.RDS.Client.Features
{
    public delegate void RdsClientPi_CategoryChanged(RdsClient ctx, ushort pi);

    public class RdsClientPi : IRdsClientFeature
    {
        private ushort pi;

        public event RdsClientPi_CategoryChanged OnPiCodeChanged;

        public void ProcessCommand(RdsClient ctx, RdsCommand command)
        {
            //Check if it's changed
            bool changed = command.PiCode != pi;

            //Apply
            pi = command.PiCode;

            //Send event if needed
            if (changed)
                OnPiCodeChanged?.Invoke(ctx, command.PiCode);
        }

        public void Reset(RdsClient ctx)
        {
            pi = 0;
            OnPiCodeChanged?.Invoke(ctx, 0);
        }
    }
}
