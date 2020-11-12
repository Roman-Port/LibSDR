using RomanPort.LibSDR.Framework.Extras;
using RomanPort.LibSDR.Framework.Multithreading;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework.Util
{
    public unsafe sealed class IQFirFilter : IDisposable
    {
        private readonly FirFilter _rFilter;
        private readonly FirFilter _iFilter;
        private readonly MultithreadWorker multithread;

        //private ThreadedWorker worker;

        public IQFirFilter(float[] coefficients, MultithreadWorker multithread = null, int decimationFactor = 1)
        {
            _rFilter = new FirFilter(coefficients, decimationFactor);
            _iFilter = new FirFilter(coefficients, decimationFactor);
            this.multithread = multithread;
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
            GC.SuppressFinalize(this);
        }

        public void Process(Complex* iq, int length)
        {
            var ptr = (float*)iq;
            if (multithread == null)
            {
                //Run both on this thread
                _rFilter.ProcessInterleaved(ptr, length);
                _iFilter.ProcessInterleaved(ptr + 1, length);
            } else
            {
                //Run one on other thread while we process
                multithread.BeginWork(() =>
                {
                    _rFilter.ProcessInterleaved(ptr, length);
                    return null;
                });
                _iFilter.ProcessInterleaved(ptr + 1, length);
                multithread.EndWork();
            }
        }

        public void SetCoefficients(float[] coefficients)
        {
            _rFilter.SetCoefficients(coefficients);
            _iFilter.SetCoefficients(coefficients);
        }
    }
}
