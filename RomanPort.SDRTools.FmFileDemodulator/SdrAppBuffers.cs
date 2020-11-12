using RomanPort.LibSDR.Framework;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanPort.SDRTools.FmFileDemodulator
{
    public unsafe class SdrAppBuffers
    {
        public SdrAppBuffers(int size)
        {
            this.size = size;
            iqBuffer = UnsafeBuffer.Create(size, sizeof(Complex));
            iqBufferPtr = (Complex*)iqBuffer;
            audioBufferA = UnsafeBuffer.Create(size, sizeof(float));
            audioBufferAPtr = (float*)audioBufferA;
            audioBufferB = UnsafeBuffer.Create(size, sizeof(float));
            audioBufferBPtr = (float*)audioBufferB;
            audioBufferC = UnsafeBuffer.Create(size, sizeof(float));
            audioBufferCPtr = (float*)audioBufferC;
        }

        public int size;
        public UnsafeBuffer iqBuffer;
        public Complex* iqBufferPtr;
        public UnsafeBuffer audioBufferA;
        public float* audioBufferAPtr;
        public UnsafeBuffer audioBufferB;
        public float* audioBufferBPtr;
        public UnsafeBuffer audioBufferC;
        public float* audioBufferCPtr;
    }
}
