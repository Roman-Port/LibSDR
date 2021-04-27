using RomanPort.LibSDR.Components.General;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters.FIR
{
    public class MultithreadedComplexFirFilter : ComplexFirFilter
    {
        public MultithreadedComplexFirFilter(BackgroundWorker worker, IFilterBuilderReal builder, int decimation = 1) : base(builder, decimation)
        {
            this.worker = worker;
        }

        public MultithreadedComplexFirFilter(BackgroundWorker worker, IFilterBuilderComplex builder, int decimation = 1) : base(builder, decimation)
        {
            this.worker = worker;
        }

        public MultithreadedComplexFirFilter(BackgroundWorker worker, Complex[] coeffs, int decimation = 1) : base(coeffs, decimation)
        {
            this.worker = worker;
        }

        public MultithreadedComplexFirFilter(BackgroundWorker worker, float[] coeffs, int decimation = 1) : base(coeffs, decimation)
        {
            this.worker = worker;
        }

        public MultithreadedComplexFirFilter(BackgroundWorker worker, float[] coeffsA, float[] coeffsB, int decimation = 1) : base(coeffsA, coeffsB, decimation)
        {
            this.worker = worker;
        }

        private BackgroundWorker worker;

        public override unsafe int Process(Complex* input, Complex* output, int count)
        {
            //Schedule work
            worker.StartWork(() => a.Process(((float*)input), ((float*)output), count, 2));
            
            //Process other
            int processed = b.Process(((float*)input) + 1, ((float*)output) + 1, count, 2);

            //Wait for work to end
            worker.EndWork();

            return processed;
        }
    }
}
