using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.PidsDecoder
{
    public delegate void PidsParserUpdatedEvent<T>(PidsParser parser, T data);
}
