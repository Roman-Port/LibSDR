
namespace RomanPort.LibSDR.Utils.FmDemodulator
{
    partial class Form1
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
            this.label1 = new System.Windows.Forms.Label();
            this.pathInput = new System.Windows.Forms.TextBox();
            this.pathInputBrowse = new System.Windows.Forms.Button();
            this.pathOutputBrowse = new System.Windows.Forms.Button();
            this.pathOutput = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.statusBar = new System.Windows.Forms.ProgressBar();
            this.btnControl = new System.Windows.Forms.Button();
            this.statusText = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Input File";
            // 
            // pathInput
            // 
            this.pathInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pathInput.Location = new System.Drawing.Point(15, 25);
            this.pathInput.Name = "pathInput";
            this.pathInput.Size = new System.Drawing.Size(395, 20);
            this.pathInput.TabIndex = 1;
            // 
            // pathInputBrowse
            // 
            this.pathInputBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pathInputBrowse.Location = new System.Drawing.Point(416, 23);
            this.pathInputBrowse.Name = "pathInputBrowse";
            this.pathInputBrowse.Size = new System.Drawing.Size(75, 24);
            this.pathInputBrowse.TabIndex = 2;
            this.pathInputBrowse.Text = "Browse...";
            this.pathInputBrowse.UseVisualStyleBackColor = true;
            // 
            // pathOutputBrowse
            // 
            this.pathOutputBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pathOutputBrowse.Location = new System.Drawing.Point(416, 62);
            this.pathOutputBrowse.Name = "pathOutputBrowse";
            this.pathOutputBrowse.Size = new System.Drawing.Size(75, 24);
            this.pathOutputBrowse.TabIndex = 5;
            this.pathOutputBrowse.Text = "Browse...";
            this.pathOutputBrowse.UseVisualStyleBackColor = true;
            // 
            // pathOutput
            // 
            this.pathOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pathOutput.Location = new System.Drawing.Point(15, 64);
            this.pathOutput.Name = "pathOutput";
            this.pathOutput.Size = new System.Drawing.Size(395, 20);
            this.pathOutput.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Output File";
            // 
            // statusBar
            // 
            this.statusBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statusBar.Location = new System.Drawing.Point(15, 159);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(395, 23);
            this.statusBar.TabIndex = 6;
            // 
            // btnControl
            // 
            this.btnControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnControl.Location = new System.Drawing.Point(416, 159);
            this.btnControl.Name = "btnControl";
            this.btnControl.Size = new System.Drawing.Size(75, 23);
            this.btnControl.TabIndex = 7;
            this.btnControl.Text = "Start";
            this.btnControl.UseVisualStyleBackColor = true;
            this.btnControl.Click += new System.EventHandler(this.btnControl_Click);
            // 
            // statusText
            // 
            this.statusText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.statusText.AutoSize = true;
            this.statusText.Location = new System.Drawing.Point(12, 142);
            this.statusText.Name = "statusText";
            this.statusText.Size = new System.Drawing.Size(108, 13);
            this.statusText.TabIndex = 8;
            this.statusText.Text = "Choose files to begin.";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(503, 194);
            this.Controls.Add(this.statusText);
            this.Controls.Add(this.btnControl);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.pathOutputBrowse);
            this.Controls.Add(this.pathOutput);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pathInputBrowse);
            this.Controls.Add(this.pathInput);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox pathInput;
        private System.Windows.Forms.Button pathInputBrowse;
        private System.Windows.Forms.Button pathOutputBrowse;
        private System.Windows.Forms.TextBox pathOutput;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ProgressBar statusBar;
        private System.Windows.Forms.Button btnControl;
        private System.Windows.Forms.Label statusText;
    }
}

