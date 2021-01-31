using RomanPort.LibSDR.Components;
using RomanPort.LibSDR.Radio.Mutators;
using RomanPort.LibSDR.UI.Framework.Mutators;
using RomanPort.LibSDR.UI.Framework.Mutators.ConfigInterfaces.ComplexCfg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanPort.LibSDR.UI
{
    public class MutatorListComplex : BaseMutatorList<Complex>
    {
        public override IMutatorInterfaceConfig<Complex>[] GetMutatorConfigs(float inputSampleRate)
        {
            return new IMutatorInterfaceConfig<Complex>[]
            {
                new DecimateConfig(inputSampleRate)
            };
        }
    }
}
