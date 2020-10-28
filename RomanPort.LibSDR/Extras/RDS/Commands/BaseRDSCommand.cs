using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Extras.RDS.Commands
{
    public abstract class RDSCommand
    {
        public ushort programIdentificationCode;

        public RdsHeaderFrame _header;
        public ushort _groupA;
        public ushort _groupB;
        public ushort _groupC;
        public ushort _groupD;

        public static RDSCommand ReadRdsFrame(ushort inGroupA, ushort inGroupB, ushort inGroupC, ushort inGroupD)
        {
            //Swap frame bits
            ushort groupA = (ushort)((inGroupA >> 8) | (inGroupA << 8));
            ushort groupB = (ushort)((inGroupB >> 8) | (inGroupB << 8));
            ushort groupC = (ushort)((inGroupC >> 8) | (inGroupC << 8));
            ushort groupD = (ushort)((inGroupD >> 8) | (inGroupD << 8));

            //Decode params in block 2
            RdsHeaderFrame header = DecodeGroupBHeader(groupB);

            //Determine the RDSCommand to use
            RDSCommand cmd;
            switch (header.groupType)
            {
                case 0: cmd = new BasicDataRDSCommand(); break;
                case 2: cmd = new RadioTextRDSCommand(); break;
                default: cmd = new UnsupportedRDSCommand(); break;
            }

            //Set basic params on the command
            cmd.programIdentificationCode = groupA;
            cmd._header = header;
            cmd._groupA = groupA;
            cmd._groupB = groupB;
            cmd._groupC = groupC;
            cmd._groupD = groupD;

            //Read the data
            cmd.ReadCommand(header.specialData, groupC, groupD);

            return cmd;
        }

        private static RdsHeaderFrame DecodeGroupBHeader(ushort groupB)
        {
            int groupType = 0;
            groupType |= (((groupB >> 7) & 1) << 3);
            groupType |= (((groupB >> 6) & 1) << 2);
            groupType |= (((groupB >> 5) & 1) << 1);
            groupType |= (((groupB >> 4) & 1) << 0);

            int typeB = 0;
            typeB |= (((groupB >> 3) & 1) << 0);

            int traffic = 0;
            traffic |= (((groupB >> 2) & 1) << 0);

            int programType = 0;
            programType |= (ushort)(((groupB >> 1) & 1) << 4);
            programType |= (ushort)(((groupB >> 0) & 1) << 3);
            programType |= (ushort)(((groupB >> 15) & 1) << 2);
            programType |= (ushort)(((groupB >> 14) & 1) << 1);
            programType |= (ushort)(((groupB >> 13) & 1) << 0);
            int unknownData = 0;
            unknownData |= (((groupB >> 12) & 1) << 0);
            unknownData |= (((groupB >> 11) & 1) << 1);
            unknownData |= (((groupB >> 10) & 1) << 2);
            unknownData |= (((groupB >> 9) & 1) << 3);
            unknownData |= (((groupB >> 8) & 1) << 4);

            return new RdsHeaderFrame
            {
                groupType = groupType,
                traffic = traffic == 1,
                isTypeA = typeB == 0,
                programType = programType,
                specialData = unknownData
            };
        }

        internal abstract void ReadCommand(int groupBSpecial, ushort groupC, ushort groupD);
    }

    public struct RdsHeaderFrame
    {
        public int groupType;
        public bool traffic;
        public bool isTypeA;
        public int programType;
        public int specialData;
    }
}
