using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework.Radio
{
    public unsafe class RadioDemodulationSession : IDisposable
    {
        private readonly IDemodulator demodulator;
        private readonly int bufferSize;
        private readonly float basebandSampleRate;
        private float demodBandwidth;
        private readonly float outputAudioSampleRate;

        //Resamples audio from basebandSampleRate to demodBandwidth
        private ComplexArbResampler basebandResampler;

        //Resampled IQ buffer. This stores baseband IQ at demodBandwidth
        private int resampledIqBufferLength;
        private UnsafeBuffer resampledIqBuffer;
        private Complex* resampledIqBufferPtr;

        //Resamples audio from demodBandwidth to outputAudioSampleRate
        private FloatArbResampler audioResamplerL;
        private FloatArbResampler audioResamplerR;

        //Raw audio demodulation buffer. This is at the sample rate of demodBandwidth
        private int audioRawBufferLength;
        private UnsafeBuffer audioRawBufferL;
        private float* audioRawBufferLPtr;
        private UnsafeBuffer audioRawBufferR;
        private float* audioRawBufferRPtr;

        public RadioDemodulationSession(IDemodulator demodulator, int bufferSize, float basebandSampleRate, float demodBandwidth, float outputAudioSampleRate)
        {
            this.demodulator = demodulator;
            this.bufferSize = bufferSize;
            this.basebandSampleRate = basebandSampleRate;
            this.demodBandwidth = demodBandwidth;
            this.outputAudioSampleRate = outputAudioSampleRate;

            //Open demodulator
            demodulator.OnAttached(bufferSize);

            //Create buffers
            SetDemodBandwidth(demodBandwidth);
        }

        public int CalculateAudioBufferSize()
        {
            return audioResamplerL.CalculateOutputBufferSize(bufferSize, demodBandwidth);
        }

        public void SetDemodBandwidth(float demodBandwidth)
        {
            //Set vars
            this.demodBandwidth = demodBandwidth;
            demodulator.OnInputSampleRateChanged(demodBandwidth);

            //Create baseband resampler
            basebandResampler = new ComplexArbResampler(basebandSampleRate, demodBandwidth, bufferSize);

            //Create resampled baseband buffer
            resampledIqBufferLength = bufferSize;
            resampledIqBuffer = UnsafeBuffer.Create(resampledIqBufferLength, sizeof(Complex));
            resampledIqBufferPtr = (Complex*)resampledIqBuffer;

            //Create raw audio resamplers
            audioResamplerL = new FloatArbResampler(demodBandwidth, outputAudioSampleRate, 1, 0);
            audioResamplerR = new FloatArbResampler(demodBandwidth, outputAudioSampleRate, 1, 0);

            //Create raw audio buffer
            audioRawBufferLength = bufferSize;
            audioRawBufferL = UnsafeBuffer.Create(audioRawBufferLength, sizeof(float));
            audioRawBufferR = UnsafeBuffer.Create(audioRawBufferLength, sizeof(float));
            audioRawBufferLPtr = (float*)audioRawBufferL;
            audioRawBufferRPtr = (float*)audioRawBufferR;
        }

        public int Process(Complex* iqInput, float* audioOutputL, float* audioOutputR, int count)
        {
            //Resample baseband
            int resampledBasebandCount = basebandResampler.Process(iqInput, resampledIqBufferPtr, count);

            //Demodulate
            int audioRead = demodulator.DemodulateStereo(resampledIqBufferPtr, audioRawBufferLPtr, audioRawBufferRPtr, resampledBasebandCount);

            //Resample
            int resampledAudioL = audioResamplerL.Process(audioRawBufferLPtr, audioRead, audioOutputL, audioRead, false);
            int resampledAudioR = audioResamplerR.Process(audioRawBufferRPtr, audioRead, audioOutputR, audioRead, false);
            if (resampledAudioL != resampledAudioR)
                throw new Exception("Resampled audio sample count for left and right channels did not match, as it was expected to!");

            return resampledAudioL;
        }

        public void Dispose()
        {
            demodulator.Dispose();
            basebandResampler.Dispose();
            resampledIqBuffer.Dispose();
            audioRawBufferL.Dispose();
            audioRawBufferR.Dispose();
        }
    }
}
