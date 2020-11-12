using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework
{
    public struct InterleavedStereoAudio
    {
        public float left;
        public float right;

        public InterleavedStereoAudio(float left, float right)
        {
            this.left = left;
            this.right = right;
        }

        public unsafe static InterleavedStereoAudio FromDualArray(float* left, float* right, int index)
        {
            return new InterleavedStereoAudio(left[index], right[index]);
        }
    }
}
