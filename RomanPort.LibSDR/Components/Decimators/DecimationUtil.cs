using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Decimators
{
    public static class DecimationUtil
    {
        public static int CalculateDecimationRate(float inputSampleRate, float desiredOutputSampleRate, out float actualOutputSampleRate)
        {
            //Calculate the rate by finding the LOWEST we can go without it becoming a rate lower than the desired rate
            int decimationRate = 1;
            while (inputSampleRate / (decimationRate + 1) >= desiredOutputSampleRate)
            {
                decimationRate++;
            }

            //Determine the actual output sample rate
            actualOutputSampleRate = inputSampleRate / decimationRate;

            return decimationRate;
        }
    }
}
