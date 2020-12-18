
namespace RomanPort.LibSDR.UI
{
    partial class FFTDisplay
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
            this.waterfall = new RomanPort.LibSDR.UI.FFTWaterfallView();
            this.mainFft = new RomanPort.LibSDR.UI.FFTSpectrumView();
            this.SuspendLayout();
            // 
            // waterfall
            // 
            this.waterfall.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.waterfall.FftMaxDb = 0F;
            this.waterfall.FftMinDb = -80F;
            this.waterfall.Location = new System.Drawing.Point(0, 150);
            this.waterfall.Name = "waterfall";
            this.waterfall.Size = new System.Drawing.Size(526, 329);
            this.waterfall.TabIndex = 2;
            // 
            // mainFft
            // 
            this.mainFft.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainFft.FftMaxDb = 0F;
            this.mainFft.FftMinDb = -80F;
            this.mainFft.Location = new System.Drawing.Point(0, 0);
            this.mainFft.Name = "mainFft";
            this.mainFft.Size = new System.Drawing.Size(526, 144);
            this.mainFft.TabIndex = 0;
            // 
            // FFTDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.waterfall);
            this.Controls.Add(this.mainFft);
            this.Name = "FFTDisplay";
            this.Size = new System.Drawing.Size(526, 478);
            this.ResumeLayout(false);

        }

        #endregion

        private FFTSpectrumView mainFft;
        private FFTWaterfallView waterfall;
    }
}
