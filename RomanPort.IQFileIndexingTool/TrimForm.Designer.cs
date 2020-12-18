namespace RomanPort.IQFileIndexingTool
{
    partial class TrimForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.startTime = new System.Windows.Forms.Label();
            this.btnMarkStart = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.endTime = new System.Windows.Forms.Label();
            this.btnMarkEnd = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.startTime);
            this.groupBox1.Controls.Add(this.btnMarkStart);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(243, 47);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "From";
            // 
            // startTime
            // 
            this.startTime.Location = new System.Drawing.Point(6, 16);
            this.startTime.Name = "startTime";
            this.startTime.Size = new System.Drawing.Size(59, 23);
            this.startTime.TabIndex = 2;
            this.startTime.Text = "00:00";
            this.startTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnMarkStart
            // 
            this.btnMarkStart.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMarkStart.Location = new System.Drawing.Point(68, 16);
            this.btnMarkStart.Name = "btnMarkStart";
            this.btnMarkStart.Size = new System.Drawing.Size(169, 23);
            this.btnMarkStart.TabIndex = 0;
            this.btnMarkStart.Text = "MARK HERE";
            this.btnMarkStart.UseVisualStyleBackColor = true;
            this.btnMarkStart.Click += new System.EventHandler(this.btnMarkStart_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.endTime);
            this.groupBox2.Controls.Add(this.btnMarkEnd);
            this.groupBox2.Location = new System.Drawing.Point(12, 65);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(243, 47);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "To";
            // 
            // endTime
            // 
            this.endTime.Location = new System.Drawing.Point(6, 16);
            this.endTime.Name = "endTime";
            this.endTime.Size = new System.Drawing.Size(59, 23);
            this.endTime.TabIndex = 2;
            this.endTime.Text = "00:00";
            this.endTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnMarkEnd
            // 
            this.btnMarkEnd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMarkEnd.Location = new System.Drawing.Point(68, 16);
            this.btnMarkEnd.Name = "btnMarkEnd";
            this.btnMarkEnd.Size = new System.Drawing.Size(169, 23);
            this.btnMarkEnd.TabIndex = 0;
            this.btnMarkEnd.Text = "MARK HERE";
            this.btnMarkEnd.UseVisualStyleBackColor = true;
            this.btnMarkEnd.Click += new System.EventHandler(this.btnMarkEnd_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Enabled = false;
            this.btnSave.Location = new System.Drawing.Point(12, 217);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(243, 23);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // TrimForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(267, 252);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "TrimForm";
            this.Text = "TrimForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TrimForm_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnMarkStart;
        private System.Windows.Forms.Label startTime;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label endTime;
        private System.Windows.Forms.Button btnMarkEnd;
        private System.Windows.Forms.Button btnSave;
    }
}