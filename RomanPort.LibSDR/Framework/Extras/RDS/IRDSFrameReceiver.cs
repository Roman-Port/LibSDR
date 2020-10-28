using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework.Extras.RDS
{
    public interface IRDSFrameReceiver
    {
        void OnRDSFrameReceived(ushort a, ushort b, ushort c, ushort d);
    }
}
