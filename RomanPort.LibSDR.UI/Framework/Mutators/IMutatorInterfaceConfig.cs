using RomanPort.LibSDR.Radio.Mutators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RomanPort.LibSDR.UI.Framework.Mutators
{
    public interface IMutatorInterfaceConfig<T> where T : unmanaged
    {
        BaseMutatorChained<T> Mutator { get; }
        DialogResult ShowDialog();
        void GetMutatorTitles(out string title, out string sub);
        string MutatorLabel { get; }
    }
}
