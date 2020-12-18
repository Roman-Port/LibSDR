﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Framework.Extras.RDS
{
    class RDSSyndromeDetector
    {
        protected enum BlockSequence
        {
            GotA = 0,
            GotB,
            GotC,
            GotD,
            WaitBitSync,
            GotBitSync
        }

        private const int MaxCorrectableBits = 5;
        private const int CheckwordBitsCount = 10;

        private IRDSFrameReceiver evt;
        private readonly bool _useFec = true; //Turning this off disables error correction, but also stops potential corrupted packets
        private readonly UInt16[] _blocks = new UInt16[4];
        private BlockSequence _sequence = BlockSequence.WaitBitSync;
        private UInt16 _syndrome;
        private UInt32 _raw;
        private int _count;

        public RDSSyndromeDetector(IRDSFrameReceiver evt)
        {
            this.evt = evt;
        }

        public void Clock(bool b)
        {
            _raw <<= 1;
            _raw |= b ? 1u : 0u;
            _count++;

            if (_sequence == BlockSequence.WaitBitSync)
            {
                _syndrome = BuildSyndrome(_raw);
                _syndrome ^= 0x3d8;

                _sequence = _syndrome == 0 ? BlockSequence.GotA : BlockSequence.WaitBitSync;
                _blocks[0] = (UInt16)((_raw >> CheckwordBitsCount) & 0xffff);
                _count = 0;
            }

            if (_count == 26)
            {
                ProcessSyndrome();
                if (_sequence == BlockSequence.GotD)
                {
                    evt.OnRDSFrameReceived(_blocks[0], _blocks[1], _blocks[2], _blocks[3]);
                    _sequence = BlockSequence.GotBitSync;
                }

                _count = 0;
            }
        }

        private void ProcessSyndrome()
        {
            _syndrome = BuildSyndrome(_raw);
            switch (_sequence)
            {
                case BlockSequence.GotBitSync:
                    _syndrome ^= 0x3d8;
                    _sequence = BlockSequence.GotA;
                    break;
                case BlockSequence.GotA:
                    _syndrome ^= 0x3d4;
                    _sequence = BlockSequence.GotB;
                    break;
                case BlockSequence.GotB:
                    _syndrome ^= (UInt16)((_blocks[1] & 0x800) == 0 ? 0x25c : 0x3cc);
                    _sequence = BlockSequence.GotC;
                    break;
                case BlockSequence.GotC:
                    _syndrome ^= 0x258;
                    _sequence = BlockSequence.GotD;
                    break;
            }

            var blockIndex = (int)_sequence;
            if (_syndrome != 0)
            {
                if (_useFec)
                {
                    var corrected = ApplyFEC();
                    if (_syndrome != 0 || corrected > MaxCorrectableBits)
                    {
                        //Failed to correcterrors
                        _sequence = BlockSequence.WaitBitSync;
                    }
                    else
                    {
                        //Error was corrected (hopefully), apply it
                        _blocks[blockIndex] = (UInt16)(_raw & 0xffff);
                    }
                }
                else
                {
                    //Packet error, but error correction was off! Desync
                    _sequence = BlockSequence.WaitBitSync;
                }
            }
            else
            {
                //No errors, read in this block
                _blocks[blockIndex] = (UInt16)((_raw >> CheckwordBitsCount) & 0xffff);
            }
        }

        private int ApplyFEC()
        {
            const UInt16 poly = 0x5b9;
            const int errorMask = (1 << 5);

            var correction = (uint)(1 << 25);
            var correctedBitsCount = 0;

            for (var i = 0; i < 16; i++)
            {
                var st = ((_syndrome & 0x200) == 0x200);
                var bitError = (_syndrome & errorMask) == 0;
                _raw ^= (st && bitError) ? correction : 0;
                _syndrome <<= 1;
                _syndrome ^= ((st && !bitError) ? poly : (UInt16)0);
                correctedBitsCount += (st && bitError) ? 1 : 0;
                correction >>= 1;
            }
            _syndrome &= 0x3ff;
            return correctedBitsCount;
        }

        private static UInt16 BuildSyndrome(uint raw)
        {
            UInt16[] Parity =
            {
                0x2DC,
                0x16E,
                0x0B7,
                0x287,
                0x39F,
                0x313,
                0x355,
                0x376,
                0x1BB,
                0x201,
                0x3DC,
                0x1EE,
                0x0F7,
                0x2A7,
                0x38F,
                0x31B
            };

            var block = raw & 0x3ffffff;
            var syndrome = (UInt16)(block >> 16);
            for (var i = 0; i < 16; i++)
            {
                syndrome ^= ((block & 0x8000) == 0x8000) ? Parity[i] : (UInt16)0;
                block <<= 1;
            }

            return syndrome;
        }
    }
}
