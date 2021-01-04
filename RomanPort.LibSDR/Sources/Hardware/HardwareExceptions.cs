using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Sources.Hardware
{
    public class HardwareNotFoundException : Exception
    {
        public HardwareNotFoundException() : base("The radio hardware was not found.")
        {

        }
    }

    public class HardwareLibraryNotFound : Exception
    {
        public HardwareLibraryNotFound(string name) : base($"The library \"{name}\" for accessing the radio hardware wasn't found. You'll have to obtain it yourself.")
        {

        }
    }

    public class HardwareLibraryInvalid : Exception
    {
        public HardwareLibraryInvalid(string name) : base($"The library \"{name}\" for accessing the radio hardware was not valid for this platform. Most likely, you have a 32-bit release for this 64-bit software (or vis versa).")
        {

        }
    }
}
