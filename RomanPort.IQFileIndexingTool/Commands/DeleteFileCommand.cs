using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanPort.IQFileIndexingTool.Commands
{
    public class DeleteFileCommand : IWorkerCommand
    {
        private string filePath;

        public DeleteFileCommand(string filePath)
        {
            this.filePath = filePath;
        }

        public void ThreadedWork()
        {
            File.Delete(filePath);
        }
    }
}
