using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Radio.Framework
{
    /// <summary>
    /// Used for thread-safe messages
    /// </summary>
    public interface ISDRMessage
    {
        void Process();
    }
}
