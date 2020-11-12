using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework.Exceptions
{
    public class RadioNotFoundException : Exception
    {
        public RadioNotFoundException() : base("The requested radio wasn't found. It likely isn't attached.")
        {

        }
    }
}
