using RomanPort.LibSDR.Sources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanPort.SDRTools.FmFileDemodulator.Workers
{
    public class OpenFileScanner : LoadingForm
    {
        public OpenFileScanner(string filePath, int bufferSize) : base("Reading file...")
        {
            this.filePath = filePath;
            this.bufferSize = bufferSize;
        }

        private string filePath;
        private int bufferSize;

        public override object WorkThread()
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            WavStreamSource s = new WavStreamSource(fs);
            float sampleRate = s.Open(bufferSize);
            return new Tuple<WavStreamSource, float>(s, sampleRate);
        }
    }
}
