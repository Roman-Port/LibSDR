using RomanPort.LibSDR.Radio.Mutators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RomanPort.LibSDR.UI.Framework.Mutators
{
    public abstract partial class BaseMutatorList<T> : UserControl where T : unmanaged
    {
        public BaseMutatorList()
        {
            InitializeComponent();
        }

        private MutatorSource<T> sourceMutator;
        private List<BaseMutator<T>> mutatorStack = new List<BaseMutator<T>>();
        private List<BaseMutatorListItem<T>> mutatorListItems = new List<BaseMutatorListItem<T>>();

        private BaseMutator<T> LastChainedMutator { get => mutatorStack[mutatorStack.Count - 1]; }

        public BaseMutator<T> InitMutator(float sampleRate)
        {
            //Add source mutator
            sourceMutator = new MutatorSource<T>(sampleRate);
            mutatorStack.Add(sourceMutator);

            return sourceMutator;
        }

        public void OnUserAddMutator(BaseMutatorChained<T> mutator, IMutatorInterfaceConfig<T> config)
        {
            //Add to list and stack
            mutatorStack[mutatorStack.Count - 1].Then(mutator);
            mutatorStack.Add(mutator);

            //Create new item and add it
            BaseMutatorListItem<T> item = new BaseMutatorListItem<T>(this, mutator, config);
            item.Width = mutatorList.Width - 10;
            mutatorList.Controls.Add(item);
            mutatorListItems.Add(item);
        }

        public void OnUserRemoveMutator(BaseMutatorListItem<T> item)
        {
            //Remove from chain
            item.mutator.RemoveFromChain();
            mutatorStack.Remove(item.mutator);

            //Remove view from list
            mutatorList.Controls.Remove(item);
            mutatorListItems.Remove(item);

            //Update each item
            foreach (var m in mutatorListItems)
                m.RefreshTitles();
        }

        private void btnAddMutator_Click(object sender, EventArgs e)
        {
            //Get mutators
            IMutatorInterfaceConfig<T>[] cfgs = GetMutatorConfigs(LastChainedMutator.OutputSampleRate);

            //Prompt
            var i = new BaseMutatorAddDialog<T>(cfgs);
            if (i.ShowDialog() == DialogResult.OK)
            {
                OnUserAddMutator(i.selectedMutator, i.selectedCfg);
            }
        }

        public abstract IMutatorInterfaceConfig<T>[] GetMutatorConfigs(float inputSampleRate);
    }
}
