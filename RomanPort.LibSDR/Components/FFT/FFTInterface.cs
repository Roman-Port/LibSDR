using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.FFT
{
    public abstract class FFTInterface
    {
        public abstract event FFTProcessorOnBlockEventArgs OnBlockProcessed;
        public abstract int FftBins { get; set; }
        public unsafe abstract void GetFFTSnapshot(float* ptr);
        public unsafe void GetFFTSnapshotManaged(float[] frame)
        {
            fixed (float* ptr = frame)
                GetFFTSnapshot(ptr);
        }
    }
}
