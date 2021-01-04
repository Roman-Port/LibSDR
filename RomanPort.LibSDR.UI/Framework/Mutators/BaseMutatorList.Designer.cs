
namespace RomanPort.LibSDR.UI.Framework.Mutators
{
    partial class BaseMutatorList<T> where T : unmanaged
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
            this.mutatorList = new System.Windows.Forms.FlowLayoutPanel();
            this.btnAddMutator = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // mutatorList
            // 
            this.mutatorList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mutatorList.AutoScroll = true;
            this.mutatorList.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.mutatorList.Location = new System.Drawing.Point(0, 0);
            this.mutatorList.Name = "mutatorList";
            this.mutatorList.Size = new System.Drawing.Size(269, 256);
            this.mutatorList.TabIndex = 0;
            // 
            // btnAddMutator
            // 
            this.btnAddMutator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddMutator.Location = new System.Drawing.Point(0, 262);
            this.btnAddMutator.Name = "btnAddMutator";
            this.btnAddMutator.Size = new System.Drawing.Size(269, 23);
            this.btnAddMutator.TabIndex = 1;
            this.btnAddMutator.Text = "Add Mutator...";
            this.btnAddMutator.UseVisualStyleBackColor = true;
            this.btnAddMutator.Click += new System.EventHandler(this.btnAddMutator_Click);
            // 
            // BaseMutatorList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnAddMutator);
            this.Controls.Add(this.mutatorList);
            this.Name = "BaseMutatorList";
            this.Size = new System.Drawing.Size(269, 285);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel mutatorList;
        private System.Windows.Forms.Button btnAddMutator;
    }
}
