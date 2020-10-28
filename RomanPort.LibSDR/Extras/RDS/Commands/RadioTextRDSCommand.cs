using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Extras.RDS.Commands
{
    public class RadioTextRDSCommand : RDSCommand
    {
        public char letterA;
        public char letterB;
        public char letterC;
        public char letterD;

        public int offset;
        public bool clear;

        internal override void ReadCommand(int groupBSpecial, ushort groupC, ushort groupD)
        {
            //Decode radio text characters
            char letterA = (char)(groupC & 0xff);
            char letterB = (char)((groupC >> 8) & 0xff);
            char letterC = (char)(groupD & 0xff);
            char letterD = (char)((groupD >> 8) & 0xff);

            //Set
            this.letterA = letterA;
            this.letterB = letterB;
            this.letterC = letterC;
            this.letterD = letterD;

            //Get params
            clear = (((groupBSpecial >> 0) & 1)) == 1;

            //Get offset
            offset = 0;
            offset |= (((groupBSpecial >> 1) & 1)) << 3;
            offset |= (((groupBSpecial >> 2) & 1)) << 2;
            offset |= (((groupBSpecial >> 3) & 1)) << 1;
            offset |= (((groupBSpecial >> 4) & 1)) << 0;
            offset *= 4;
        }
    }
}
