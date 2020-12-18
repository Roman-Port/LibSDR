namespace RomanPort.IQFileIndexingTool
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
            this.fileList = new System.Windows.Forms.ListView();
            this.filename = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.filesize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.metaNotes = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.metaSuffix = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.metaPrefix = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.metaTitle = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.metaArtist = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.metaCall = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.metaTime = new System.Windows.Forms.DateTimePicker();
            this.panel2 = new System.Windows.Forms.Panel();
            this.alreadyExistsBadge = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.metaHash = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.metaSize = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.metaRadio = new System.Windows.Forms.ComboBox();
            this.metaSampleRate = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.trackProgress = new System.Windows.Forms.TrackBar();
            this.btnSave = new System.Windows.Forms.Button();
            this.timerText = new System.Windows.Forms.Label();
            this.waveformView = new System.Windows.Forms.PictureBox();
            this.btnDelete = new System.Windows.Forms.Button();
            this.rdsRadioText = new System.Windows.Forms.TextBox();
            this.rdsPsName = new System.Windows.Forms.TextBox();
            this.sliderPlaybackVol = new System.Windows.Forms.TrackBar();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.sliderIfGain = new System.Windows.Forms.TrackBar();
            this.label13 = new System.Windows.Forms.Label();
            this.sliderFftRange = new System.Windows.Forms.TrackBar();
            this.mpxSpectrumView = new RomanPort.LibSDR.UI.FFTSpectrumView();
            this.fftSpectrumView1 = new RomanPort.LibSDR.UI.FFTSpectrumView();
            this.btnTrim = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackProgress)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.waveformView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sliderPlaybackVol)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sliderIfGain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sliderFftRange)).BeginInit();
            this.SuspendLayout();
            // 
            // fileList
            // 
            this.fileList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.fileList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.filename,
            this.filesize});
            this.fileList.FullRowSelect = true;
            this.fileList.HideSelection = false;
            this.fileList.Location = new System.Drawing.Point(12, 12);
            this.fileList.Name = "fileList";
            this.fileList.Size = new System.Drawing.Size(263, 339);
            this.fileList.TabIndex = 1;
            this.fileList.UseCompatibleStateImageBehavior = false;
            this.fileList.View = System.Windows.Forms.View.Details;
            this.fileList.SelectedIndexChanged += new System.EventHandler(this.fileList_SelectedIndexChanged);
            // 
            // filename
            // 
            this.filename.Text = "Filename";
            this.filename.Width = 177;
            // 
            // filesize
            // 
            this.filesize.Text = "Size";
            this.filesize.Width = 82;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 19);
            this.label1.TabIndex = 2;
            this.label1.Text = "Station";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.metaNotes);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.metaSuffix);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.metaPrefix);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.metaTitle);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.metaArtist);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.metaCall);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(12, 357);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(939, 48);
            this.panel1.TabIndex = 3;
            // 
            // metaNotes
            // 
            this.metaNotes.Location = new System.Drawing.Point(680, 20);
            this.metaNotes.Name = "metaNotes";
            this.metaNotes.Size = new System.Drawing.Size(256, 20);
            this.metaNotes.TabIndex = 15;
            this.metaNotes.TextChanged += new System.EventHandler(this.metaNotes_TextChanged);
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(677, -2);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(174, 19);
            this.label7.TabIndex = 14;
            this.label7.Text = "Notes";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // metaSuffix
            // 
            this.metaSuffix.FormattingEnabled = true;
            this.metaSuffix.Items.AddRange(new object[] {
            "Station slogan",
            "Station promo",
            "Call Letters",
            "DJ, no clip",
            "DJ, clip",
            "Song",
            "Advertisement",
            "Artist Intro",
            "N/A"});
            this.metaSuffix.Location = new System.Drawing.Point(564, 20);
            this.metaSuffix.Name = "metaSuffix";
            this.metaSuffix.Size = new System.Drawing.Size(110, 21);
            this.metaSuffix.TabIndex = 13;
            this.metaSuffix.SelectedIndexChanged += new System.EventHandler(this.metaSuffix_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(561, -1);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(84, 19);
            this.label6.TabIndex = 12;
            this.label6.Text = "Suffix";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // metaPrefix
            // 
            this.metaPrefix.FormattingEnabled = true;
            this.metaPrefix.Items.AddRange(new object[] {
            "Station slogan",
            "Station promo",
            "Call Letters",
            "DJ, no clip",
            "DJ, clip",
            "Song",
            "Advertisement",
            "Artist Intro",
            "N/A"});
            this.metaPrefix.Location = new System.Drawing.Point(448, 20);
            this.metaPrefix.Name = "metaPrefix";
            this.metaPrefix.Size = new System.Drawing.Size(110, 21);
            this.metaPrefix.TabIndex = 11;
            this.metaPrefix.SelectedIndexChanged += new System.EventHandler(this.metaPrefix_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(445, -1);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(84, 19);
            this.label5.TabIndex = 10;
            this.label5.Text = "Prefix";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // metaTitle
            // 
            this.metaTitle.Location = new System.Drawing.Point(262, 21);
            this.metaTitle.Name = "metaTitle";
            this.metaTitle.Size = new System.Drawing.Size(180, 20);
            this.metaTitle.TabIndex = 7;
            this.metaTitle.TextChanged += new System.EventHandler(this.metaTitle_TextChanged);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(259, -1);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 19);
            this.label3.TabIndex = 6;
            this.label3.Text = "Track Title";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // metaArtist
            // 
            this.metaArtist.Location = new System.Drawing.Point(76, 22);
            this.metaArtist.Name = "metaArtist";
            this.metaArtist.Size = new System.Drawing.Size(180, 20);
            this.metaArtist.TabIndex = 5;
            this.metaArtist.TextChanged += new System.EventHandler(this.metaArtist_TextChanged);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(73, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 19);
            this.label2.TabIndex = 4;
            this.label2.Text = "Track Artist";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // metaCall
            // 
            this.metaCall.Location = new System.Drawing.Point(6, 22);
            this.metaCall.Name = "metaCall";
            this.metaCall.Size = new System.Drawing.Size(64, 20);
            this.metaCall.TabIndex = 3;
            this.metaCall.TextChanged += new System.EventHandler(this.metaCall_TextChanged);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(92, -1);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(84, 19);
            this.label4.TabIndex = 8;
            this.label4.Text = "Recording Date";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // metaTime
            // 
            this.metaTime.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.metaTime.Location = new System.Drawing.Point(95, 21);
            this.metaTime.Name = "metaTime";
            this.metaTime.Size = new System.Drawing.Size(106, 20);
            this.metaTime.TabIndex = 9;
            this.metaTime.ValueChanged += new System.EventHandler(this.metaTime_ValueChanged);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.alreadyExistsBadge);
            this.panel2.Controls.Add(this.label10);
            this.panel2.Controls.Add(this.metaHash);
            this.panel2.Controls.Add(this.label9);
            this.panel2.Controls.Add(this.metaSize);
            this.panel2.Controls.Add(this.metaTime);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.label8);
            this.panel2.Controls.Add(this.metaRadio);
            this.panel2.Controls.Add(this.metaSampleRate);
            this.panel2.Controls.Add(this.label14);
            this.panel2.Location = new System.Drawing.Point(12, 407);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(939, 48);
            this.panel2.TabIndex = 16;
            // 
            // alreadyExistsBadge
            // 
            this.alreadyExistsBadge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.alreadyExistsBadge.BackColor = System.Drawing.Color.Red;
            this.alreadyExistsBadge.ForeColor = System.Drawing.Color.White;
            this.alreadyExistsBadge.Location = new System.Drawing.Point(844, 1);
            this.alreadyExistsBadge.Name = "alreadyExistsBadge";
            this.alreadyExistsBadge.Size = new System.Drawing.Size(92, 18);
            this.alreadyExistsBadge.TabIndex = 16;
            this.alreadyExistsBadge.Text = "Already Exists";
            this.alreadyExistsBadge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.alreadyExistsBadge.Visible = false;
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(413, -1);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(81, 19);
            this.label10.TabIndex = 9;
            this.label10.Text = "SHA-256";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // metaHash
            // 
            this.metaHash.Location = new System.Drawing.Point(416, 21);
            this.metaHash.Name = "metaHash";
            this.metaHash.ReadOnly = true;
            this.metaHash.Size = new System.Drawing.Size(520, 20);
            this.metaHash.TabIndex = 8;
            this.metaHash.Text = "Calculating...";
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(307, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(81, 19);
            this.label9.TabIndex = 7;
            this.label9.Text = "File Size";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // metaSize
            // 
            this.metaSize.Location = new System.Drawing.Point(310, 21);
            this.metaSize.Name = "metaSize";
            this.metaSize.ReadOnly = true;
            this.metaSize.Size = new System.Drawing.Size(100, 20);
            this.metaSize.TabIndex = 6;
            this.metaSize.Text = "0";
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(201, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(81, 19);
            this.label8.TabIndex = 5;
            this.label8.Text = "Sample Rate";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // metaRadio
            // 
            this.metaRadio.FormattingEnabled = true;
            this.metaRadio.Items.AddRange(new object[] {
            "AirSpy",
            "RTL-SDR"});
            this.metaRadio.Location = new System.Drawing.Point(6, 21);
            this.metaRadio.Name = "metaRadio";
            this.metaRadio.Size = new System.Drawing.Size(83, 21);
            this.metaRadio.TabIndex = 4;
            this.metaRadio.SelectedIndexChanged += new System.EventHandler(this.metaRadio_SelectedIndexChanged);
            // 
            // metaSampleRate
            // 
            this.metaSampleRate.Location = new System.Drawing.Point(204, 21);
            this.metaSampleRate.Name = "metaSampleRate";
            this.metaSampleRate.ReadOnly = true;
            this.metaSampleRate.Size = new System.Drawing.Size(100, 20);
            this.metaSampleRate.TabIndex = 3;
            this.metaSampleRate.Text = "0";
            // 
            // label14
            // 
            this.label14.Location = new System.Drawing.Point(3, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(56, 19);
            this.label14.TabIndex = 2;
            this.label14.Text = "Radio";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // trackProgress
            // 
            this.trackProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackProgress.LargeChange = 15;
            this.trackProgress.Location = new System.Drawing.Point(1, 516);
            this.trackProgress.Maximum = 200;
            this.trackProgress.Name = "trackProgress";
            this.trackProgress.Size = new System.Drawing.Size(853, 45);
            this.trackProgress.SmallChange = 10;
            this.trackProgress.TabIndex = 17;
            this.trackProgress.TickFrequency = 10;
            this.trackProgress.Scroll += new System.EventHandler(this.trackProgress_Scroll);
            this.trackProgress.MouseDown += new System.Windows.Forms.MouseEventHandler(this.trackProgress_MouseDown);
            this.trackProgress.MouseUp += new System.Windows.Forms.MouseEventHandler(this.trackProgress_MouseUp);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Enabled = false;
            this.btnSave.Location = new System.Drawing.Point(852, 517);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(99, 23);
            this.btnSave.TabIndex = 18;
            this.btnSave.Text = "Save Full";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // timerText
            // 
            this.timerText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.timerText.Location = new System.Drawing.Point(852, 449);
            this.timerText.Name = "timerText";
            this.timerText.Size = new System.Drawing.Size(99, 23);
            this.timerText.TabIndex = 19;
            this.timerText.Text = "00:00 / 00:00";
            this.timerText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // waveformView
            // 
            this.waveformView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.waveformView.Location = new System.Drawing.Point(12, 455);
            this.waveformView.Name = "waveformView";
            this.waveformView.Size = new System.Drawing.Size(834, 61);
            this.waveformView.TabIndex = 20;
            this.waveformView.TabStop = false;
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelete.Enabled = false;
            this.btnDelete.Location = new System.Drawing.Point(852, 471);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(99, 23);
            this.btnDelete.TabIndex = 21;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // rdsRadioText
            // 
            this.rdsRadioText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rdsRadioText.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdsRadioText.Location = new System.Drawing.Point(356, 12);
            this.rdsRadioText.Name = "rdsRadioText";
            this.rdsRadioText.ReadOnly = true;
            this.rdsRadioText.Size = new System.Drawing.Size(591, 20);
            this.rdsRadioText.TabIndex = 10;
            this.rdsRadioText.Text = "QWERTYUI";
            // 
            // rdsPsName
            // 
            this.rdsPsName.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdsPsName.Location = new System.Drawing.Point(281, 12);
            this.rdsPsName.MaxLength = 8;
            this.rdsPsName.Name = "rdsPsName";
            this.rdsPsName.ReadOnly = true;
            this.rdsPsName.Size = new System.Drawing.Size(69, 20);
            this.rdsPsName.TabIndex = 22;
            this.rdsPsName.Text = "QWERTYUI";
            this.rdsPsName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // sliderPlaybackVol
            // 
            this.sliderPlaybackVol.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.sliderPlaybackVol.LargeChange = 10;
            this.sliderPlaybackVol.Location = new System.Drawing.Point(914, 52);
            this.sliderPlaybackVol.Maximum = 100;
            this.sliderPlaybackVol.Name = "sliderPlaybackVol";
            this.sliderPlaybackVol.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.sliderPlaybackVol.Size = new System.Drawing.Size(45, 88);
            this.sliderPlaybackVol.TabIndex = 24;
            this.sliderPlaybackVol.TickFrequency = 10;
            this.sliderPlaybackVol.Value = 100;
            this.sliderPlaybackVol.Scroll += new System.EventHandler(this.sliderPlaybackVol_Scroll);
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label11.Location = new System.Drawing.Point(906, 38);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(45, 14);
            this.label11.TabIndex = 25;
            this.label11.Text = "Volume";
            this.label11.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // label12
            // 
            this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label12.Location = new System.Drawing.Point(903, 143);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(45, 14);
            this.label12.TabIndex = 27;
            this.label12.Text = "IF Gain";
            this.label12.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // sliderIfGain
            // 
            this.sliderIfGain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.sliderIfGain.LargeChange = 10;
            this.sliderIfGain.Location = new System.Drawing.Point(911, 157);
            this.sliderIfGain.Maximum = 200;
            this.sliderIfGain.Name = "sliderIfGain";
            this.sliderIfGain.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.sliderIfGain.Size = new System.Drawing.Size(45, 88);
            this.sliderIfGain.TabIndex = 26;
            this.sliderIfGain.TickFrequency = 10;
            this.sliderIfGain.Value = 100;
            this.sliderIfGain.Scroll += new System.EventHandler(this.sliderIfGain_Scroll);
            // 
            // label13
            // 
            this.label13.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label13.Location = new System.Drawing.Point(906, 248);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(45, 14);
            this.label13.TabIndex = 29;
            this.label13.Text = "Range";
            this.label13.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            // 
            // sliderFftRange
            // 
            this.sliderFftRange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.sliderFftRange.LargeChange = 10;
            this.sliderFftRange.Location = new System.Drawing.Point(914, 262);
            this.sliderFftRange.Maximum = 100;
            this.sliderFftRange.Name = "sliderFftRange";
            this.sliderFftRange.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.sliderFftRange.Size = new System.Drawing.Size(45, 88);
            this.sliderFftRange.TabIndex = 28;
            this.sliderFftRange.TickFrequency = 10;
            this.sliderFftRange.Value = 100;
            // 
            // mpxSpectrumView
            // 
            this.mpxSpectrumView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mpxSpectrumView.Location = new System.Drawing.Point(281, 264);
            this.mpxSpectrumView.Name = "mpxSpectrumView";
            this.mpxSpectrumView.Size = new System.Drawing.Size(616, 87);
            this.mpxSpectrumView.TabIndex = 23;
            // 
            // fftSpectrumView1
            // 
            this.fftSpectrumView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fftSpectrumView1.Location = new System.Drawing.Point(281, 38);
            this.fftSpectrumView1.Name = "fftSpectrumView1";
            this.fftSpectrumView1.Size = new System.Drawing.Size(616, 222);
            this.fftSpectrumView1.TabIndex = 0;
            // 
            // btnTrim
            // 
            this.btnTrim.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTrim.Enabled = false;
            this.btnTrim.Location = new System.Drawing.Point(852, 494);
            this.btnTrim.Name = "btnTrim";
            this.btnTrim.Size = new System.Drawing.Size(99, 23);
            this.btnTrim.TabIndex = 30;
            this.btnTrim.Text = "Trim...";
            this.btnTrim.UseVisualStyleBackColor = true;
            this.btnTrim.Click += new System.EventHandler(this.btnTrim_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(959, 547);
            this.Controls.Add(this.btnTrim);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.sliderFftRange);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.sliderIfGain);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.sliderPlaybackVol);
            this.Controls.Add(this.mpxSpectrumView);
            this.Controls.Add(this.rdsPsName);
            this.Controls.Add(this.rdsRadioText);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.waveformView);
            this.Controls.Add(this.timerText);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.trackProgress);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.fileList);
            this.Controls.Add(this.fftSpectrumView1);
            this.MaximumSize = new System.Drawing.Size(975, 2000);
            this.MinimumSize = new System.Drawing.Size(975, 39);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackProgress)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.waveformView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sliderPlaybackVol)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sliderIfGain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sliderFftRange)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListView fileList;
        private System.Windows.Forms.ColumnHeader filename;
        private System.Windows.Forms.ColumnHeader filesize;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox metaNotes;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox metaSuffix;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox metaPrefix;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DateTimePicker metaTime;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label timerText;
        public System.Windows.Forms.TextBox metaSampleRate;
        public System.Windows.Forms.TextBox metaHash;
        public System.Windows.Forms.TextBox metaSize;
        public System.Windows.Forms.PictureBox waveformView;
        public System.Windows.Forms.ComboBox metaRadio;
        public System.Windows.Forms.TrackBar trackProgress;
        public System.Windows.Forms.Button btnDelete;
        public System.Windows.Forms.Button btnSave;
        public System.Windows.Forms.TextBox rdsRadioText;
        public System.Windows.Forms.Label alreadyExistsBadge;
        public System.Windows.Forms.TextBox rdsPsName;
        public System.Windows.Forms.TextBox metaTitle;
        public System.Windows.Forms.TextBox metaArtist;
        public System.Windows.Forms.TextBox metaCall;
        public LibSDR.UI.FFTSpectrumView fftSpectrumView1;
        public LibSDR.UI.FFTSpectrumView mpxSpectrumView;
        private System.Windows.Forms.TrackBar sliderPlaybackVol;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TrackBar sliderIfGain;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TrackBar sliderFftRange;
        public System.Windows.Forms.Button btnTrim;
    }
}

