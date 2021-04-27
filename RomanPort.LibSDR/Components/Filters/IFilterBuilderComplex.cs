using System;
using System.Collections.Generic;
using System.Text;

namespace RomanPort.LibSDR.Components.Filters
{
    public interface IFilterBuilderComplex : IFilterBuilder
    {
        Complex[] BuildFilterComplex();
    }
}
