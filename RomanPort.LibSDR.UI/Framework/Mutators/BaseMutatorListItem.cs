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
    public partial class BaseMutatorListItem<T> : UserControl where T : unmanaged
    {
        public BaseMutatorListItem(BaseMutatorList<T> list, BaseMutatorChained<T> mutator, IMutatorInterfaceConfig<T> config)
        {
            InitializeComponent();
            this.list = list;
            this.mutator = mutator;
            this.config = config;
        }

        public void RefreshTitles()
        {
            config.GetMutatorTitles(out string mutatorTitle, out string mutatorSub);
            this.mutatorTitle.Text = mutatorTitle;
            this.mutatorSub.Text = mutatorSub;
        }

        public BaseMutatorList<T> list;
        public BaseMutatorChained<T> mutator;
        public IMutatorInterfaceConfig<T> config;

        private void btnRemove_Click(object sender, EventArgs e)
        {
            list.OnUserRemoveMutator(this);
        }

        private void BaseMutatorListItem_Load(object sender, EventArgs e)
        {
            RefreshTitles();
        }
    }
}
