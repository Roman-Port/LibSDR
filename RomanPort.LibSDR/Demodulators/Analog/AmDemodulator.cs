using RomanPort.LibSDR.Components;
using RomanPort.LibSDR.Components.Analog.Primitive;
using RomanPort.LibSDR.Components.Misc;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Demodulators.Analog
{
    public class AmDemodulator : IAudioDemodulator
    {
        private SnrCalculator snr;
        private AmBasebandDemodulator am;

        public AmDemodulator()
        {
            snr = new SnrCalculator();
            am = new AmBasebandDemodulator();
        }
        
        public float Configure(int bufferSize, float sampleRate, float targetOutputRate)
        {
            return sampleRate;
        }

        public unsafe int Demodulate(Complex* iq, float* audio, int count)
        {
            am.Demodulate(iq, audio, count);
            snr.AddSamples(audio, count);
            return count;
        }

        public unsafe int DemodulateStereo(Complex* iq, float* left, float* right, int count)
        {
            count = Demodulate(iq, left, count);
            Utils.Memcpy(right, left, count * sizeof(float));
            return count;
        }

        public SnrReading ReadAverageSnr()
        {
            return snr.CalculateAverageSnr();
        }

        public SnrReading ReadInstantSnr()
        {
            return snr.CalculateInstantSnr();
        }
    }
}
