using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanPort.IQFileIndexingTool.Commands
{
    public class SaveFileCommand : IWorkerCommand
    {
        private Form1 context;
        private string sourcePath;
        private string destPath;
        private string copyText;

        public SaveFileCommand(Form1 context, string sourcePath, string destPath, string copyText)
        {
            this.context = context;
            this.sourcePath = sourcePath;
            this.destPath = destPath;
            this.copyText = copyText;
        }

        public void ThreadedWork()
        {
            //Write copy text
            byte[] copyTextData = Encoding.UTF8.GetBytes(copyText + "\n");
            context.copyTextFile.Write(copyTextData, 0, copyTextData.Length);
            context.copyTextFile.Flush();

            //Move file
            File.Move(sourcePath, destPath);
        }
    }
}
