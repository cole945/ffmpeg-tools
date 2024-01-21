namespace gepmff
{
    partial class GepmffForm
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
            this.components = new System.ComponentModel.Container();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabelConsole = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabelFilename = new System.Windows.Forms.ToolStripStatusLabel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.listViewFiles = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFolder = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderBitrate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderDuration = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFPS = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderVideo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderAudio = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderNewSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderResolution = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderNewBitrate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderParameters = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openFileLocationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.applyNewParametersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetStatusToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.textBoxAudioBitrate = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.comboBoxAudio = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.comboBoxYADIF = new System.Windows.Forms.ComboBox();
            this.maskedTextBoxFPS = new System.Windows.Forms.MaskedTextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.comboBoxVsync = new System.Windows.Forms.ComboBox();
            this.multipasComboBox = new System.Windows.Forms.ComboBox();
            this.seekerToTextBox = new System.Windows.Forms.MaskedTextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.maskedBF = new System.Windows.Forms.MaskedTextBox();
            this.comboBRefMode = new System.Windows.Forms.ComboBox();
            this.checkBoxSpatialAQ = new System.Windows.Forms.CheckBox();
            this.checkBoxTemporalAQ = new System.Windows.Forms.CheckBox();
            this.checkHwDec = new System.Windows.Forms.CheckBox();
            this.checkBoxSkipIfNotSmaller = new System.Windows.Forms.CheckBox();
            this.seekerTextBox = new System.Windows.Forms.MaskedTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxReplaceOldFile = new System.Windows.Forms.CheckBox();
            this.numericUpDownBitrate = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownCRF = new System.Windows.Forms.NumericUpDown();
            this.radioButtonBitrate = new System.Windows.Forms.RadioButton();
            this.radioButtonCRF = new System.Windows.Forms.RadioButton();
            this.labelBF = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBoxScale = new System.Windows.Forms.ComboBox();
            this.checkBoxDeleteWhenDone = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxCodec = new System.Windows.Forms.ComboBox();
            this.textBoxSuffix = new System.Windows.Forms.TextBox();
            this.buttonStart = new System.Windows.Forms.Button();
            this.textBoxOutdir = new System.Windows.Forms.TextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.statusStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBitrate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCRF)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(40, 40);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.toolStripStatusLabelConsole,
            this.toolStripStatusLabelFilename});
            this.statusStrip1.Location = new System.Drawing.Point(0, 528);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1241, 32);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(200, 24);
            // 
            // toolStripStatusLabelConsole
            // 
            this.toolStripStatusLabelConsole.Name = "toolStripStatusLabelConsole";
            this.toolStripStatusLabelConsole.Size = new System.Drawing.Size(102, 25);
            this.toolStripStatusLabelConsole.Text = "                  ";
            // 
            // toolStripStatusLabelFilename
            // 
            this.toolStripStatusLabelFilename.Name = "toolStripStatusLabelFilename";
            this.toolStripStatusLabelFilename.Size = new System.Drawing.Size(47, 25);
            this.toolStripStatusLabelFilename.Text = "       ";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tabControl1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1241, 528);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(3, 189);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1235, 336);
            this.tabControl1.TabIndex = 11;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.listViewFiles);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1227, 310);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Files";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // listViewFiles
            // 
            this.listViewFiles.AllowDrop = true;
            this.listViewFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderFolder,
            this.columnHeaderBitrate,
            this.columnHeaderDuration,
            this.columnHeaderFPS,
            this.columnHeaderVideo,
            this.columnHeaderAudio,
            this.columnHeaderSize,
            this.columnHeaderNewSize,
            this.columnHeaderResolution,
            this.columnHeaderNewBitrate,
            this.columnHeaderParameters,
            this.columnHeaderStatus});
            this.listViewFiles.ContextMenuStrip = this.contextMenuStrip1;
            this.listViewFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewFiles.HideSelection = false;
            this.listViewFiles.Location = new System.Drawing.Point(3, 3);
            this.listViewFiles.Name = "listViewFiles";
            this.listViewFiles.Size = new System.Drawing.Size(1221, 304);
            this.listViewFiles.TabIndex = 0;
            this.listViewFiles.UseCompatibleStateImageBehavior = false;
            this.listViewFiles.View = System.Windows.Forms.View.Details;
            this.listViewFiles.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listViewFiles_ColumnClick);
            this.listViewFiles.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewFiles_DragDrop);
            this.listViewFiles.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewFiles_DragEnter);
            this.listViewFiles.DoubleClick += new System.EventHandler(this.listViewFiles_DoubleClick);
            this.listViewFiles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewFiles_KeyDown);
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Filename";
            this.columnHeaderName.Width = 138;
            // 
            // columnHeaderFolder
            // 
            this.columnHeaderFolder.Text = "Folder";
            this.columnHeaderFolder.Width = 256;
            // 
            // columnHeaderBitrate
            // 
            this.columnHeaderBitrate.Text = "Bitrate";
            // 
            // columnHeaderDuration
            // 
            this.columnHeaderDuration.Text = "Duration";
            // 
            // columnHeaderFPS
            // 
            this.columnHeaderFPS.Text = "FPS";
            // 
            // columnHeaderVideo
            // 
            this.columnHeaderVideo.Text = "Video";
            // 
            // columnHeaderAudio
            // 
            this.columnHeaderAudio.Text = "Audio";
            // 
            // columnHeaderSize
            // 
            this.columnHeaderSize.Text = "Size";
            this.columnHeaderSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // columnHeaderNewSize
            // 
            this.columnHeaderNewSize.Text = "New Size";
            this.columnHeaderNewSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // columnHeaderResolution
            // 
            this.columnHeaderResolution.Text = "Resolution";
            // 
            // columnHeaderNewBitrate
            // 
            this.columnHeaderNewBitrate.Text = "New bitrate";
            this.columnHeaderNewBitrate.Width = 72;
            // 
            // columnHeaderParameters
            // 
            this.columnHeaderParameters.Text = "Parameters";
            this.columnHeaderParameters.Width = 106;
            // 
            // columnHeaderStatus
            // 
            this.columnHeaderStatus.Text = "Status";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(36, 36);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openFileLocationToolStripMenuItem,
            this.applyNewParametersToolStripMenuItem,
            this.resetStatusToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(263, 100);
            // 
            // openFileLocationToolStripMenuItem
            // 
            this.openFileLocationToolStripMenuItem.Name = "openFileLocationToolStripMenuItem";
            this.openFileLocationToolStripMenuItem.Size = new System.Drawing.Size(262, 32);
            this.openFileLocationToolStripMenuItem.Text = "Open file location";
            this.openFileLocationToolStripMenuItem.Click += new System.EventHandler(this.OpenFileLocationToolStripMenuItem_Click);
            // 
            // applyNewParametersToolStripMenuItem
            // 
            this.applyNewParametersToolStripMenuItem.Name = "applyNewParametersToolStripMenuItem";
            this.applyNewParametersToolStripMenuItem.Size = new System.Drawing.Size(262, 32);
            this.applyNewParametersToolStripMenuItem.Text = "Apply new parameters";
            this.applyNewParametersToolStripMenuItem.Click += new System.EventHandler(this.ApplyNewParametersToolStripMenuItem_Click);
            // 
            // resetStatusToolStripMenuItem
            // 
            this.resetStatusToolStripMenuItem.Name = "resetStatusToolStripMenuItem";
            this.resetStatusToolStripMenuItem.Size = new System.Drawing.Size(262, 32);
            this.resetStatusToolStripMenuItem.Text = "Reset status";
            this.resetStatusToolStripMenuItem.Click += new System.EventHandler(this.resetStatusToolStripMenuItem_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.textBoxLog);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1227, 310);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Commands";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // textBoxLog
            // 
            this.textBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxLog.Location = new System.Drawing.Point(3, 3);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.Size = new System.Drawing.Size(1221, 304);
            this.textBoxLog.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.textBoxAudioBitrate);
            this.panel1.Controls.Add(this.label11);
            this.panel1.Controls.Add(this.comboBoxAudio);
            this.panel1.Controls.Add(this.label10);
            this.panel1.Controls.Add(this.comboBoxYADIF);
            this.panel1.Controls.Add(this.maskedTextBoxFPS);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.comboBoxVsync);
            this.panel1.Controls.Add(this.multipasComboBox);
            this.panel1.Controls.Add(this.seekerToTextBox);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.maskedBF);
            this.panel1.Controls.Add(this.comboBRefMode);
            this.panel1.Controls.Add(this.checkBoxSpatialAQ);
            this.panel1.Controls.Add(this.checkBoxTemporalAQ);
            this.panel1.Controls.Add(this.checkHwDec);
            this.panel1.Controls.Add(this.checkBoxSkipIfNotSmaller);
            this.panel1.Controls.Add(this.seekerTextBox);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.checkBoxReplaceOldFile);
            this.panel1.Controls.Add(this.numericUpDownBitrate);
            this.panel1.Controls.Add(this.numericUpDownCRF);
            this.panel1.Controls.Add(this.radioButtonBitrate);
            this.panel1.Controls.Add(this.radioButtonCRF);
            this.panel1.Controls.Add(this.labelBF);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.comboBoxScale);
            this.panel1.Controls.Add(this.checkBoxDeleteWhenDone);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.comboBoxCodec);
            this.panel1.Controls.Add(this.textBoxSuffix);
            this.panel1.Controls.Add(this.buttonStart);
            this.panel1.Controls.Add(this.textBoxOutdir);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1235, 180);
            this.panel1.TabIndex = 4;
            // 
            // textBoxAudioBitrate
            // 
            this.textBoxAudioBitrate.Location = new System.Drawing.Point(1079, 67);
            this.textBoxAudioBitrate.Name = "textBoxAudioBitrate";
            this.textBoxAudioBitrate.Size = new System.Drawing.Size(85, 20);
            this.textBoxAudioBitrate.TabIndex = 48;
            this.textBoxAudioBitrate.Text = "640k";
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(882, 70);
            this.label11.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(66, 13);
            this.label11.TabIndex = 47;
            this.label11.Text = "Audio:";
            this.label11.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // comboBoxAudio
            // 
            this.comboBoxAudio.FormattingEnabled = true;
            this.comboBoxAudio.Items.AddRange(new object[] {
            "copy",
            "libfdk_aac",
            "aac",
            "ac3"});
            this.comboBoxAudio.Location = new System.Drawing.Point(952, 67);
            this.comboBoxAudio.Name = "comboBoxAudio";
            this.comboBoxAudio.Size = new System.Drawing.Size(121, 21);
            this.comboBoxAudio.TabIndex = 46;
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(882, 43);
            this.label10.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(66, 13);
            this.label10.TabIndex = 45;
            this.label10.Text = "YADIF:";
            this.label10.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // comboBoxYADIF
            // 
            this.comboBoxYADIF.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxYADIF.FormattingEnabled = true;
            this.comboBoxYADIF.Items.AddRange(new object[] {
            "(no YADIF)",
            "YADIF",
            "YADIF_CUDA"});
            this.comboBoxYADIF.Location = new System.Drawing.Point(952, 40);
            this.comboBoxYADIF.Name = "comboBoxYADIF";
            this.comboBoxYADIF.Size = new System.Drawing.Size(121, 21);
            this.comboBoxYADIF.TabIndex = 44;
            // 
            // maskedTextBoxFPS
            // 
            this.maskedTextBoxFPS.Location = new System.Drawing.Point(952, 140);
            this.maskedTextBoxFPS.Mask = "00.000";
            this.maskedTextBoxFPS.Name = "maskedTextBoxFPS";
            this.maskedTextBoxFPS.Size = new System.Drawing.Size(121, 20);
            this.maskedTextBoxFPS.TabIndex = 41;
            this.maskedTextBoxFPS.ValidatingType = typeof(int);
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(882, 140);
            this.label9.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(66, 13);
            this.label9.TabIndex = 40;
            this.label9.Text = "fps:";
            this.label9.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(676, 140);
            this.label8.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(66, 13);
            this.label8.TabIndex = 39;
            this.label8.Text = "vsync:";
            this.label8.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(676, 113);
            this.label7.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(66, 13);
            this.label7.TabIndex = 38;
            this.label7.Text = "multipass:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // comboBoxVsync
            // 
            this.comboBoxVsync.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxVsync.FormattingEnabled = true;
            this.comboBoxVsync.Items.AddRange(new object[] {
            "0, passthrough",
            "1, cfr",
            "2, vfr",
            "drop",
            "-1, auto (1 or 2)"});
            this.comboBoxVsync.Location = new System.Drawing.Point(746, 137);
            this.comboBoxVsync.Name = "comboBoxVsync";
            this.comboBoxVsync.Size = new System.Drawing.Size(121, 21);
            this.comboBoxVsync.TabIndex = 37;
            // 
            // multipasComboBox
            // 
            this.multipasComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.multipasComboBox.FormattingEnabled = true;
            this.multipasComboBox.Items.AddRange(new object[] {
            "0 disable",
            "1 qres",
            "2 fullres"});
            this.multipasComboBox.Location = new System.Drawing.Point(746, 110);
            this.multipasComboBox.Name = "multipasComboBox";
            this.multipasComboBox.Size = new System.Drawing.Size(121, 21);
            this.multipasComboBox.TabIndex = 37;
            // 
            // seekerToTextBox
            // 
            this.seekerToTextBox.InsertKeyMode = System.Windows.Forms.InsertKeyMode.Overwrite;
            this.seekerToTextBox.Location = new System.Drawing.Point(448, 129);
            this.seekerToTextBox.Mask = "00:00:00.000";
            this.seekerToTextBox.Name = "seekerToTextBox";
            this.seekerToTextBox.Size = new System.Drawing.Size(120, 20);
            this.seekerToTextBox.TabIndex = 36;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(407, 129);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(23, 13);
            this.label6.TabIndex = 35;
            this.label6.Text = "To:";
            // 
            // maskedBF
            // 
            this.maskedBF.Location = new System.Drawing.Point(746, 84);
            this.maskedBF.Mask = "000";
            this.maskedBF.Name = "maskedBF";
            this.maskedBF.Size = new System.Drawing.Size(121, 20);
            this.maskedBF.TabIndex = 34;
            this.maskedBF.Text = "3";
            this.maskedBF.ValidatingType = typeof(int);
            // 
            // comboBRefMode
            // 
            this.comboBRefMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBRefMode.FormattingEnabled = true;
            this.comboBRefMode.Items.AddRange(new object[] {
            "0 disable",
            "1 each",
            "2 middle"});
            this.comboBRefMode.Location = new System.Drawing.Point(746, 53);
            this.comboBRefMode.Name = "comboBRefMode";
            this.comboBRefMode.Size = new System.Drawing.Size(121, 21);
            this.comboBRefMode.TabIndex = 33;
            // 
            // checkBoxSpatialAQ
            // 
            this.checkBoxSpatialAQ.AutoSize = true;
            this.checkBoxSpatialAQ.Location = new System.Drawing.Point(1059, 103);
            this.checkBoxSpatialAQ.Name = "checkBoxSpatialAQ";
            this.checkBoxSpatialAQ.Size = new System.Drawing.Size(83, 21);
            this.checkBoxSpatialAQ.TabIndex = 32;
            this.checkBoxSpatialAQ.Text = "Spatial AQ";
            this.checkBoxSpatialAQ.UseVisualStyleBackColor = true;
            // 
            // checkBoxTemporalAQ
            // 
            this.checkBoxTemporalAQ.AutoSize = true;
            this.checkBoxTemporalAQ.Location = new System.Drawing.Point(952, 103);
            this.checkBoxTemporalAQ.Name = "checkBoxTemporalAQ";
            this.checkBoxTemporalAQ.Size = new System.Drawing.Size(95, 21);
            this.checkBoxTemporalAQ.TabIndex = 32;
            this.checkBoxTemporalAQ.Text = "Temporal AQ";
            this.checkBoxTemporalAQ.UseVisualStyleBackColor = true;
            // 
            // checkHwDec
            // 
            this.checkHwDec.AutoSize = true;
            this.checkHwDec.Location = new System.Drawing.Point(679, 26);
            this.checkHwDec.Name = "checkHwDec";
            this.checkHwDec.Size = new System.Drawing.Size(101, 21);
            this.checkHwDec.TabIndex = 32;
            this.checkHwDec.Text = "HW Decoding";
            this.checkHwDec.UseVisualStyleBackColor = true;
            // 
            // checkBoxSkipIfNotSmaller
            // 
            this.checkBoxSkipIfNotSmaller.AutoSize = true;
            this.checkBoxSkipIfNotSmaller.Checked = true;
            this.checkBoxSkipIfNotSmaller.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSkipIfNotSmaller.Location = new System.Drawing.Point(50, 131);
            this.checkBoxSkipIfNotSmaller.Name = "checkBoxSkipIfNotSmaller";
            this.checkBoxSkipIfNotSmaller.Size = new System.Drawing.Size(115, 21);
            this.checkBoxSkipIfNotSmaller.TabIndex = 31;
            this.checkBoxSkipIfNotSmaller.Text = "Skip if not smaller";
            this.checkBoxSkipIfNotSmaller.UseVisualStyleBackColor = true;
            // 
            // seekerTextBox
            // 
            this.seekerTextBox.InsertKeyMode = System.Windows.Forms.InsertKeyMode.Overwrite;
            this.seekerTextBox.Location = new System.Drawing.Point(449, 107);
            this.seekerTextBox.Mask = "00:00:00.000";
            this.seekerTextBox.Name = "seekerTextBox";
            this.seekerTextBox.Size = new System.Drawing.Size(120, 20);
            this.seekerTextBox.TabIndex = 30;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(408, 107);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 29;
            this.label1.Text = "Seek:";
            // 
            // checkBoxReplaceOldFile
            // 
            this.checkBoxReplaceOldFile.AutoSize = true;
            this.checkBoxReplaceOldFile.Location = new System.Drawing.Point(50, 107);
            this.checkBoxReplaceOldFile.Name = "checkBoxReplaceOldFile";
            this.checkBoxReplaceOldFile.Size = new System.Drawing.Size(106, 21);
            this.checkBoxReplaceOldFile.TabIndex = 27;
            this.checkBoxReplaceOldFile.Text = "Replace old file";
            this.checkBoxReplaceOldFile.UseVisualStyleBackColor = true;
            // 
            // numericUpDownBitrate
            // 
            this.numericUpDownBitrate.Increment = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownBitrate.Location = new System.Drawing.Point(449, 53);
            this.numericUpDownBitrate.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.numericUpDownBitrate.Name = "numericUpDownBitrate";
            this.numericUpDownBitrate.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownBitrate.TabIndex = 26;
            this.numericUpDownBitrate.Value = new decimal(new int[] {
            1800,
            0,
            0,
            0});
            this.numericUpDownBitrate.ValueChanged += new System.EventHandler(this.numericUpDownBitrate_ValueChanged);
            // 
            // numericUpDownCRF
            // 
            this.numericUpDownCRF.Location = new System.Drawing.Point(449, 27);
            this.numericUpDownCRF.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDownCRF.Name = "numericUpDownCRF";
            this.numericUpDownCRF.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownCRF.TabIndex = 25;
            this.numericUpDownCRF.Value = new decimal(new int[] {
            23,
            0,
            0,
            0});
            this.numericUpDownCRF.ValueChanged += new System.EventHandler(this.numericUpDownCRF_ValueChanged);
            // 
            // radioButtonBitrate
            // 
            this.radioButtonBitrate.AutoSize = true;
            this.radioButtonBitrate.Location = new System.Drawing.Point(363, 53);
            this.radioButtonBitrate.Name = "radioButtonBitrate";
            this.radioButtonBitrate.Size = new System.Drawing.Size(87, 20);
            this.radioButtonBitrate.TabIndex = 24;
            this.radioButtonBitrate.Text = "Avg Bitrate:";
            this.radioButtonBitrate.UseVisualStyleBackColor = true;
            // 
            // radioButtonCRF
            // 
            this.radioButtonCRF.AutoSize = true;
            this.radioButtonCRF.Checked = true;
            this.radioButtonCRF.Location = new System.Drawing.Point(394, 27);
            this.radioButtonCRF.Name = "radioButtonCRF";
            this.radioButtonCRF.Size = new System.Drawing.Size(56, 20);
            this.radioButtonCRF.TabIndex = 23;
            this.radioButtonCRF.TabStop = true;
            this.radioButtonCRF.Text = "CRF:";
            this.radioButtonCRF.UseVisualStyleBackColor = true;
            // 
            // labelBF
            // 
            this.labelBF.Location = new System.Drawing.Point(676, 84);
            this.labelBF.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.labelBF.Name = "labelBF";
            this.labelBF.Size = new System.Drawing.Size(66, 13);
            this.labelBF.TabIndex = 22;
            this.labelBF.Text = "bf:";
            this.labelBF.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(676, 56);
            this.label4.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(66, 13);
            this.label4.TabIndex = 22;
            this.label4.Text = "b_ref_mode:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(406, 83);
            this.label5.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(37, 13);
            this.label5.TabIndex = 22;
            this.label5.Text = "Scale:";
            // 
            // comboBoxScale
            // 
            this.comboBoxScale.FormattingEnabled = true;
            this.comboBoxScale.Items.AddRange(new object[] {
            "(none)",
            "720",
            "1080",
            "2160",
            "576",
            "480"});
            this.comboBoxScale.Location = new System.Drawing.Point(448, 80);
            this.comboBoxScale.Name = "comboBoxScale";
            this.comboBoxScale.Size = new System.Drawing.Size(121, 21);
            this.comboBoxScale.TabIndex = 20;
            this.comboBoxScale.Text = "(none)";
            // 
            // checkBoxDeleteWhenDone
            // 
            this.checkBoxDeleteWhenDone.AutoSize = true;
            this.checkBoxDeleteWhenDone.Location = new System.Drawing.Point(50, 84);
            this.checkBoxDeleteWhenDone.Name = "checkBoxDeleteWhenDone";
            this.checkBoxDeleteWhenDone.Size = new System.Drawing.Size(139, 21);
            this.checkBoxDeleteWhenDone.TabIndex = 17;
            this.checkBoxDeleteWhenDone.Text = "Delete file when done.";
            this.checkBoxDeleteWhenDone.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 49);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Output";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "Suffix";
            // 
            // comboBoxCodec
            // 
            this.comboBoxCodec.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCodec.FormattingEnabled = true;
            this.comboBoxCodec.Items.AddRange(new object[] {
            "libx265",
            "hevc_nvenc",
            "hevc_qsv",
            "libsvtav1",
            "libx264",
            "h264_nvenc",
            "h264_qsv",
            "vmaf",
            "copy"});
            this.comboBoxCodec.Location = new System.Drawing.Point(156, 17);
            this.comboBoxCodec.Name = "comboBoxCodec";
            this.comboBoxCodec.Size = new System.Drawing.Size(157, 21);
            this.comboBoxCodec.TabIndex = 14;
            this.comboBoxCodec.SelectedIndexChanged += new System.EventHandler(this.comboBoxCodec_SelectedIndexChanged);
            // 
            // textBoxSuffix
            // 
            this.textBoxSuffix.Location = new System.Drawing.Point(50, 15);
            this.textBoxSuffix.Name = "textBoxSuffix";
            this.textBoxSuffix.Size = new System.Drawing.Size(99, 20);
            this.textBoxSuffix.TabIndex = 13;
            this.textBoxSuffix.Text = "x265";
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(244, 76);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(69, 29);
            this.buttonStart.TabIndex = 12;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // textBoxOutdir
            // 
            this.textBoxOutdir.Location = new System.Drawing.Point(50, 45);
            this.textBoxOutdir.Name = "textBoxOutdir";
            this.textBoxOutdir.Size = new System.Drawing.Size(263, 20);
            this.textBoxOutdir.TabIndex = 10;
            // 
            // GepmffForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1241, 560);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.statusStrip1);
            this.DoubleBuffered = true;
            this.Name = "GepmffForm";
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownBitrate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCRF)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelConsole;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.ListView listViewFiles;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderFolder;
        private System.Windows.Forms.ColumnHeader columnHeaderBitrate;
        private System.Windows.Forms.ColumnHeader columnHeaderDuration;
        private System.Windows.Forms.ColumnHeader columnHeaderVideo;
        private System.Windows.Forms.ColumnHeader columnHeaderAudio;
        private System.Windows.Forms.ColumnHeader columnHeaderSize;
        private System.Windows.Forms.ColumnHeader columnHeaderNewSize;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox checkBoxDeleteWhenDone;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxCodec;
        private System.Windows.Forms.TextBox textBoxSuffix;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.TextBox textBoxOutdir;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ColumnHeader columnHeaderResolution;
        private System.Windows.Forms.ColumnHeader columnHeaderNewBitrate;
        private System.Windows.Forms.ColumnHeader columnHeaderParameters;
        private System.Windows.Forms.ComboBox comboBoxScale;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ColumnHeader columnHeaderStatus;
        private System.Windows.Forms.RadioButton radioButtonBitrate;
        private System.Windows.Forms.RadioButton radioButtonCRF;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem openFileLocationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem applyNewParametersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetStatusToolStripMenuItem;
        private System.Windows.Forms.NumericUpDown numericUpDownBitrate;
        private System.Windows.Forms.NumericUpDown numericUpDownCRF;
        private System.Windows.Forms.CheckBox checkBoxReplaceOldFile;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.MaskedTextBox seekerTextBox;
        private System.Windows.Forms.CheckBox checkBoxSkipIfNotSmaller;
        private System.Windows.Forms.CheckBox checkHwDec;
        private System.Windows.Forms.ComboBox comboBRefMode;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.MaskedTextBox maskedBF;
        private System.Windows.Forms.Label labelBF;
        private System.Windows.Forms.CheckBox checkBoxSpatialAQ;
        private System.Windows.Forms.CheckBox checkBoxTemporalAQ;
        private System.Windows.Forms.MaskedTextBox seekerToTextBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox multipasComboBox;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelFilename;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox comboBoxVsync;
        private System.Windows.Forms.ColumnHeader columnHeaderFPS;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.MaskedTextBox maskedTextBoxFPS;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox comboBoxYADIF;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox comboBoxAudio;
        private System.Windows.Forms.TextBox textBoxAudioBitrate;
    }
}

