using RomanPort.LibSDR.Demodulators;
using RomanPort.LibSDR.Extras;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RomanPort.IQFileIndexingTool
{
    public delegate void EditingFileMetadata_ReadyEventArgs();

    public class EditingFileMetadata
    {
        private string metadataFilename;
        private string originalFilename;
        private bool generated;
        private byte[] buffer;
        //0-31 : SHA 256
        //32-2031 : Waveform (as floats)

        public const int WAVEFORM_RESOLUTION = 500;

        public event EditingFileMetadata_ReadyEventArgs OnMadeReady;

        public EditingFileMetadata(string metadataFilename, string originalFilename)
        {
            this.metadataFilename = metadataFilename;
            this.originalFilename = originalFilename;

            //Init
            buffer = new byte[32 + (4 * WAVEFORM_RESOLUTION)];
            generated = File.Exists(metadataFilename);

            //Load if exists
            if(generated)
            {
                using (FileStream fs = new FileStream(metadataFilename, FileMode.Open))
                    fs.Read(buffer, 0, buffer.Length);
            }
        }

        public byte[] GetSha256()
        {
            byte[] b = new byte[32];
            Array.Copy(buffer, b, 32);
            return b;
        }

        public unsafe float[] GetWaveform()
        {
            float[] points = new float[WAVEFORM_RESOLUTION];
            fixed(float* outputPtr = points)
            fixed(byte* inputPtr = buffer)
            {
                float* inputFloatPtr = (float*)(inputPtr + 32);
                Buffer.MemoryCopy(inputFloatPtr, outputPtr, 4 * WAVEFORM_RESOLUTION, 4 * WAVEFORM_RESOLUTION);
            }
            return points;
        }

        private void SetSha256(byte[] sha)
        {
            Array.Copy(sha, buffer, 32);
        }

        private unsafe void SetWaveform(float[] points)
        {
            fixed (float* inputPtr = points)
            fixed (byte* outputPtr = buffer)
            {
                float* outputFloatPtr = (float*)(outputPtr + 32);
                Buffer.MemoryCopy(inputPtr, outputFloatPtr, 4 * WAVEFORM_RESOLUTION, 4 * WAVEFORM_RESOLUTION);
            }
        }

        private void Save()
        {
            using (FileStream fs = new FileStream(metadataFilename, FileMode.Create))
                fs.Write(buffer, 0, buffer.Length);
            generated = true;
        }

        public bool IsReady()
        {
            return generated;
        }

        public void GenerateAndSave()
        {
            GenerateSha();
            GenerateWaveform();
            Save();
            OnMadeReady?.Invoke();
        }

        private void GenerateSha()
        {
            //Oppn file and begin hashing
            byte[] hash;
            using (FileStream fs = new FileStream(originalFilename, FileMode.Open, FileAccess.Read))
            using (SHA256Managed a = (SHA256Managed)SHA256Managed.Create())
            {
                //Similar to ComputeHash in https://referencesource.microsoft.com/#mscorlib/system/security/cryptography/hashalgorithm.cs,e7c6be1ed86f474f
                //We use a (much) larger buffer to help speed things up
                byte[] buffer = new byte[32768];
                int bytesRead;
                do
                {
                    bytesRead = fs.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        a.TransformBlock(buffer, 0, bytesRead, null, 0);
                    }
                } while (bytesRead > 0);
                a.TransformFinalBlock(new byte[0], 0, 0);
                hash = a.Hash;
            }

            //Set
            SetSha256(hash);
        }

        private unsafe void GenerateWaveform()
        {
            fixed (byte* outputPtr = buffer)
            {
                //Get pointer
                float* outputFloatPtr = (float*)(outputPtr + 32);

                //Generate
                using (WaveformGenerator g = new WaveformGenerator(originalFilename, new FmDemodulator(), 250000, 19000, 16384 / 4))
                    g.RequestFull(outputFloatPtr, WAVEFORM_RESOLUTION);

                //Search for the max value
                float max = 0;
                for (int i = 0; i < WAVEFORM_RESOLUTION; i++)
                    max = Math.Max(max, outputFloatPtr[i]);

                //Scale all so that the max value is 1
                float scale = 1f / max;
                for (int i = 0; i < WAVEFORM_RESOLUTION; i++)
                    outputFloatPtr[i] *= scale;
            }
        }

        public void Delete()
        {
            if (File.Exists(metadataFilename))
                File.Delete(metadataFilename);
        }
    }
}
