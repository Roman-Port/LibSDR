using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.PidsDecoder
{
    public class PidsMessageBaseContext
    {
        private object ctx;

        public T GetContext<T>(T defaultValue)
        {
            if (ctx == null)
                ctx = defaultValue;
            return (T)ctx;
        }
    }
}
