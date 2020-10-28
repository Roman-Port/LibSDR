using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Extras.RDS.Commands
{
    /// <summary>
    /// Transfers basic station data. Group type #0.
    /// 
    /// GroupC contains alternative frequencies
    /// GroupB contains two characters of the station name (used in some countries, such as the US, to display scrolling track info)
    /// </summary>
    public class BasicDataRDSCommand : RDSCommand
    {
        public char letterA;
        public char letterB;
        public int stationNameIndex;
        public bool di;

        internal override void ReadCommand(int groupBSpecial, ushort groupC, ushort groupD)
        {
            //Read header bits
            bool trafficAnnouncement = (((groupBSpecial >> 0) & 1)) == 1;
            bool isMusic = (((groupBSpecial >> 1) & 1)) == 1;
            bool di = (((groupBSpecial >> 2) & 1)) == 1;
            bool column1 = (((groupBSpecial >> 3) & 1)) == 1;
            bool column0 = (((groupBSpecial >> 4) & 1)) == 1;

            //Decode radio text segment
            char letterA = (char)(groupD & 0xff);
            char letterB = (char)((groupD >> 8) & 0xff);

            //Determine the starting index of the radio text
            int stationNameIndex = 0;
            if (!column1 && column0)
                stationNameIndex = 2;
            if (column1 && !column0)
                stationNameIndex = 4;
            if (column1 && column0)
                stationNameIndex = 6;

            //Set
            this.stationNameIndex = stationNameIndex;
            this.letterA = letterA;
            this.letterB = letterB;
            this.di = di;
        }
    }
}
