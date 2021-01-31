using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RomanPort.LibSDR.Components;
using RomanPort.LibSDR.Radio.Mutators;
using RomanPort.LibSDR.Radio.Mutators.Chain.ComplexMutators;

namespace RomanPort.LibSDR.UI.Framework.Mutators.ConfigInterfaces.ComplexCfg
{
    public partial class DecimateConfig : Form, IMutatorInterfaceConfig<Complex>
    {
        public DecimateConfig(float sampleRate)
        {
            InitializeComponent();
            this.sampleRate = sampleRate;
        }

        private float sampleRate;
        public string MutatorLabel => "Decimation Mutator";

        private ComplexDecimateMutator mutator;
        public BaseMutatorChained<Complex> Mutator { get => mutator; }

        private float OutputSampleRate { get => sampleRate / (int)decimationFactorEntry.Value; }

        public void GetMutatorTitles(out string title, out string sub)
        {
            title = "Decimation Mutator";
            sub = $"[/{mutator.DecimationFactor}] {Math.Round(mutator.OutputSampleRate)}";
        }

        private void decimationFactorEntry_ValueChanged(object sender, EventArgs e)
        {
            UpdateFactorText();
        }

        private void UpdateFactorText()
        {
            decimationStatus.Text = $"{Math.Round(sampleRate)} / {decimationFactorEntry.Value} -> {Math.Round(OutputSampleRate)}";
        }

        private void DecimateConfig_Load(object sender, EventArgs e)
        {
            UpdateFactorText();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            mutator = new ComplexDecimateMutator((int)decimationFactorEntry.Value);
            DialogResult = DialogResult.OK;
            return;
        }
    }
}
