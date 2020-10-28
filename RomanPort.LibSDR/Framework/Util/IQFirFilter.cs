using RomanPort.LibSDR.Framework.Extras;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework.Util
{
    public unsafe sealed class IQFirFilter : IDisposable
    {
        private readonly FirFilter _rFilter;
        private readonly FirFilter _iFilter;

        //private ThreadedWorker worker;

        public IQFirFilter(float[] coefficients, int decimationFactor = 1)
        {
            _rFilter = new FirFilter(coefficients, decimationFactor);
            _iFilter = new FirFilter(coefficients, decimationFactor);
            //worker = new ThreadedWorker();
        }

        ~IQFirFilter()
        {
            Dispose();
        }

        public void Dispose()
        {
            _rFilter.Dispose();
            _iFilter.Dispose();
            //worker.Stop();
            GC.SuppressFinalize(this);
        }

        public void Process(Complex* iq, int length)
        {
            var ptr = (float*)iq;
            //worker.StartExecute(new ThreadedCmd(ptr, length, _iFilter));
            _rFilter.ProcessInterleaved(ptr, length);
            _iFilter.ProcessInterleaved(ptr, length);
            //worker.EndExecute();
        }

        public void SetCoefficients(float[] coefficients)
        {
            _rFilter.SetCoefficients(coefficients);
            _iFilter.SetCoefficients(coefficients);
        }

        private struct ThreadedCmd
        {
            public float* ptr;
            public int len;
            public FirFilter filter;

            public ThreadedCmd(float* ptr, int len, FirFilter filter)
            {
                this.ptr = ptr;
                this.len = len;
                this.filter = filter;
            }
        }

        private class ThreadedWorker : ThreadedCommandExecuter<ThreadedCmd, ThreadedCmd>
        {
            public override ThreadedCmd Compute(ThreadedCmd cmd)
            {
                cmd.filter.ProcessInterleaved(cmd.ptr + 1, cmd.len);
                return cmd;
            }
        }
    }
}
