using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Decimators
{
    public abstract unsafe class BaseDecimator<T> where T : unmanaged
    {
        public BaseDecimator(int decimationRate)
        {
            this.decimationRate = decimationRate;
        }

        public int DecimationRate { get => decimationRate; set => decimationRate = value; }

        private int decimationRate;
        private int skip;

        public int Process(T* ptr, int count)
        {
            //Get pointers
            T* inputPtr = ptr;
            T* outputPtr = ptr;

            //Filter
            ProcessFilter(inputPtr, count);

            //Offset by skip
            while (skip > 0 && count > 0)
            {
                inputPtr++;
                count--;
                skip--;
            }

            //Decimate
            int processed = 0;
            while(count >= decimationRate)
            {
                outputPtr[0] = inputPtr[0];
                processed++;
                outputPtr++;
                inputPtr += decimationRate;
                count -= decimationRate;
            }

            //Set skip
            skip += count;

            return processed;
        }

        protected abstract void ProcessFilter(T* ptr, int count);
    }
}
