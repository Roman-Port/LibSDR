using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework.Util
{
    public class InterpolationUtil
    {
        public unsafe static float Interpolate(float* data, int dataLen, float interp)
        {
            //Since we're working with indexing, it's easier to have dataLen be the max sample
            int lastSample = dataLen - 1;
            
            //Handle quick and easy special cases
            if (interp == 0)
                return data[0];
            if (interp == 1)
                return data[lastSample];

            //Find samples we're closest to
            int aIndex = (int)Math.Floor(lastSample / interp);
            int bIndex = (int)Math.Ceiling(lastSample / interp);

            //Handle easy case of them matching
            if (aIndex == bIndex)
                return data[aIndex];

            //Interpolate
            float m = (lastSample / interp) - aIndex;

            return (data[aIndex] * (1 - m)) + (data[bIndex] * m);
        }

        public unsafe static Complex Interpolate(Complex* data, int dataLen, float interp)
        {
            //Since we're working with indexing, it's easier to have dataLen be the max sample
            int lastSample = dataLen - 1;

            //Handle quick and easy special cases
            if (interp == 0)
                return data[0];
            if (interp == 1)
                return data[lastSample];

            //Find samples we're closest to
            int aIndex = (int)Math.Floor(lastSample * interp);
            int bIndex = (int)Math.Ceiling(lastSample * interp);

            //Handle easy case of them matching
            if (aIndex == bIndex)
                return data[aIndex];

            //Interpolate
            float m = (lastSample * interp) - aIndex;

            return (data[aIndex] * (1 - m)) + (data[bIndex] * m);
        }
    }
}
