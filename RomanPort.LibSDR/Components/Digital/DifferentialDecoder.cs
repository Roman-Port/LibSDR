using RomanPort.LibSDR.Components.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Digital
{
    public class DifferentialDecoder
    {
        public DifferentialDecoder()
        {

        }

        private byte regA;
        private byte regB;
        private byte regState;

        public unsafe int Process(byte* ptr, int count)
        {
            //Get history
            for(int offset = 0; count > 0 && regState < 2; offset++)
            {
                regB = regA;
                regA = ptr[offset];
                regState++;
            }

            //Work
            for(int i = 0; i<count; i++)
            {
                regB = regA;
                regA = ptr[i];
                ptr[i] = (byte)((regA - regB) & 0b1);
            }
            return count;
        }
    }
}
