using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Sources.Hardware.RTLSDR
{
    public class RtlDeviceErrorException : Exception
    {
        public int rtlStatusCode;
        
        public RtlDeviceErrorException(int rtlStatusCode) : base("There was an error communicating with the RTL-SDR device: " + rtlStatusCode)
        {
            this.rtlStatusCode = rtlStatusCode;
        }
    }
}
