using RomanPort.LibSDR.Framework.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Resamplers.Arbitrary
{
    public unsafe class ArbitraryFloatResampler : IDisposable
    {
        public readonly double resamplingRate;
        public readonly int bufferSize;

        private UnsafeBuffer bufferInput;
        private float* bufferInputPtr;
        private int bufferInputUsage;
        private int bufferInputSize;

        private UnsafeBuffer bufferOutput;
        private float* bufferOutputPtr;
        private int bufferOutputUsage;
        private int bufferOutputSize;

        private float[] _imp;
        private float[] _impD;
        private float _lpScl;
        private double _maxFactor;
        private double _minFactor;
        private int _nmult;
        private int _nwing;
        private float[] _x;
        private int _xoff;
        private int _xSize;
        private float[] _y;
        private double _time;
        private int _xp; // current "now"-sample pointer for input
        private int _xread; // position to put new samples
        private int _yp;

        public ArbitraryFloatResampler(double inSampleRate, double outSampleRate, int bufferSize)
        {
            this.resamplingRate = outSampleRate / inSampleRate;
            this.bufferSize = bufferSize;

            //Create input buffer
            bufferInputSize = bufferSize * 2;
            bufferInput = UnsafeBuffer.Create(bufferInputSize, sizeof(float));
            bufferInputPtr = (float*)bufferInput;

            //Create output buffer
            bufferOutputSize = (int)((resamplingRate * bufferSize) + 2);
            bufferOutput = UnsafeBuffer.Create(bufferOutputSize, sizeof(float));
            bufferOutputPtr = (float*)bufferOutput;
            Init(true, resamplingRate, resamplingRate);
        }

        public void Input(float* ptr, int count, int incomingChannels)
        {
            //Transfer to the input buffer
            for(int i = 0; i < count; i+=1)
            {
                //Make sure there is space
                if (bufferInputUsage == bufferInputSize)
                    throw new Exception("Not enough room in the input buffer!");

                //Transfer
                bufferInputPtr[bufferInputUsage] = ptr[i * incomingChannels];

                //Update
                bufferInputUsage++;
            }

            //Get space in the output buffer and ensure it isn't full
            int bufferOutputAvailable = bufferOutputSize - bufferOutputUsage;
            if (bufferOutputAvailable == 0)
                throw new Exception("Output buffer is full! Make sure you are calling Output.");

            //Process
            Process(bufferInputPtr, bufferInputUsage, bufferOutputPtr + bufferOutputUsage, bufferOutputAvailable, false, out int inputBufferUsed, out int outputBufferUsed);

            //Transfer unused bytes in the input buffer to the beginning and update its state
            Utils.Memcpy(bufferInputPtr, bufferInputPtr + inputBufferUsed, (bufferInputUsage - inputBufferUsed) * sizeof(float));
            bufferInputUsage -= inputBufferUsed;

            //Update state of output buffer
            bufferOutputUsage += outputBufferUsed;
        }
        
        public int Output(float* ptr, int maxCount, int outgoingChannels)
        {
            //Determine how much we can transfer
            int transferrable = Math.Min(maxCount, bufferOutputUsage);

            //Transfer to the output buffer (respecting channels)
            for (int i = 0; i < transferrable; i++)
                ptr[i * outgoingChannels] = bufferOutputPtr[i];

            //Transfer unsued bytes to the beginning and update the state
            Utils.Memcpy(bufferOutputPtr, bufferOutputPtr + transferrable, (bufferOutputUsage - transferrable) * sizeof(float));
            bufferOutputUsage -= transferrable;

            return transferrable;
        }

        public int CalculateOutputBufferSize(float bufferLength, float incomingSampleRate)
        {
            return (int)((incomingSampleRate * resamplingRate * bufferLength) + 2);
        }

        private void Init(bool highQuality, double minFactor, double maxFactor)
        {
            if (minFactor <= 0.0 || maxFactor <= 0.0)
            {
                throw new ArgumentException("minFactor and maxFactor must be positive");
            }
            if (maxFactor < minFactor)
            {
                throw new ArgumentException("minFactor must be less or equal to maxFactor");
            }

            _minFactor = minFactor;
            _maxFactor = maxFactor;
            _nmult = highQuality ? 35 : 11;
            _lpScl = 1.0f;
            _nwing = ArbitraryFilterKit.Npc * (_nmult - 1) / 2; // # of filter coeffs in right wing

            const double rolloff = 0.90;
            const double beta = 6;

            var imp64 = new double[_nwing];

            ArbitraryFilterKit.LrsLpFilter(imp64, _nwing, 0.5 * rolloff, beta, ArbitraryFilterKit.Npc);
            _imp = new float[_nwing];
            _impD = new float[_nwing];

            for (var i = 0; i < _nwing; i++)
            {
                _imp[i] = (float)imp64[i];
            }

            for (var i = 0; i < _nwing - 1; i++)
            {
                _impD[i] = _imp[i + 1] - _imp[i];
            }

            _impD[_nwing - 1] = -_imp[_nwing - 1];

            var xoffMin = (int)(((_nmult + 1) / 2.0) * Math.Max(1.0, 1.0 / minFactor) + 10);
            var xoffMax = (int)(((_nmult + 1) / 2.0) * Math.Max(1.0, 1.0 / maxFactor) + 10);
            _xoff = Math.Max(xoffMin, xoffMax);

            _xSize = Math.Max(2 * _xoff + 10, 4096);
            _x = new float[_xSize + _xoff];
            _xp = _xoff;
            _xread = _xoff;

            var ySize = (int)(_xSize * maxFactor + 2.0);
            _y = new float[ySize];
            _yp = 0;

            _time = _xoff;
        }

        private int GetIOArrayIndex(int i, int samplesUsed)
        {
            int baseIndex = i + samplesUsed;
            return baseIndex;
        }

        private int Process(float* inBuffer, int inBufferLen, float* outBuffer, int outBufferLen, bool lastBatch)
        {
            Process(inBuffer, inBufferLen, outBuffer, outBufferLen, lastBatch, out int inBufferUsed, out int outSampleCount);
            return outSampleCount;
        }

        private bool Process(float* inBuffer, int inBufferLen, float* outBuffer, int outBufferLen, bool lastBatch, out int inBufferUsed, out int outSampleCount)
        {
            double factor = resamplingRate;
            if (factor < _minFactor || factor > _maxFactor)
            {
                throw new ArgumentException("factor" + factor + "is not between minFactor=" + _minFactor + " and maxFactor=" + _maxFactor);
            }

            float[] imp = _imp;
            float[] impD = _impD;
            float lpScl = _lpScl;
            int nwing = _nwing;
            bool interpFilt = false;

            inBufferUsed = 0;
            outSampleCount = 0;

            if ((_yp != 0) && (outBufferLen - outSampleCount) > 0)
            {
                int len = Math.Min(outBufferLen - outSampleCount, _yp);

                //Write output
                for (int i = 0; i < len; i++)
                    outBuffer[GetIOArrayIndex(i, outSampleCount)] = _y[i];

                outSampleCount += len;
                for (int i = 0; i < _yp - len; i++)
                {
                    _y[i] = _y[i + len];
                }
                _yp -= len;
            }

            if (_yp != 0)
            {
                return inBufferUsed == 0 && outSampleCount == 0;
            }

            if (factor < 1)
            {
                lpScl = (float)(lpScl * factor);
            }

            while (true)
            {
                int len = _xSize - _xread;

                if (len >= inBufferLen - inBufferUsed)
                {
                    len = inBufferLen - inBufferUsed;
                }

                //Read input
                for (int i = 0; i < len; i++)
                    _x[i + _xread] = inBuffer[GetIOArrayIndex(i, inBufferUsed)];

                inBufferUsed += len;
                _xread += len;

                int nx;
                if (lastBatch && (inBufferUsed == inBufferLen))
                {
                    nx = _xread - _xoff;
                    for (int i = 0; i < _xoff; i++)
                    {
                        _x[_xread + i] = 0;
                    }
                }
                else
                {
                    nx = _xread - 2 * _xoff;
                }

                if (nx <= 0)
                {
                    break;
                }

                int nout;
                if (factor >= 1)
                {
                    nout = LrsSrcUp(_x, _y, factor, nx, nwing, lpScl, imp, impD, interpFilt);
                }
                else
                {
                    nout = LrsSrcUd(_x, _y, factor, nx, nwing, lpScl, imp, impD, interpFilt);
                }

                _time -= nx;
                _xp += nx;

                int ncreep = (int)(_time) - _xoff;
                if (ncreep != 0)
                {
                    _time -= ncreep;
                    _xp += ncreep;
                }

                int nreuse = _xread - (_xp - _xoff);

                for (int i = 0; i < nreuse; i++)
                {
                    _x[i] = _x[i + (_xp - _xoff)];
                }

                _xread = nreuse;
                _xp = _xoff;

                _yp = nout;

                if (_yp != 0 && (outBufferLen - outSampleCount) > 0)
                {
                    len = Math.Min(outBufferLen - outSampleCount, _yp);

                    //Write output
                    for (int i = 0; i < len; i++)
                        outBuffer[GetIOArrayIndex(i, outSampleCount)] = _y[i];
                    outSampleCount += len;
                    for (int i = 0; i < _yp - len; i++)
                    {
                        _y[i] = _y[i + len];
                    }
                    _yp -= len;
                }

                if (_yp != 0)
                {
                    break;
                }
            }

            return inBufferUsed == 0 && outSampleCount == 0;
        }

        private int LrsSrcUp(float[] x, float[] y, double factor, int nx, int nwing, float lpScl, float[] imp, float[] impD, bool interp)
        {
            float[] xpArray = x;
            int xpIndex;

            float[] ypArray = y;
            int ypIndex = 0;

            float v;

            double currentTime = _time;
            double dt;
            double endTime;

            dt = 1.0 / factor;

            endTime = currentTime + nx;
            while (currentTime < endTime)
            {
                double leftPhase = currentTime - Math.Floor(currentTime);
                double rightPhase = 1.0 - leftPhase;

                xpIndex = (int)currentTime;
                v = ArbitraryFilterKit.LrsFilterUp(imp, impD, nwing, interp, xpArray, xpIndex++, leftPhase, -1);
                v += ArbitraryFilterKit.LrsFilterUp(imp, impD, nwing, interp, xpArray, xpIndex, rightPhase, 1);
                v *= lpScl;

                ypArray[ypIndex++] = v;
                currentTime += dt;
            }

            _time = currentTime;

            return ypIndex;
        }

        private int LrsSrcUd(float[] x, float[] y, double factor, int nx, int nwing, float lpScl, float[] imp, float[] impD, bool interp)
        {
            float[] xpArray = x;
            int xpIndex;

            float[] ypArray = y;
            int ypIndex = 0;

            float v;

            double currentTime = _time;
            double dh;
            double dt;
            double endTime;

            dt = 1.0 / factor;

            dh = Math.Min(ArbitraryFilterKit.Npc, factor * ArbitraryFilterKit.Npc);

            endTime = currentTime + nx;
            while (currentTime < endTime)
            {
                double leftPhase = currentTime - Math.Floor(currentTime);
                double rightPhase = 1.0 - leftPhase;

                xpIndex = (int)currentTime;
                v = ArbitraryFilterKit.LrsFilterUd(imp, impD, nwing, interp, xpArray, xpIndex++, leftPhase, -1, dh);
                v += ArbitraryFilterKit.LrsFilterUd(imp, impD, nwing, interp, xpArray, xpIndex, rightPhase, 1, dh);
                v *= lpScl;

                ypArray[ypIndex++] = v;
                currentTime += dt;
            }

            _time = currentTime;
            return ypIndex;
        }

        public void Dispose()
        {
            bufferInput.Dispose();
            bufferOutput.Dispose();
        }
    }
}
