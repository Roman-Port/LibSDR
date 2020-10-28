using RomanPort.LibSDR.Framework.Radio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace RomanPort.LibSDR.Receivers
{
    public class FFPlayReceiver : IRadioSampleReceiver, IDisposable
    {
        public static string FFPLAY_PATHNAME = "ffplay";
        
        private Process process;
        private byte[] buffer;

        public FFPlayReceiver()
        {
            //Open buffer
            buffer = new byte[8]; //Two channels of floats
        }

        public void Open(float sampleRate, int bufferSize)
        {
            //Open FFPlay
            try
            {
                process = Process.Start(new ProcessStartInfo
                {
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    FileName = FFPLAY_PATHNAME,
                    Arguments = $"-nodisp -hide_banner -loglevel warning -f f32le -ar {sampleRate} -ac 2 -"
                });
            } catch (System.ComponentModel.Win32Exception)
            {
                //FFPlay not found
                throw new Exception($"Couldn't find FFPlay! Specify it's path with \"FFPlayReceiver.FFPLAY_PATHNAME\", or download FFPlay/FFMpeg.");
            }
        }

        public unsafe void OnSamples(float* left, float* right, int samplesRead)
        {
            //Open byte pointers
            byte* leftBytes = (byte*)left;
            byte* rightBytes = (byte*)right;
            
            //Loop
            for(int i = 0; i<samplesRead; i++)
            {
                //Read
                int readOffset = (4 * i);
                buffer[0] = leftBytes[readOffset + 0];
                buffer[1] = leftBytes[readOffset + 1];
                buffer[2] = leftBytes[readOffset + 2];
                buffer[3] = leftBytes[readOffset + 3];
                buffer[4] = rightBytes[readOffset + 0];
                buffer[5] = rightBytes[readOffset + 1];
                buffer[6] = rightBytes[readOffset + 2];
                buffer[7] = rightBytes[readOffset + 3];

                //Send
                process.StandardInput.BaseStream.Write(buffer, 0, 8);
            }
        }

        public void Dispose()
        {
            process.Kill();
            process.Dispose();
        }
    }
}
