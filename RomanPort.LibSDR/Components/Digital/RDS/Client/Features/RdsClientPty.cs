using RomanPort.LibSDR.Components.Digital.RDS.Data;
using RomanPort.LibSDR.Components.Digital.RDS.Data.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Digital.RDS.Client.Features
{
    public delegate void RdsClientPty_CategoryChanged(RdsClient ctx, byte category);
    
    public class RdsClientPty : IRdsClientFeature
    {
        private byte category;

        public byte Category { get => category; }
        public RdsPtyAmerica CategoryAmerica { get => (RdsPtyAmerica)Category; }
        public RdsPtyInternational CategoryInternational { get => (RdsPtyInternational)Category; }

        public event RdsClientPty_CategoryChanged OnCategoryChanged;

        public void ProcessCommand(RdsClient ctx, RdsCommand command)
        {
            //Check if it's changed
            bool changed = command.ProgramType != category;

            //Apply
            category = command.ProgramType;

            //Send event if needed
            if (changed)
                OnCategoryChanged?.Invoke(ctx, command.ProgramType);
        }

        public void Reset(RdsClient ctx)
        {
            category = 0;
            OnCategoryChanged?.Invoke(ctx, 0);
        }
    }
}
