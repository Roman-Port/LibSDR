using RomanPort.LibSDR.Components.Digital.RDS.Data.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanPort.LibSDR.Utils.RdsSpy
{
    public class RdsHistoryAdapter
    {
        public RdsHistoryAdapter(RdsCommand command)
        {
            this.command = command;
        }

        private RdsCommand command;

        public string PI { get => command.PiCode.ToString("X"); }
        public string Group { get => command.GroupName; }
        public string Description { get => command.DescribeCommand(); }
    }
}
