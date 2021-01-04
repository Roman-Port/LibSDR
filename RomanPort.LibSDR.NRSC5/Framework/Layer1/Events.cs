using RomanPort.LibSDR.NRSC5.Framework.Layer1.Frames;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.Layer1
{
    public delegate void Nrsc5Layer1OnFrameEventArgs<T>(Nrsc5Layer1Decoder decoder, T frame);
}
