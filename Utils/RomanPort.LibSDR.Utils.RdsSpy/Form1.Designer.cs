
namespace RomanPort.LibSDR.Utils.RdsSpy
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.indRds = new System.Windows.Forms.Label();
            this.indStereo = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.freq = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.gain = new System.Windows.Forms.TrackBar();
            this.label3 = new System.Windows.Forms.Label();
            this.rdsPs = new System.Windows.Forms.TextBox();
            this.rdsPi = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.rdsCategory = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.rdsRt = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.rdsGroupChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.rdsHistory = new System.Windows.Forms.DataGridView();
            this.btnReset = new System.Windows.Forms.Button();
            this.historyPause = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.freq)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rdsGroupChart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rdsHistory)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.indRds);
            this.groupBox1.Controls.Add(this.indStereo);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.freq);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.gain);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(456, 82);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Radio Settings";
            // 
            // indRds
            // 
            this.indRds.AutoSize = true;
            this.indRds.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.indRds.Location = new System.Drawing.Point(39, 55);
            this.indRds.Name = "indRds";
            this.indRds.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.indRds.Size = new System.Drawing.Size(38, 19);
            this.indRds.TabIndex = 10;
            this.indRds.Text = "RDS";
            // 
            // indStereo
            // 
            this.indStereo.AutoSize = true;
            this.indStereo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.indStereo.Location = new System.Drawing.Point(8, 55);
            this.indStereo.Name = "indStereo";
            this.indStereo.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.indStereo.Size = new System.Drawing.Size(29, 19);
            this.indStereo.TabIndex = 9;
            this.indStereo.Text = "ST";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(84, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Gain";
            // 
            // freq
            // 
            this.freq.DecimalPlaces = 1;
            this.freq.Increment = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            this.freq.Location = new System.Drawing.Point(9, 32);
            this.freq.Maximum = new decimal(new int[] {
            1079,
            0,
            0,
            65536});
            this.freq.Minimum = new decimal(new int[] {
            879,
            0,
            0,
            65536});
            this.freq.Name = "freq";
            this.freq.Size = new System.Drawing.Size(68, 20);
            this.freq.TabIndex = 1;
            this.freq.Value = new decimal(new int[] {
            1041,
            0,
            0,
            65536});
            this.freq.ValueChanged += new System.EventHandler(this.freq_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Frequency";
            // 
            // gain
            // 
            this.gain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gain.LargeChange = 20;
            this.gain.Location = new System.Drawing.Point(87, 32);
            this.gain.Maximum = 100;
            this.gain.Name = "gain";
            this.gain.Size = new System.Drawing.Size(363, 45);
            this.gain.TabIndex = 1;
            this.gain.TickFrequency = 10;
            this.gain.Scroll += new System.EventHandler(this.gain_Scroll);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 97);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "PS Name";
            // 
            // rdsPs
            // 
            this.rdsPs.Enabled = false;
            this.rdsPs.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdsPs.Location = new System.Drawing.Point(15, 113);
            this.rdsPs.Name = "rdsPs";
            this.rdsPs.Size = new System.Drawing.Size(71, 20);
            this.rdsPs.TabIndex = 2;
            // 
            // rdsPi
            // 
            this.rdsPi.Enabled = false;
            this.rdsPi.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdsPi.Location = new System.Drawing.Point(92, 113);
            this.rdsPi.Name = "rdsPi";
            this.rdsPi.Size = new System.Drawing.Size(71, 20);
            this.rdsPi.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(89, 97);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(45, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "PI Code";
            // 
            // rdsCategory
            // 
            this.rdsCategory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rdsCategory.Enabled = false;
            this.rdsCategory.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdsCategory.Location = new System.Drawing.Point(169, 113);
            this.rdsCategory.Name = "rdsCategory";
            this.rdsCategory.Size = new System.Drawing.Size(299, 20);
            this.rdsCategory.TabIndex = 6;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(166, 97);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(49, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "Category";
            // 
            // rdsRt
            // 
            this.rdsRt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rdsRt.Enabled = false;
            this.rdsRt.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdsRt.Location = new System.Drawing.Point(15, 152);
            this.rdsRt.Name = "rdsRt";
            this.rdsRt.Size = new System.Drawing.Size(453, 20);
            this.rdsRt.TabIndex = 8;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 136);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(56, 13);
            this.label6.TabIndex = 7;
            this.label6.Text = "RadioText";
            // 
            // rdsGroupChart
            // 
            this.rdsGroupChart.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.Name = "ChartArea1";
            this.rdsGroupChart.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.rdsGroupChart.Legends.Add(legend1);
            this.rdsGroupChart.Location = new System.Drawing.Point(12, 178);
            this.rdsGroupChart.Name = "rdsGroupChart";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series1.Legend = "Legend1";
            series1.Name = "Groups";
            this.rdsGroupChart.Series.Add(series1);
            this.rdsGroupChart.Size = new System.Drawing.Size(456, 210);
            this.rdsGroupChart.TabIndex = 10;
            this.rdsGroupChart.Text = "chart1";
            // 
            // rdsHistory
            // 
            this.rdsHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rdsHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.rdsHistory.Location = new System.Drawing.Point(12, 394);
            this.rdsHistory.Name = "rdsHistory";
            this.rdsHistory.Size = new System.Drawing.Size(456, 186);
            this.rdsHistory.TabIndex = 11;
            // 
            // btnReset
            // 
            this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReset.Location = new System.Drawing.Point(372, 586);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(96, 23);
            this.btnReset.TabIndex = 12;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // historyPause
            // 
            this.historyPause.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.historyPause.AutoSize = true;
            this.historyPause.Checked = true;
            this.historyPause.CheckState = System.Windows.Forms.CheckState.Checked;
            this.historyPause.Location = new System.Drawing.Point(12, 590);
            this.historyPause.Name = "historyPause";
            this.historyPause.Size = new System.Drawing.Size(91, 17);
            this.historyPause.TabIndex = 13;
            this.historyPause.Text = "Pause History";
            this.historyPause.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(480, 621);
            this.Controls.Add(this.historyPause);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.rdsHistory);
            this.Controls.Add(this.rdsGroupChart);
            this.Controls.Add(this.rdsRt);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.rdsCategory);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.rdsPi);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.rdsPs);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "RaptorSDR RDS Spy";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.freq)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rdsGroupChart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rdsHistory)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown freq;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TrackBar gain;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox rdsPs;
        private System.Windows.Forms.TextBox rdsPi;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox rdsCategory;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox rdsRt;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label indRds;
        private System.Windows.Forms.Label indStereo;
        private System.Windows.Forms.DataVisualization.Charting.Chart rdsGroupChart;
        private System.Windows.Forms.DataGridView rdsHistory;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.CheckBox historyPause;
    }
}

