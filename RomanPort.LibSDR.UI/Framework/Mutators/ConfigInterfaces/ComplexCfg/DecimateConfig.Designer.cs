
namespace RomanPort.LibSDR.UI.Framework.Mutators.ConfigInterfaces.ComplexCfg
{
    partial class DecimateConfig
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.decimationStatus = new System.Windows.Forms.Label();
            this.btnAdd = new System.Windows.Forms.Button();
            this.decimationFactorEntry = new System.Windows.Forms.NumericUpDown();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.decimationFactorEntry)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // decimationStatus
            // 
            this.decimationStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.decimationStatus.Location = new System.Drawing.Point(6, 42);
            this.decimationStatus.Name = "decimationStatus";
            this.decimationStatus.Size = new System.Drawing.Size(203, 20);
            this.decimationStatus.TabIndex = 0;
            this.decimationStatus.Text = "label1";
            this.decimationStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdd.Location = new System.Drawing.Point(12, 170);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(215, 23);
            this.btnAdd.TabIndex = 1;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // decimationFactorEntry
            // 
            this.decimationFactorEntry.Location = new System.Drawing.Point(6, 19);
            this.decimationFactorEntry.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.decimationFactorEntry.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.decimationFactorEntry.Name = "decimationFactorEntry";
            this.decimationFactorEntry.Size = new System.Drawing.Size(203, 20);
            this.decimationFactorEntry.TabIndex = 2;
            this.decimationFactorEntry.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.decimationFactorEntry.ValueChanged += new System.EventHandler(this.decimationFactorEntry_ValueChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.decimationFactorEntry);
            this.groupBox1.Controls.Add(this.decimationStatus);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(215, 71);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Decimation Factor";
            // 
            // DecimateConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(239, 205);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnAdd);
            this.Name = "DecimateConfig";
            this.Text = "DecimateConfig";
            this.Load += new System.EventHandler(this.DecimateConfig_Load);
            ((System.ComponentModel.ISupportInitialize)(this.decimationFactorEntry)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label decimationStatus;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.NumericUpDown decimationFactorEntry;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}