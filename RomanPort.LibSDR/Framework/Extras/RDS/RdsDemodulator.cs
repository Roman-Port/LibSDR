using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework.Extras.RDS
{
    public delegate void RDSFrameReceivedEventArgs(ushort a, ushort b, ushort c, ushort d);

    public unsafe class RdsDemodulator : IDisposable
    {
        private RDSSyndromeDetector detector;

        private UnsafeBuffer _rawBuffer;
        private Complex* _rawPtr;
        private UnsafeBuffer _magBuffer;
        private float* _magPtr;
        private UnsafeBuffer _dataBuffer;
        private float* _dataPtr;
        private IQDecimator _decimator;
        private double _demodulationSampleRate;
        private int _decimationFactor;

        private readonly Pll* _pll;
        private readonly UnsafeBuffer _pllBuffer;
        private readonly Oscillator* _osc;
        private readonly UnsafeBuffer _oscBuffer;
        private readonly IirFilter* _syncFilter;
        private readonly UnsafeBuffer _syncFilterBuffer;

        private readonly IQFirFilter _baseBandFilter = new IQFirFilter(null);
        private readonly FirFilter _matchedFilter = new FirFilter();

        private float _lastSync;
        private float _lastData;
        private float _lastSyncSlope;
        private bool _lastBit;

        private const int PllDefaultFrequency = 57000;
        private const int PllRange = 12;
        private const int PllBandwith = 1;
        private const float PllZeta = 0.707f;
        private const float PllLockTime = 0.5f;
        private const float PllLockThreshold = 3.2f;
        private const float RdsBitRate = 1187.5f;

        public event RDSFrameReceivedEventArgs OnRDSFrameReceived;

        public RdsDemodulator()
        {
            this.detector = new RDSSyndromeDetector();
            detector.OnRDSFrameReceived += Detector_OnRDSFrameReceived;
            
            _pllBuffer = UnsafeBuffer.Create(sizeof(Pll));
            _pll = (Pll*)_pllBuffer;

            _oscBuffer = UnsafeBuffer.Create(sizeof(Oscillator));
            _osc = (Oscillator*)_oscBuffer;

            _syncFilterBuffer = UnsafeBuffer.Create(sizeof(IirFilter));
            _syncFilter = (IirFilter*)_syncFilterBuffer;
        }

        private void Detector_OnRDSFrameReceived(ushort a, ushort b, ushort c, ushort d)
        {
            OnRDSFrameReceived?.Invoke(a, b, c, d);
        }

        public void Dispose()
        {
            _rawBuffer.Dispose();
            _magBuffer.Dispose();
            _dataBuffer.Dispose();
            _decimator.Dispose();
            _pllBuffer.Dispose();
            _oscBuffer.Dispose();
            _syncFilterBuffer.Dispose();
            _baseBandFilter.Dispose();
            _matchedFilter.Dispose();
        }

        public void Configure(float sampleRate)
        {
            _osc->SampleRate = sampleRate;
            _osc->Frequency = PllDefaultFrequency;

            var decimationStageCount = 0;
            while (sampleRate >= 20000 * Math.Pow(2.0, decimationStageCount))
            {
                decimationStageCount++;
            }

            _decimator = new IQDecimator(decimationStageCount, sampleRate, true);
            _decimationFactor = (int)Math.Pow(2.0, decimationStageCount);
            _demodulationSampleRate = sampleRate / _decimationFactor;

            var coefficients = FilterBuilder.MakeLowPassKernel(_demodulationSampleRate, 200, 2500, WindowType.BlackmanHarris4);
            _baseBandFilter.SetCoefficients(coefficients);

            _pll->SampleRate = (float)_demodulationSampleRate;
            _pll->DefaultFrequency = 0;
            _pll->Range = PllRange;
            _pll->Bandwidth = PllBandwith;
            _pll->Zeta = PllZeta;
            _pll->LockTime = PllLockTime;
            _pll->LockThreshold = PllLockThreshold;

            var matchedFilterLength = (int)(_demodulationSampleRate / RdsBitRate) | 1;
            coefficients = FilterBuilder.MakeSin(_demodulationSampleRate, RdsBitRate, matchedFilterLength);
            _matchedFilter.SetCoefficients(coefficients);

            _syncFilter->Init(IirFilterType.BandPass, RdsBitRate, _demodulationSampleRate, 500);
        }

        public void CreateBuffers(int bufferLength)
        {
            //Raw buffer
            _rawBuffer = UnsafeBuffer.Create(bufferLength, sizeof(Complex));
            _rawPtr = (Complex*)_rawBuffer;

            //Mag buffer
            _magBuffer = UnsafeBuffer.Create(bufferLength, sizeof(float));
            _magPtr = (float*)_magBuffer;

            //Data buffer
            _dataBuffer = UnsafeBuffer.Create(bufferLength, sizeof(float));
            _dataPtr = (float*)_dataBuffer;
        }

        public void Process(float* baseBand, int length)
        {
            // Downconvert
            for (var i = 0; i < length; i++)
            {
                _osc->Tick();
                _rawPtr[i] = _osc->Phase * baseBand[i];
            }

            // Decimate
            _decimator.Process(_rawPtr, length);
            length /= _decimationFactor;

            // Filter
            //THIS LINE WAS PREVIOUSLY DISABLED BY ME because of a bug in filtering. I believe it's fixed now, but I will leave this note just in case
            _baseBandFilter.Process(_rawPtr, length);            

            // PLL
            for (var i = 0; i < length; i++)
            {
                _dataPtr[i] = _pll->Process(_rawPtr[i]).Imag;
            }

            // Matched filter
            _matchedFilter.Process(_dataPtr, length);

            // Recover signal energy to sustain the oscillation in the IIR
            for (var i = 0; i < length; i++)
            {
                _magPtr[i] = Math.Abs(_dataPtr[i]);
            }

            // Synchronize to RDS bitrate
            _syncFilter->Process(_magPtr, length);

            // Detect RDS bits
            for (int i = 0; i < length; i++)
            {
                var data = _dataPtr[i];
                var syncVal = _magPtr[i];
                var slope = syncVal - _lastSync;
                _lastSync = syncVal;
                if (slope < 0.0f && _lastSyncSlope * slope < 0.0f)
                {
                    bool bit = _lastData > 0;
                    ClockBit(bit ^ _lastBit);
                    _lastBit = bit;
                }
                _lastData = data;
                _lastSyncSlope = slope;
            }
        }

        private void ClockBit(bool b)
        {
            detector.Clock(b);
        }
    }
}
