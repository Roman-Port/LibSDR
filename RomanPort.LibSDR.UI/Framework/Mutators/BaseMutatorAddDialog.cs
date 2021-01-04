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
    public partial class BaseMutatorAddDialog<T> : Form where T : unmanaged
    {
        public BaseMutatorAddDialog(IMutatorInterfaceConfig<T>[] options)
        {
            InitializeComponent();
            this.options = options;
        }

        private IMutatorInterfaceConfig<T>[] options;
        private List<RadioButton> btns;

        public BaseMutatorChained<T> selectedMutator;
        public IMutatorInterfaceConfig<T> selectedCfg;

        private void btnAdd_Click(object sender, EventArgs e)
        {
            //Find the selected option
            IMutatorInterfaceConfig<T> config = GetSelectedOption();

            //Hide this dialog and show the config screen
            Hide();
            bool success = config.ShowDialog() == DialogResult.OK;

            //If this was successful, close this and continue. Otherwise, show this window again
            if(success)
            {
                selectedMutator = config.Mutator;
                selectedCfg = config;
                DialogResult = DialogResult.OK;
                Close();
            } else
            {
                Show();
            }
        }

        private IMutatorInterfaceConfig<T> GetSelectedOption()
        {
            foreach (var b in btns)
            {
                if (b.Checked)
                    return (IMutatorInterfaceConfig<T>)b.Tag;
            }
            throw new Exception("Not Selected");
        }

        private void BaseMutatorAddDialog_Load(object sender, EventArgs e)
        {
            btns = new List<RadioButton>();
            bool first = true;
            foreach(var o in options)
            {
                RadioButton btn = new RadioButton();
                btn.Text = o.MutatorLabel;
                btn.Tag = o;
                btn.TextAlign = ContentAlignment.MiddleLeft;
                btn.Checked = first;
                first = false;
                mutatorOptions.Controls.Add(btn);
                btns.Add(btn);
            }
        }
    }
}
