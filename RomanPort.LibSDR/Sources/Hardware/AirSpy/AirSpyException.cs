using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Sources.Hardware.AirSpy
{
    public class AirSpyException : Exception
    {
        public airspy_error error;

        public AirSpyException(airspy_error error, string msg) : base($"{error.ToString()}: {msg}")
        {
            this.error = error;
        }
    }
}
