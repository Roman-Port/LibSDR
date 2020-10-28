using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanPort.IQFileIndexingTool
{
    public class FFPlayAudio
    {
        private Process process;
        
        public FFPlayAudio(int sampleRate, int channels)
        {
            process = Process.Start(new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "ffplay",
                Arguments = $"-nodisp -hide_banner -loglevel warning -f f32le -ar {sampleRate} -ac {channels} -"
            });
        }

        public void WriteAudioSample(float s)
        {
            //Convert
            byte[] data = BitConverter.GetBytes(s);

            //Write to process
            process.StandardInput.BaseStream.Write(data, 0, 4);
        }

        public void Stop()
        {
            process.StandardInput.BaseStream.Close();
        }
    }
}
