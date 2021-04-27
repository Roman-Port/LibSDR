using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters.FIR
{
    public class FIRFixedPointUtil
    {
        public const int BITS = 63;
        public const int NON_DECIMAL_BITS = 32;
        public const int DECIMAL_BITS = BITS - NON_DECIMAL_BITS;
        public const int MULT_BITS = DECIMAL_BITS * 2;

        public const long SCALE_FACTOR = (long)((1UL << DECIMAL_BITS) - 1);
        public const long MAX_VALUE = 1L << NON_DECIMAL_BITS;
        public const long MIN_VALUE = -MAX_VALUE;
    }
}
