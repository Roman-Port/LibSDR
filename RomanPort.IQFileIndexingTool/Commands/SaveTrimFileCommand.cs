using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanPort.IQFileIndexingTool.Commands
{
    public class SaveTrimFileCommand : IWorkerCommand
    {
        private Form1 context;
        private string sourcePath;
        private string destPath;
        private string copyText;
        private long startSample;
        private long endSample;

        public SaveTrimFileCommand(Form1 context, string sourcePath, string destPath, string copyText, long startSample, long endSample)
        {
            this.context = context;
            this.sourcePath = sourcePath;
            this.destPath = destPath;
            this.copyText = copyText;
            this.startSample = startSample;
            this.endSample = endSample;
        }

        public void ThreadedWork()
        {
            //Write copy text
            byte[] copyTextData = Encoding.UTF8.GetBytes(copyText + "\n");
            context.copyTextFile.Write(copyTextData, 0, copyTextData.Length);
            context.copyTextFile.Flush();

            //Begin trimming
            using(FileStream input = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
            using(FileStream output = new FileStream(destPath, FileMode.Create))
            {
                //Allocate large buffer
                byte[] buffer = new byte[65536];

                //Read header
                input.Read(buffer, 0, 44);
                short channels = 2;
                short bitsPerSample = BitConverter.ToInt16(buffer, 34);
                int bytesPerSample = bitsPerSample / 8;

                //Update lengths
                long dataLen = (endSample - startSample) * bytesPerSample * channels;
                BitConverter.GetBytes((int)dataLen).CopyTo(buffer, 40);
                BitConverter.GetBytes((int)(dataLen + 36)).CopyTo(buffer, 4);

                //Write header
                output.Write(buffer, 0, 44);

                //Jump to beginning and calculate ending byte
                input.Position = SampleIndexToByteIndex(startSample, bytesPerSample, channels);
                long endByte = SampleIndexToByteIndex(endSample, bytesPerSample, channels);

                //Begin copying
                int read;
                do
                {
                    //Calculate number to read
                    int desiredRead = (int)Math.Min(endByte - input.Position, buffer.Length);

                    //Read from input
                    read = input.Read(buffer, 0, desiredRead);

                    //Write to output
                    output.Write(buffer, 0, read);
                } while (read != 0);
            }

            //Delete original
            File.Delete(sourcePath);
        }

        private long SampleIndexToByteIndex(long sample, int bytesPerSample, int channels)
        {
            return (sample * bytesPerSample * channels) + 44;
        }
    }
}
