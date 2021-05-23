using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Digital.RDS
{
    public delegate void RDSEventResetArgs(RDSClient client);
    public delegate void RDSEventPiCodeUpdated(RDSClient client, ushort pi);
    public delegate void RDSEventTrafficUpdated(RDSClient client, bool enabled);
    public delegate void RDSEventProgramTypeUpdated(RDSClient client, byte type);

    public delegate void RDSEventPsNameUpdated(RDSClient client, string name);
    public delegate void RDSEventPsBufferUpdated(RDSClient client, char[] buffer);

    public delegate void RDSEventRtTextUpdated(RDSClient client, string text);
    public delegate void RDSEventRtBufferUpdated(RDSClient client, char[] buffer);
    public delegate void RDSEventRtBufferCleared(RDSClient client);

    public delegate void RDSEventTimeUpdated(RDSClient client, DateTime time, TimeSpan offset);

    //other

    public delegate void RDSFrameDecoded(ulong frame);
    public delegate void RDSSyncStateChanged(bool sync);
}
