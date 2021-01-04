using RomanPort.LibSDR.NRSC5.Framework.Layer1.Frames;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.NRSC5.Framework.AasDecoder
{
    public class AasParser
    {
        public AasParser()
        {

        }

        public void Process(FrameAas frame)
        {
            if (frame.port == 0x5100 || (frame.port >= 0x5201 && frame.port <= 0x5207))
            {
                // PSD frame.ports
                //output_id3(st, frame.port & 0x7, buf + 4, len - 4);
            }
            else if (frame.port == 0x20)
            {
                // Station Information Guide
                //parse_sig(st, buf + 4, len - 4);
            }
            else if (frame.port >= 0x401 && frame.port <= 0x50FF)
            {
                //process_frame.port(st, frame.port, buf + 4, len - 4);
            }
            else
            {
                throw new Exception("unknown AAS frame port");
            }
        }
    }
}
