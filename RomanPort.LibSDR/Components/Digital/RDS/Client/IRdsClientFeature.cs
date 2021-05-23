using RomanPort.LibSDR.Components.Digital.RDS.Data.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Digital.RDS.Client
{
    interface IRdsClientFeature
    {
        void ProcessCommand(RdsClient ctx, RdsCommand command);
        void Reset(RdsClient ctx);
    }
}
