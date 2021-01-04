using RomanPort.LibSDR.Framework.Components.Filters;
using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework.Components.Resamplers
{
    public unsafe class ComplexResamplerPipeline : BaseResamplerPipeline<Complex>
    {
        public ComplexResamplerPipeline(float inputSampleRate, float outputSampleRate, int bufferSize, BaseResamplerPipelineAction<Complex> processBlock) : base(inputSampleRate, outputSampleRate, bufferSize, processBlock)
        {
        }

        private IQFirFilter filter;

        protected override void InitFilter(float sampleRate, int filterBw)
        {
            filter = new IQFirFilter(FilterBuilder.MakeLowPassKernel(sampleRate, 15, filterBw, WindowType.BlackmanHarris7));
        }

        private FloatArbResampler resamplerA;
        private FloatArbResampler resamplerB;

        protected override void InitArbResampler(float fromRate, float toRate)
        {
            resamplerA = new FloatArbResampler(fromRate, toRate, 2, 0);
            resamplerB = new FloatArbResampler(fromRate, toRate, 2, 1);
        }

        private ComplexCicFilter cic;
        private int decimationFactor;

        protected override void InitDecimator(int decimationFactor)
        {
            cic = new ComplexCicFilter(decimationFactor, 1, 1);
            this.decimationFactor = decimationFactor;
        }

        protected override unsafe void ProcessFilter(Complex* ptr, int count)
        {
            filter.Process(ptr, count);
        }

        protected override unsafe int ProcessArbResampler(Complex* input, Complex* output, int inputCount, int outputCount, int oldSamples, out int inputConsumed)
        {
            int outSamplesProduced;
            //resamplerFilter.Process(input + oldSamples, inputCount - oldSamples);
            resamplerA.Process((float*)input, inputCount, (float*)output, outputCount, false, out inputConsumed, out outSamplesProduced);
            resamplerB.Process((float*)input, inputCount, (float*)output, outputCount, false, out inputConsumed, out outSamplesProduced);
            return outSamplesProduced;
        }

        protected override unsafe int ProcessDecimator(Complex* input, Complex* output, int inputCount, int outputCount, int oldSamples, out int inputConsumed)
        {
            //Compute blocks
            int blocksProcessed = inputCount / decimationFactor;
            inputConsumed = blocksProcessed * decimationFactor;

            //Process
            int count = cic.Process(input, inputConsumed);
            Utils.Memcpy(output, input, count * sizeof(Complex));

            return count;
        }
    }
}
