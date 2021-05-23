using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Digital.RDS.Data.Commands
{
    public class RdsClockTimeCommand : RdsCommand
    {
        public RdsClockTimeCommand(ulong frame) : base(frame)
        {
        }

        //Raw values. Private to prevent confusion to the user.
        private uint JulianDayCode { get => (uint)ReadInteger(OFFSET_GROUP_C - 2, 17); }
        private byte Hour { get => (byte)ReadInteger(OFFSET_GROUP_D - 1, 5); }
        private byte Minute { get => (byte)ReadInteger(OFFSET_GROUP_D + 4, 6); }
        private sbyte LocalOffsetSegments { get => (sbyte)ReadSignedInteger(OFFSET_END - 6, 6); }

        //Public methods
        public TimeSpan LocalOffset { get => new TimeSpan(0, LocalOffsetSegments * 30, 0); }
        public DateTime DateUtc {
            get
            {
                //Decode UTC day code according to Annex G of http://www.interactive-radio-system.com/docs/EN50067_RDS_Standard.pdf
                uint dayCode = JulianDayCode;
                int year = (int)((dayCode - 15078.2f) / 365.25f);
                int month = (int)((dayCode - 14956.1f - (int)(year * 365.25f)) / 30.6001f);
                int day = (int)dayCode - 14956 - ((int)(year * 365.25f)) - ((int)(month * 30.6001f));
                int k;
                if (month == 14 || month == 15)
                    k = 1;
                else
                    k = 0;
                year += 1900 + k;
                month -= 1 - (k * 12);

                //Create
                return new DateTime(year, month, day, Hour, Minute, 0, DateTimeKind.Utc);
            }
        }
        public DateTime DateLocal { get => DateUtc.Add(LocalOffset); }

        public override string DescribeCommand()
        {
            return $"UTC: {DateUtc.ToShortDateString()} {DateUtc.ToShortTimeString()} / LOCAL: {DateLocal.ToShortDateString()} {DateLocal.ToShortTimeString()} / OFFSET: {LocalOffset.TotalHours} hours";
        }
    }
}
