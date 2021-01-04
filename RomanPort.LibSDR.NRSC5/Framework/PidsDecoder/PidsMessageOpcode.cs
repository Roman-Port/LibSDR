using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.PidsDecoder
{
    public enum PidsMessageOpcode : byte
    {
        STATION_ID = 0b0000,
        STATION_NAME_SHORT = 0b0001,
        STATION_NAME_LONG = 0b0010,
        //Reserved, 0011
        STATION_LOCATION = 0b0100,
        STATION_MESSAGE = 0b0101,
        SERVICE_INFORMATION_MESSAGE = 0b0110,
        SIS_PARAMETER_MESSAGE = 0b0111,
        UNIVERSAL_SHORT_STATION_NAME = 0b1000,
        EMERGENCY_ALERTS_MESSAGE = 0b1001,
        //Reserved, 1010
        //Reserved, 1110
        //Reserved, 1111
    }
}
