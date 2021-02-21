
namespace RomanPort.LibSDR.UI
{
    partial class SpectrumWaterfallView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.waterfallView = new RomanPort.LibSDR.UI.WaterfallView();
            this.spectrumView = new RomanPort.LibSDR.UI.SpectrumView();
            this.SuspendLayout();
            // 
            // waterfallView
            // 
            this.waterfallView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.waterfallView.BackColor = System.Drawing.SystemColors.ControlDark;
            this.waterfallView.FftOffset = 0F;
            this.waterfallView.FftRange = 100F;
            this.waterfallView.Location = new System.Drawing.Point(0, 349);
            this.waterfallView.Name = "waterfallView";
            this.waterfallView.Size = new System.Drawing.Size(725, 174);
            this.waterfallView.TabIndex = 1;
            // 
            // spectrumView
            // 
            this.spectrumView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.spectrumView.BackColor = System.Drawing.SystemColors.ControlDark;
            this.spectrumView.FftOffset = 0F;
            this.spectrumView.FftRange = 100F;
            this.spectrumView.Location = new System.Drawing.Point(0, 0);
            this.spectrumView.Name = "spectrumView";
            this.spectrumView.Size = new System.Drawing.Size(725, 343);
            this.spectrumView.TabIndex = 0;
            // 
            // SpectrumWaterfallView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.waterfallView);
            this.Controls.Add(this.spectrumView);
            this.Name = "SpectrumWaterfallView";
            this.Size = new System.Drawing.Size(725, 523);
            this.Load += new System.EventHandler(this.SpectrumWaterfallView_Load);
            this.Resize += new System.EventHandler(this.SpectrumWaterfallView_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private SpectrumView spectrumView;
        private WaterfallView waterfallView;
    }
}
