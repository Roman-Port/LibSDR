using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanPort.IQFileIndexingTool
{
    public interface IWorkerCommand
    {
        void ThreadedWork();
    }
}
