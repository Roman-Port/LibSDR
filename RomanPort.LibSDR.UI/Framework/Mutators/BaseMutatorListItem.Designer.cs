
namespace RomanPort.LibSDR.UI.Framework.Mutators
{
    partial class BaseMutatorListItem<T> where T : unmanaged
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
            this.btnRemove = new System.Windows.Forms.Button();
            this.mutatorTitle = new System.Windows.Forms.Label();
            this.mutatorSub = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnRemove
            // 
            this.btnRemove.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemove.Location = new System.Drawing.Point(382, 6);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(59, 22);
            this.btnRemove.TabIndex = 0;
            this.btnRemove.Text = "Remove";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // mutatorTitle
            // 
            this.mutatorTitle.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mutatorTitle.Location = new System.Drawing.Point(3, 2);
            this.mutatorTitle.Name = "mutatorTitle";
            this.mutatorTitle.Size = new System.Drawing.Size(358, 15);
            this.mutatorTitle.TabIndex = 1;
            this.mutatorTitle.Text = "label1";
            this.mutatorTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // mutatorSub
            // 
            this.mutatorSub.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mutatorSub.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.mutatorSub.Location = new System.Drawing.Point(3, 17);
            this.mutatorSub.Name = "mutatorSub";
            this.mutatorSub.Size = new System.Drawing.Size(358, 15);
            this.mutatorSub.TabIndex = 2;
            this.mutatorSub.Text = "label1";
            this.mutatorSub.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // BaseMutatorListItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mutatorSub);
            this.Controls.Add(this.mutatorTitle);
            this.Controls.Add(this.btnRemove);
            this.Name = "BaseMutatorListItem";
            this.Size = new System.Drawing.Size(442, 35);
            this.Load += new System.EventHandler(this.BaseMutatorListItem_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Label mutatorTitle;
        private System.Windows.Forms.Label mutatorSub;
    }
}
