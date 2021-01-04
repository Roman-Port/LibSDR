using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5
{
    public delegate void HDRadioDecoderAudioEventArgs(HDRadioDecoder decoder, int program, short[] buffer, int count);
}
