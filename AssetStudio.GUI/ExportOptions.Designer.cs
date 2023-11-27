using System;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace AssetStudio.GUI
{
    partial class ExportOptions
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
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle6 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle7 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle8 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            OKbutton = new Button();
            Cancel = new Button();
            groupBox1 = new GroupBox();
            label6 = new Label();
            uvsGridView = new DataGridView();
            UVName = new DataGridViewTextBoxColumn();
            UVEnabled = new DataGridViewCheckBoxColumn();
            UVType = new DataGridViewComboBoxColumn();
            minimalAssetMap = new CheckBox();
            assetGroupOptions = new ComboBox();
            label7 = new Label();
            openAfterExport = new CheckBox();
            restoreExtensionName = new CheckBox();
            key = new NumericUpDown();
            encrypted = new CheckBox();
            convertAudio = new CheckBox();
            panel1 = new Panel();
            totga = new RadioButton();
            tojpg = new RadioButton();
            topng = new RadioButton();
            tobmp = new RadioButton();
            converttexture = new CheckBox();
            collectAnimations = new CheckBox();
            groupBox2 = new GroupBox();
            label9 = new Label();
            texsGridView = new DataGridView();
            TexName = new DataGridViewTextBoxColumn();
            TexType = new DataGridViewComboBoxColumn();
            exportBlendShape = new CheckBox();
            exportAnimations = new CheckBox();
            scaleFactor = new NumericUpDown();
            label5 = new Label();
            fbxFormat = new ComboBox();
            label4 = new Label();
            fbxVersion = new ComboBox();
            label3 = new Label();
            boneSize = new NumericUpDown();
            label2 = new Label();
            exportSkins = new CheckBox();
            label1 = new Label();
            filterPrecision = new NumericUpDown();
            castToBone = new CheckBox();
            exportAllNodes = new CheckBox();
            eulerFilter = new CheckBox();
            exportUvsTooltip = new ToolTip(components);
            keyToolTip = new ToolTip(components);
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)uvsGridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)key).BeginInit();
            panel1.SuspendLayout();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)texsGridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)scaleFactor).BeginInit();
            ((System.ComponentModel.ISupportInitialize)boneSize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)filterPrecision).BeginInit();
            SuspendLayout();
            // 
            // OKbutton
            // 
            OKbutton.Location = new System.Drawing.Point(480, 439);
            OKbutton.Margin = new Padding(4);
            OKbutton.Name = "OKbutton";
            OKbutton.Size = new System.Drawing.Size(88, 26);
            OKbutton.TabIndex = 6;
            OKbutton.Text = "OK";
            OKbutton.UseVisualStyleBackColor = true;
            OKbutton.Click += OKbutton_Click;
            // 
            // Cancel
            // 
            Cancel.DialogResult = DialogResult.Cancel;
            Cancel.Location = new System.Drawing.Point(576, 439);
            Cancel.Margin = new Padding(4);
            Cancel.Name = "Cancel";
            Cancel.Size = new System.Drawing.Size(88, 26);
            Cancel.TabIndex = 7;
            Cancel.Text = "Cancel";
            Cancel.UseVisualStyleBackColor = true;
            Cancel.Click += Cancel_Click;
            // 
            // groupBox1
            // 
            groupBox1.AutoSize = true;
            groupBox1.Controls.Add(label6);
            groupBox1.Controls.Add(uvsGridView);
            groupBox1.Controls.Add(minimalAssetMap);
            groupBox1.Controls.Add(assetGroupOptions);
            groupBox1.Controls.Add(label7);
            groupBox1.Controls.Add(openAfterExport);
            groupBox1.Controls.Add(restoreExtensionName);
            groupBox1.Controls.Add(key);
            groupBox1.Controls.Add(encrypted);
            groupBox1.Controls.Add(convertAudio);
            groupBox1.Controls.Add(panel1);
            groupBox1.Controls.Add(converttexture);
            groupBox1.Location = new System.Drawing.Point(14, 15);
            groupBox1.Margin = new Padding(4);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4);
            groupBox1.Size = new System.Drawing.Size(271, 419);
            groupBox1.TabIndex = 9;
            groupBox1.TabStop = false;
            groupBox1.Text = "Export";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(101, 275);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(73, 15);
            label6.TabIndex = 27;
            label6.Text = "UV Mapping";
            // 
            // uvsGridView
            // 
            uvsGridView.AllowUserToAddRows = false;
            uvsGridView.AllowUserToDeleteRows = false;
            uvsGridView.AllowUserToResizeRows = false;
            uvsGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            uvsGridView.Columns.AddRange(new DataGridViewColumn[] { UVName, UVEnabled, UVType });
            uvsGridView.Location = new System.Drawing.Point(8, 293);
            uvsGridView.Name = "uvsGridView";
            uvsGridView.RowHeadersVisible = false;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleCenter;
            uvsGridView.RowsDefaultCellStyle = dataGridViewCellStyle1;
            uvsGridView.RowTemplate.Height = 25;
            uvsGridView.Size = new System.Drawing.Size(255, 103);
            uvsGridView.TabIndex = 18;
            // 
            // UVName
            // 
            UVName.HeaderText = "Name";
            UVName.Name = "UVName";
            UVName.ReadOnly = true;
            UVName.Width = 50;
            // 
            // UVEnabled
            // 
            UVEnabled.HeaderText = "Enabled";
            UVEnabled.Name = "UVEnabled";
            UVEnabled.Width = 60;
            // 
            // UVType
            // 
            UVType.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            UVType.HeaderText = "Type";
            UVType.Name = "UVType";
            UVType.Items.AddRange(typeMap.Values.ToArray());
            // 
            // minimalAssetMap
            // 
            minimalAssetMap.AutoSize = true;
            minimalAssetMap.Location = new System.Drawing.Point(7, 129);
            minimalAssetMap.Name = "minimalAssetMap";
            minimalAssetMap.Size = new System.Drawing.Size(125, 19);
            minimalAssetMap.TabIndex = 17;
            minimalAssetMap.Text = "Minimal AssetMap";
            minimalAssetMap.UseVisualStyleBackColor = true;
            // 
            // assetGroupOptions
            // 
            assetGroupOptions.DropDownStyle = ComboBoxStyle.DropDownList;
            assetGroupOptions.FormattingEnabled = true;
            assetGroupOptions.Items.AddRange(new object[] { "type name", "container path", "source file name", "do not group" });
            assetGroupOptions.Location = new System.Drawing.Point(8, 241);
            assetGroupOptions.Margin = new Padding(4);
            assetGroupOptions.Name = "assetGroupOptions";
            assetGroupOptions.Size = new System.Drawing.Size(173, 23);
            assetGroupOptions.TabIndex = 12;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(8, 222);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(140, 15);
            label7.TabIndex = 11;
            label7.Text = "Group exported assets by";
            // 
            // openAfterExport
            // 
            openAfterExport.AutoSize = true;
            openAfterExport.Checked = true;
            openAfterExport.CheckState = CheckState.Checked;
            openAfterExport.Location = new System.Drawing.Point(7, 78);
            openAfterExport.Margin = new Padding(4);
            openAfterExport.Name = "openAfterExport";
            openAfterExport.Size = new System.Drawing.Size(153, 19);
            openAfterExport.TabIndex = 10;
            openAfterExport.Text = "Open folder after export";
            openAfterExport.UseVisualStyleBackColor = true;
            // 
            // restoreExtensionName
            // 
            restoreExtensionName.AutoSize = true;
            restoreExtensionName.Checked = true;
            restoreExtensionName.CheckState = CheckState.Checked;
            restoreExtensionName.Location = new System.Drawing.Point(7, 24);
            restoreExtensionName.Margin = new Padding(4);
            restoreExtensionName.Name = "restoreExtensionName";
            restoreExtensionName.Size = new System.Drawing.Size(204, 19);
            restoreExtensionName.TabIndex = 9;
            restoreExtensionName.Text = "Restore TextAsset extension name";
            restoreExtensionName.UseVisualStyleBackColor = true;
            // 
            // key
            // 
            key.Hexadecimal = true;
            key.Location = new System.Drawing.Point(186, 103);
            key.Margin = new Padding(4, 3, 4, 3);
            key.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            key.Name = "key";
            key.Size = new System.Drawing.Size(55, 23);
            key.TabIndex = 8;
            keyToolTip.SetToolTip(key, "Key in hex");
            // 
            // encrypted
            // 
            encrypted.AutoSize = true;
            encrypted.Checked = true;
            encrypted.CheckState = CheckState.Checked;
            encrypted.Location = new System.Drawing.Point(7, 104);
            encrypted.Margin = new Padding(4, 3, 4, 3);
            encrypted.Name = "encrypted";
            encrypted.Size = new System.Drawing.Size(166, 19);
            encrypted.TabIndex = 12;
            encrypted.Text = "Encrypted MiHoYoBinData\r\n";
            encrypted.UseVisualStyleBackColor = true;
            // 
            // convertAudio
            // 
            convertAudio.AutoSize = true;
            convertAudio.Checked = true;
            convertAudio.CheckState = CheckState.Checked;
            convertAudio.Location = new System.Drawing.Point(7, 51);
            convertAudio.Margin = new Padding(4);
            convertAudio.Name = "convertAudio";
            convertAudio.Size = new System.Drawing.Size(200, 19);
            convertAudio.TabIndex = 6;
            convertAudio.Text = "Convert AudioClip to WAV(PCM)";
            convertAudio.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            panel1.Controls.Add(totga);
            panel1.Controls.Add(tojpg);
            panel1.Controls.Add(topng);
            panel1.Controls.Add(tobmp);
            panel1.Location = new System.Drawing.Point(18, 180);
            panel1.Margin = new Padding(4);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(236, 38);
            panel1.TabIndex = 5;
            // 
            // totga
            // 
            totga.AutoSize = true;
            totga.Location = new System.Drawing.Point(175, 8);
            totga.Margin = new Padding(4);
            totga.Name = "totga";
            totga.Size = new System.Drawing.Size(43, 19);
            totga.TabIndex = 2;
            totga.Text = "Tga";
            totga.UseVisualStyleBackColor = true;
            // 
            // tojpg
            // 
            tojpg.AutoSize = true;
            tojpg.Location = new System.Drawing.Point(113, 8);
            tojpg.Margin = new Padding(4);
            tojpg.Name = "tojpg";
            tojpg.Size = new System.Drawing.Size(49, 19);
            tojpg.TabIndex = 4;
            tojpg.Text = "Jpeg";
            tojpg.UseVisualStyleBackColor = true;
            // 
            // topng
            // 
            topng.AutoSize = true;
            topng.Checked = true;
            topng.Location = new System.Drawing.Point(58, 8);
            topng.Margin = new Padding(4);
            topng.Name = "topng";
            topng.Size = new System.Drawing.Size(46, 19);
            topng.TabIndex = 3;
            topng.TabStop = true;
            topng.Text = "Png";
            topng.UseVisualStyleBackColor = true;
            // 
            // tobmp
            // 
            tobmp.AutoSize = true;
            tobmp.Location = new System.Drawing.Point(4, 8);
            tobmp.Margin = new Padding(4);
            tobmp.Name = "tobmp";
            tobmp.Size = new System.Drawing.Size(50, 19);
            tobmp.TabIndex = 2;
            tobmp.Text = "Bmp";
            tobmp.UseVisualStyleBackColor = true;
            // 
            // converttexture
            // 
            converttexture.AutoSize = true;
            converttexture.Checked = true;
            converttexture.CheckState = CheckState.Checked;
            converttexture.Location = new System.Drawing.Point(7, 153);
            converttexture.Margin = new Padding(4);
            converttexture.Name = "converttexture";
            converttexture.Size = new System.Drawing.Size(123, 19);
            converttexture.TabIndex = 1;
            converttexture.Text = "Convert Texture2D";
            converttexture.UseVisualStyleBackColor = true;
            // 
            // collectAnimations
            // 
            collectAnimations.AutoSize = true;
            collectAnimations.Checked = true;
            collectAnimations.CheckState = CheckState.Checked;
            collectAnimations.Location = new System.Drawing.Point(8, 43);
            collectAnimations.Margin = new Padding(4, 3, 4, 3);
            collectAnimations.Name = "collectAnimations";
            collectAnimations.Size = new System.Drawing.Size(125, 19);
            collectAnimations.TabIndex = 24;
            collectAnimations.Text = "Collect animations";
            collectAnimations.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            groupBox2.AutoSize = true;
            groupBox2.Controls.Add(label9);
            groupBox2.Controls.Add(texsGridView);
            groupBox2.Controls.Add(collectAnimations);
            groupBox2.Controls.Add(exportBlendShape);
            groupBox2.Controls.Add(exportAnimations);
            groupBox2.Controls.Add(scaleFactor);
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(fbxFormat);
            groupBox2.Controls.Add(label4);
            groupBox2.Controls.Add(fbxVersion);
            groupBox2.Controls.Add(label3);
            groupBox2.Controls.Add(boneSize);
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(exportSkins);
            groupBox2.Controls.Add(label1);
            groupBox2.Controls.Add(filterPrecision);
            groupBox2.Controls.Add(castToBone);
            groupBox2.Controls.Add(exportAllNodes);
            groupBox2.Controls.Add(eulerFilter);
            groupBox2.Location = new System.Drawing.Point(292, 15);
            groupBox2.Margin = new Padding(4);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(4);
            groupBox2.Size = new System.Drawing.Size(379, 419);
            groupBox2.TabIndex = 11;
            groupBox2.TabStop = false;
            groupBox2.Text = "Fbx";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(133, 230);
            label9.Margin = new Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(96, 15);
            label9.TabIndex = 28;
            label9.Text = "Texture Mapping";
            // 
            // texsGridView
            // 
            texsGridView.AllowUserToResizeColumns = false;
            texsGridView.AllowUserToResizeRows = false;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
            texsGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle2;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
            texsGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            texsGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            texsGridView.Columns.AddRange(new DataGridViewColumn[] { TexName, TexType });
            dataGridViewCellStyle6.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = DataGridViewTriState.False;
            texsGridView.DefaultCellStyle = dataGridViewCellStyle6;
            texsGridView.Location = new System.Drawing.Point(7, 248);
            texsGridView.Name = "texsGridView";
            dataGridViewCellStyle7.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = DataGridViewTriState.True;
            texsGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle7;
            texsGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle8.Alignment = DataGridViewContentAlignment.MiddleCenter;
            texsGridView.RowsDefaultCellStyle = dataGridViewCellStyle8;
            texsGridView.RowTemplate.Height = 25;
            texsGridView.Size = new System.Drawing.Size(365, 148);
            texsGridView.TabIndex = 27;
            // 
            // TexName
            // 
            TexName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleCenter;
            TexName.DefaultCellStyle = dataGridViewCellStyle4;
            TexName.HeaderText = "Name";
            TexName.Name = "TexName";
            TexName.Resizable = DataGridViewTriState.False;
            // 
            // TexType
            // 
            dataGridViewCellStyle5.Alignment = DataGridViewContentAlignment.MiddleCenter;
            TexType.DefaultCellStyle = dataGridViewCellStyle5;
            TexType.HeaderText = "Type";
            TexType.Name = "TexType";
            TexType.Items.AddRange(typeMap.Values.ToArray());
            TexType.Resizable = DataGridViewTriState.False;
            // 
            // exportBlendShape
            // 
            exportBlendShape.AutoSize = true;
            exportBlendShape.Checked = true;
            exportBlendShape.CheckState = CheckState.Checked;
            exportBlendShape.Location = new System.Drawing.Point(7, 69);
            exportBlendShape.Margin = new Padding(4);
            exportBlendShape.Name = "exportBlendShape";
            exportBlendShape.Size = new System.Drawing.Size(124, 19);
            exportBlendShape.TabIndex = 22;
            exportBlendShape.Text = "Export blendshape";
            exportBlendShape.UseVisualStyleBackColor = true;
            // 
            // exportAnimations
            // 
            exportAnimations.AutoSize = true;
            exportAnimations.Checked = true;
            exportAnimations.CheckState = CheckState.Checked;
            exportAnimations.Location = new System.Drawing.Point(154, 43);
            exportAnimations.Margin = new Padding(4);
            exportAnimations.Name = "exportAnimations";
            exportAnimations.Size = new System.Drawing.Size(122, 19);
            exportAnimations.TabIndex = 21;
            exportAnimations.Text = "Export animations";
            exportAnimations.UseVisualStyleBackColor = true;
            // 
            // scaleFactor
            // 
            scaleFactor.DecimalPlaces = 2;
            scaleFactor.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            scaleFactor.Location = new System.Drawing.Point(103, 185);
            scaleFactor.Margin = new Padding(4);
            scaleFactor.Name = "scaleFactor";
            scaleFactor.Size = new System.Drawing.Size(59, 23);
            scaleFactor.TabIndex = 20;
            scaleFactor.TextAlign = HorizontalAlignment.Center;
            scaleFactor.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(9, 189);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(67, 15);
            label5.TabIndex = 19;
            label5.Text = "ScaleFactor";
            // 
            // fbxFormat
            // 
            fbxFormat.DropDownStyle = ComboBoxStyle.DropDownList;
            fbxFormat.FormattingEnabled = true;
            fbxFormat.Items.AddRange(new object[] { "Binary", "Ascii" });
            fbxFormat.Location = new System.Drawing.Point(272, 121);
            fbxFormat.Margin = new Padding(4);
            fbxFormat.Name = "fbxFormat";
            fbxFormat.Size = new System.Drawing.Size(70, 23);
            fbxFormat.TabIndex = 18;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(189, 125);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(65, 15);
            label4.TabIndex = 17;
            label4.Text = "FBXFormat";
            // 
            // fbxVersion
            // 
            fbxVersion.DropDownStyle = ComboBoxStyle.DropDownList;
            fbxVersion.FormattingEnabled = true;
            fbxVersion.Items.AddRange(new object[] { "6.1", "7.1", "7.2", "7.3", "7.4", "7.5" });
            fbxVersion.Location = new System.Drawing.Point(272, 153);
            fbxVersion.Margin = new Padding(4);
            fbxVersion.Name = "fbxVersion";
            fbxVersion.Size = new System.Drawing.Size(70, 23);
            fbxVersion.TabIndex = 16;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(189, 157);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(65, 15);
            label3.TabIndex = 15;
            label3.Text = "FBXVersion";
            // 
            // boneSize
            // 
            boneSize.Location = new System.Drawing.Point(103, 153);
            boneSize.Margin = new Padding(4);
            boneSize.Name = "boneSize";
            boneSize.Size = new System.Drawing.Size(59, 23);
            boneSize.TabIndex = 11;
            boneSize.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(9, 157);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(54, 15);
            label2.TabIndex = 10;
            label2.Text = "BoneSize";
            // 
            // exportSkins
            // 
            exportSkins.AutoSize = true;
            exportSkins.Checked = true;
            exportSkins.CheckState = CheckState.Checked;
            exportSkins.Location = new System.Drawing.Point(94, 17);
            exportSkins.Margin = new Padding(4);
            exportSkins.Name = "exportSkins";
            exportSkins.Size = new System.Drawing.Size(89, 19);
            exportSkins.TabIndex = 8;
            exportSkins.Text = "Export skins";
            exportSkins.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(9, 125);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(81, 15);
            label1.TabIndex = 7;
            label1.Text = "FilterPrecision";
            // 
            // filterPrecision
            // 
            filterPrecision.DecimalPlaces = 2;
            filterPrecision.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            filterPrecision.Location = new System.Drawing.Point(103, 123);
            filterPrecision.Margin = new Padding(4);
            filterPrecision.Name = "filterPrecision";
            filterPrecision.Size = new System.Drawing.Size(59, 23);
            filterPrecision.TabIndex = 6;
            filterPrecision.Value = new decimal(new int[] { 25, 0, 0, 131072 });
            // 
            // castToBone
            // 
            castToBone.AutoSize = true;
            castToBone.Location = new System.Drawing.Point(154, 69);
            castToBone.Margin = new Padding(4);
            castToBone.Name = "castToBone";
            castToBone.Size = new System.Drawing.Size(143, 19);
            castToBone.TabIndex = 5;
            castToBone.Text = "All nodes cast to bone";
            castToBone.UseVisualStyleBackColor = true;
            // 
            // exportAllNodes
            // 
            exportAllNodes.AutoSize = true;
            exportAllNodes.Checked = true;
            exportAllNodes.CheckState = CheckState.Checked;
            exportAllNodes.Location = new System.Drawing.Point(191, 17);
            exportAllNodes.Margin = new Padding(4);
            exportAllNodes.Name = "exportAllNodes";
            exportAllNodes.Size = new System.Drawing.Size(110, 19);
            exportAllNodes.TabIndex = 4;
            exportAllNodes.Text = "Export all nodes";
            exportAllNodes.UseVisualStyleBackColor = true;
            // 
            // eulerFilter
            // 
            eulerFilter.AutoSize = true;
            eulerFilter.Checked = true;
            eulerFilter.CheckState = CheckState.Checked;
            eulerFilter.Location = new System.Drawing.Point(8, 17);
            eulerFilter.Margin = new Padding(4);
            eulerFilter.Name = "eulerFilter";
            eulerFilter.Size = new System.Drawing.Size(78, 19);
            eulerFilter.TabIndex = 3;
            eulerFilter.Text = "EulerFilter";
            eulerFilter.UseVisualStyleBackColor = true;
            // 
            // ExportOptions
            // 
            AcceptButton = OKbutton;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = Cancel;
            ClientSize = new System.Drawing.Size(677, 480);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(Cancel);
            Controls.Add(OKbutton);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Margin = new Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ExportOptions";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Export options";
            TopMost = true;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)uvsGridView).EndInit();
            ((System.ComponentModel.ISupportInitialize)key).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)texsGridView).EndInit();
            ((System.ComponentModel.ISupportInitialize)scaleFactor).EndInit();
            ((System.ComponentModel.ISupportInitialize)boneSize).EndInit();
            ((System.ComponentModel.ISupportInitialize)filterPrecision).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Button OKbutton;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox converttexture;
        private System.Windows.Forms.RadioButton tojpg;
        private System.Windows.Forms.RadioButton topng;
        private System.Windows.Forms.RadioButton tobmp;
        private System.Windows.Forms.RadioButton totga;
        private System.Windows.Forms.CheckBox convertAudio;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.NumericUpDown boneSize;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox exportSkins;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown filterPrecision;
        private System.Windows.Forms.CheckBox castToBone;
        private System.Windows.Forms.CheckBox exportAllNodes;
        private System.Windows.Forms.CheckBox eulerFilter;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox fbxVersion;
        private System.Windows.Forms.ComboBox fbxFormat;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown scaleFactor;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox exportBlendShape;
        private System.Windows.Forms.CheckBox exportAnimations;
        private System.Windows.Forms.ComboBox assetGroupOptions;
        private System.Windows.Forms.CheckBox restoreExtensionName;
        private System.Windows.Forms.CheckBox openAfterExport;
        private System.Windows.Forms.ToolTip exportUvsTooltip;
        private System.Windows.Forms.CheckBox collectAnimations;
        private System.Windows.Forms.CheckBox encrypted;
        private System.Windows.Forms.NumericUpDown key;
        private System.Windows.Forms.ToolTip keyToolTip;
        private System.Windows.Forms.CheckBox minimalAssetMap;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.DataGridView texsGridView;
        private Label label9;
        private DataGridViewTextBoxColumn TexName;
        private DataGridViewComboBoxColumn TexType;
        private Label label6;
        private DataGridView uvsGridView;
        private DataGridViewTextBoxColumn UVName;
        private DataGridViewCheckBoxColumn UVEnabled;
        private DataGridViewComboBoxColumn UVType;
    }
}
