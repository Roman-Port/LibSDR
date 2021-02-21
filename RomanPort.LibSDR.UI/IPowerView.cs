using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanPort.LibSDR.UI
{
    public interface IPowerView
    {
        float FftOffset { get; set; }
        float FftRange { get; set; }
        unsafe void WritePowerSamples(float* power, int powerLen);
        void DrawFrame();
        unsafe void RawDrawFrame(float* power);
    }
}
