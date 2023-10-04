using System;
using System.Linq;

namespace AssetStudio
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            OKbutton = new System.Windows.Forms.Button();
            Cancel = new System.Windows.Forms.Button();
            groupBox1 = new System.Windows.Forms.GroupBox();
            typesGridView = new System.Windows.Forms.DataGridView();
            TypeName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            TypeParse = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            TypeExport = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            assetGroupOptions = new System.Windows.Forms.ComboBox();
            minimalAssetMap = new System.Windows.Forms.CheckBox();
            label7 = new System.Windows.Forms.Label();
            openAfterExport = new System.Windows.Forms.CheckBox();
            converttexture = new System.Windows.Forms.CheckBox();
            restoreExtensionName = new System.Windows.Forms.CheckBox();
            key = new System.Windows.Forms.NumericUpDown();
            convertAudio = new System.Windows.Forms.CheckBox();
            encrypted = new System.Windows.Forms.CheckBox();
            panel1 = new System.Windows.Forms.Panel();
            totga = new System.Windows.Forms.RadioButton();
            tojpg = new System.Windows.Forms.RadioButton();
            topng = new System.Windows.Forms.RadioButton();
            tobmp = new System.Windows.Forms.RadioButton();
            collectAnimations = new System.Windows.Forms.CheckBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            label8 = new System.Windows.Forms.Label();
            texsGridView = new System.Windows.Forms.DataGridView();
            TexName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            TexType = new System.Windows.Forms.DataGridViewComboBoxColumn();
            label6 = new System.Windows.Forms.Label();
            uvExportList = new System.Windows.Forms.CheckedListBox();
            exportBlendShape = new System.Windows.Forms.CheckBox();
            exportAnimations = new System.Windows.Forms.CheckBox();
            scaleFactor = new System.Windows.Forms.NumericUpDown();
            label5 = new System.Windows.Forms.Label();
            fbxFormat = new System.Windows.Forms.ComboBox();
            label4 = new System.Windows.Forms.Label();
            fbxVersion = new System.Windows.Forms.ComboBox();
            label3 = new System.Windows.Forms.Label();
            boneSize = new System.Windows.Forms.NumericUpDown();
            label2 = new System.Windows.Forms.Label();
            exportSkins = new System.Windows.Forms.CheckBox();
            label1 = new System.Windows.Forms.Label();
            filterPrecision = new System.Windows.Forms.NumericUpDown();
            castToBone = new System.Windows.Forms.CheckBox();
            exportAllNodes = new System.Windows.Forms.CheckBox();
            eulerFilter = new System.Windows.Forms.CheckBox();
            exportUvsTooltip = new System.Windows.Forms.ToolTip(components);
            keyToolTip = new System.Windows.Forms.ToolTip(components);
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)typesGridView).BeginInit();
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
            OKbutton.Location = new System.Drawing.Point(461, 441);
            OKbutton.Margin = new System.Windows.Forms.Padding(4);
            OKbutton.Name = "OKbutton";
            OKbutton.Size = new System.Drawing.Size(88, 26);
            OKbutton.TabIndex = 6;
            OKbutton.Text = "OK";
            OKbutton.UseVisualStyleBackColor = true;
            OKbutton.Click += OKbutton_Click;
            // 
            // Cancel
            // 
            Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Cancel.Location = new System.Drawing.Point(557, 441);
            Cancel.Margin = new System.Windows.Forms.Padding(4);
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
            groupBox1.Controls.Add(typesGridView);
            groupBox1.Controls.Add(assetGroupOptions);
            groupBox1.Controls.Add(minimalAssetMap);
            groupBox1.Controls.Add(label7);
            groupBox1.Controls.Add(openAfterExport);
            groupBox1.Controls.Add(converttexture);
            groupBox1.Controls.Add(restoreExtensionName);
            groupBox1.Controls.Add(key);
            groupBox1.Controls.Add(convertAudio);
            groupBox1.Controls.Add(encrypted);
            groupBox1.Controls.Add(panel1);
            groupBox1.Location = new System.Drawing.Point(14, 15);
            groupBox1.Margin = new System.Windows.Forms.Padding(4);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(4);
            groupBox1.Size = new System.Drawing.Size(289, 418);
            groupBox1.TabIndex = 9;
            groupBox1.TabStop = false;
            groupBox1.Text = "Export";
            // 
            // typesGridView
            // 
            typesGridView.AllowUserToAddRows = false;
            typesGridView.AllowUserToDeleteRows = false;
            typesGridView.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            typesGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            typesGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            typesGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { TypeName, TypeParse, TypeExport });
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            typesGridView.DefaultCellStyle = dataGridViewCellStyle2;
            typesGridView.Location = new System.Drawing.Point(7, 273);
            typesGridView.Name = "typesGridView";
            typesGridView.RowHeadersVisible = false;
            typesGridView.RowTemplate.Height = 25;
            typesGridView.Size = new System.Drawing.Size(275, 114);
            typesGridView.TabIndex = 19;
            // 
            // TypeName
            // 
            TypeName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            TypeName.HeaderText = "Types";
            TypeName.Name = "TypeName";
            TypeName.ReadOnly = true;
            TypeName.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // TypeParse
            // 
            TypeParse.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            TypeParse.HeaderText = "Parse";
            TypeParse.Name = "TypeParse";
            TypeParse.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            TypeParse.Width = 60;
            // 
            // TypeExport
            // 
            TypeExport.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            TypeExport.HeaderText = "Export";
            TypeExport.Name = "TypeExport";
            TypeExport.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            TypeExport.Width = 70;
            // 
            // assetGroupOptions
            // 
            assetGroupOptions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            assetGroupOptions.FormattingEnabled = true;
            assetGroupOptions.Items.AddRange(new object[] { "type name", "container path", "source file name", "do not group" });
            assetGroupOptions.Location = new System.Drawing.Point(23, 241);
            assetGroupOptions.Margin = new System.Windows.Forms.Padding(4);
            assetGroupOptions.Name = "assetGroupOptions";
            assetGroupOptions.Size = new System.Drawing.Size(173, 23);
            assetGroupOptions.TabIndex = 12;
            // 
            // minimalAssetMap
            // 
            minimalAssetMap.AutoSize = true;
            minimalAssetMap.Location = new System.Drawing.Point(8, 128);
            minimalAssetMap.Name = "minimalAssetMap";
            minimalAssetMap.Size = new System.Drawing.Size(125, 19);
            minimalAssetMap.TabIndex = 17;
            minimalAssetMap.Text = "Minimal AssetMap";
            minimalAssetMap.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(8, 223);
            label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(140, 15);
            label7.TabIndex = 11;
            label7.Text = "Group exported assets by";
            // 
            // openAfterExport
            // 
            openAfterExport.AutoSize = true;
            openAfterExport.Checked = true;
            openAfterExport.CheckState = System.Windows.Forms.CheckState.Checked;
            openAfterExport.Location = new System.Drawing.Point(8, 78);
            openAfterExport.Margin = new System.Windows.Forms.Padding(4);
            openAfterExport.Name = "openAfterExport";
            openAfterExport.Size = new System.Drawing.Size(153, 19);
            openAfterExport.TabIndex = 10;
            openAfterExport.Text = "Open folder after export";
            openAfterExport.UseVisualStyleBackColor = true;
            // 
            // converttexture
            // 
            converttexture.AutoSize = true;
            converttexture.Checked = true;
            converttexture.CheckState = System.Windows.Forms.CheckState.Checked;
            converttexture.Location = new System.Drawing.Point(8, 154);
            converttexture.Margin = new System.Windows.Forms.Padding(4);
            converttexture.Name = "converttexture";
            converttexture.Size = new System.Drawing.Size(123, 19);
            converttexture.TabIndex = 1;
            converttexture.Text = "Convert Texture2D";
            converttexture.UseVisualStyleBackColor = true;
            // 
            // restoreExtensionName
            // 
            restoreExtensionName.AutoSize = true;
            restoreExtensionName.Checked = true;
            restoreExtensionName.CheckState = System.Windows.Forms.CheckState.Checked;
            restoreExtensionName.Location = new System.Drawing.Point(8, 24);
            restoreExtensionName.Margin = new System.Windows.Forms.Padding(4);
            restoreExtensionName.Name = "restoreExtensionName";
            restoreExtensionName.Size = new System.Drawing.Size(204, 19);
            restoreExtensionName.TabIndex = 9;
            restoreExtensionName.Text = "Restore TextAsset extension name";
            restoreExtensionName.UseVisualStyleBackColor = true;
            // 
            // key
            // 
            key.Hexadecimal = true;
            key.Location = new System.Drawing.Point(181, 103);
            key.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            key.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            key.Name = "key";
            key.Size = new System.Drawing.Size(55, 23);
            key.TabIndex = 8;
            keyToolTip.SetToolTip(key, "Key in hex");
            // 
            // convertAudio
            // 
            convertAudio.AutoSize = true;
            convertAudio.Checked = true;
            convertAudio.CheckState = System.Windows.Forms.CheckState.Checked;
            convertAudio.Location = new System.Drawing.Point(8, 51);
            convertAudio.Margin = new System.Windows.Forms.Padding(4);
            convertAudio.Name = "convertAudio";
            convertAudio.Size = new System.Drawing.Size(200, 19);
            convertAudio.TabIndex = 6;
            convertAudio.Text = "Convert AudioClip to WAV(PCM)";
            convertAudio.UseVisualStyleBackColor = true;
            // 
            // encrypted
            // 
            encrypted.AutoSize = true;
            encrypted.Checked = true;
            encrypted.CheckState = System.Windows.Forms.CheckState.Checked;
            encrypted.Location = new System.Drawing.Point(8, 103);
            encrypted.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            encrypted.Name = "encrypted";
            encrypted.Size = new System.Drawing.Size(166, 19);
            encrypted.TabIndex = 12;
            encrypted.Text = "Encrypted MiHoYoBinData\r\n";
            encrypted.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            panel1.Controls.Add(totga);
            panel1.Controls.Add(tojpg);
            panel1.Controls.Add(topng);
            panel1.Controls.Add(tobmp);
            panel1.Location = new System.Drawing.Point(23, 181);
            panel1.Margin = new System.Windows.Forms.Padding(4);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(236, 38);
            panel1.TabIndex = 5;
            // 
            // totga
            // 
            totga.AutoSize = true;
            totga.Location = new System.Drawing.Point(175, 8);
            totga.Margin = new System.Windows.Forms.Padding(4);
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
            tojpg.Margin = new System.Windows.Forms.Padding(4);
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
            topng.Margin = new System.Windows.Forms.Padding(4);
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
            tobmp.Margin = new System.Windows.Forms.Padding(4);
            tobmp.Name = "tobmp";
            tobmp.Size = new System.Drawing.Size(50, 19);
            tobmp.TabIndex = 2;
            tobmp.Text = "Bmp";
            tobmp.UseVisualStyleBackColor = true;
            // 
            // collectAnimations
            // 
            collectAnimations.AutoSize = true;
            collectAnimations.Checked = true;
            collectAnimations.CheckState = System.Windows.Forms.CheckState.Checked;
            collectAnimations.Location = new System.Drawing.Point(159, 51);
            collectAnimations.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            collectAnimations.Name = "collectAnimations";
            collectAnimations.Size = new System.Drawing.Size(125, 19);
            collectAnimations.TabIndex = 24;
            collectAnimations.Text = "Collect animations";
            collectAnimations.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            groupBox2.AutoSize = true;
            groupBox2.Controls.Add(label8);
            groupBox2.Controls.Add(texsGridView);
            groupBox2.Controls.Add(label6);
            groupBox2.Controls.Add(uvExportList);
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
            groupBox2.Location = new System.Drawing.Point(311, 15);
            groupBox2.Margin = new System.Windows.Forms.Padding(4);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new System.Windows.Forms.Padding(4);
            groupBox2.Size = new System.Drawing.Size(343, 418);
            groupBox2.TabIndex = 11;
            groupBox2.TabStop = false;
            groupBox2.Text = "Fbx";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(239, 219);
            label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(96, 15);
            label8.TabIndex = 32;
            label8.Text = "Texture Mapping";
            // 
            // texsGridView
            // 
            texsGridView.AllowUserToResizeColumns = false;
            texsGridView.AllowUserToResizeRows = false;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            texsGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle3;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            texsGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            texsGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            texsGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { TexName, TexType });
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            texsGridView.DefaultCellStyle = dataGridViewCellStyle7;
            texsGridView.Location = new System.Drawing.Point(71, 241);
            texsGridView.Name = "texsGridView";
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            texsGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle8;
            texsGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            texsGridView.RowsDefaultCellStyle = dataGridViewCellStyle9;
            texsGridView.RowTemplate.Height = 25;
            texsGridView.Size = new System.Drawing.Size(265, 148);
            texsGridView.TabIndex = 31;
            // 
            // TexName
            // 
            TexName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            TexName.DefaultCellStyle = dataGridViewCellStyle5;
            TexName.HeaderText = "Name";
            TexName.Name = "TexName";
            TexName.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // TexType
            // 
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            TexType.DefaultCellStyle = dataGridViewCellStyle6;
            TexType.HeaderText = "Type";
            TexType.Items.AddRange(new object[] { "Diffuse", "NormalMap", "Specular", "Bump" });
            TexType.Name = "TexType";
            TexType.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(8, 219);
            label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(59, 15);
            label6.TabIndex = 30;
            label6.Text = "UV Export";
            // 
            // uvExportList
            // 
            uvExportList.CheckOnClick = true;
            uvExportList.FormattingEnabled = true;
            uvExportList.Items.AddRange(new object[] { "UV0", "UV1", "UV2", "UV3", "UV4", "UV5", "UV6", "UV7" });
            uvExportList.Location = new System.Drawing.Point(9, 241);
            uvExportList.Name = "uvExportList";
            uvExportList.Size = new System.Drawing.Size(55, 148);
            uvExportList.TabIndex = 29;
            // 
            // exportBlendShape
            // 
            exportBlendShape.AutoSize = true;
            exportBlendShape.Checked = true;
            exportBlendShape.CheckState = System.Windows.Forms.CheckState.Checked;
            exportBlendShape.Location = new System.Drawing.Point(159, 78);
            exportBlendShape.Margin = new System.Windows.Forms.Padding(4);
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
            exportAnimations.CheckState = System.Windows.Forms.CheckState.Checked;
            exportAnimations.Location = new System.Drawing.Point(8, 51);
            exportAnimations.Margin = new System.Windows.Forms.Padding(4);
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
            scaleFactor.Location = new System.Drawing.Point(97, 194);
            scaleFactor.Margin = new System.Windows.Forms.Padding(4);
            scaleFactor.Name = "scaleFactor";
            scaleFactor.Size = new System.Drawing.Size(70, 23);
            scaleFactor.TabIndex = 20;
            scaleFactor.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            scaleFactor.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(8, 196);
            label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(67, 15);
            label5.TabIndex = 19;
            label5.Text = "ScaleFactor";
            // 
            // fbxFormat
            // 
            fbxFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            fbxFormat.FormattingEnabled = true;
            fbxFormat.Items.AddRange(new object[] { "Binary", "Ascii" });
            fbxFormat.Location = new System.Drawing.Point(248, 132);
            fbxFormat.Margin = new System.Windows.Forms.Padding(4);
            fbxFormat.Name = "fbxFormat";
            fbxFormat.Size = new System.Drawing.Size(86, 23);
            fbxFormat.TabIndex = 18;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(175, 135);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(65, 15);
            label4.TabIndex = 17;
            label4.Text = "FBXFormat";
            // 
            // fbxVersion
            // 
            fbxVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            fbxVersion.FormattingEnabled = true;
            fbxVersion.Items.AddRange(new object[] { "6.1", "7.1", "7.2", "7.3", "7.4", "7.5" });
            fbxVersion.Location = new System.Drawing.Point(248, 171);
            fbxVersion.Margin = new System.Windows.Forms.Padding(4);
            fbxVersion.Name = "fbxVersion";
            fbxVersion.Size = new System.Drawing.Size(86, 23);
            fbxVersion.TabIndex = 16;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(175, 173);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(65, 15);
            label3.TabIndex = 15;
            label3.Text = "FBXVersion";
            // 
            // boneSize
            // 
            boneSize.Location = new System.Drawing.Point(97, 163);
            boneSize.Margin = new System.Windows.Forms.Padding(4);
            boneSize.Name = "boneSize";
            boneSize.Size = new System.Drawing.Size(70, 23);
            boneSize.TabIndex = 11;
            boneSize.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(9, 165);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(54, 15);
            label2.TabIndex = 10;
            label2.Text = "BoneSize";
            // 
            // exportSkins
            // 
            exportSkins.AutoSize = true;
            exportSkins.Checked = true;
            exportSkins.CheckState = System.Windows.Forms.CheckState.Checked;
            exportSkins.Location = new System.Drawing.Point(159, 24);
            exportSkins.Margin = new System.Windows.Forms.Padding(4);
            exportSkins.Name = "exportSkins";
            exportSkins.Size = new System.Drawing.Size(89, 19);
            exportSkins.TabIndex = 8;
            exportSkins.Text = "Export skins";
            exportSkins.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(8, 135);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(81, 15);
            label1.TabIndex = 7;
            label1.Text = "FilterPrecision";
            // 
            // filterPrecision
            // 
            filterPrecision.DecimalPlaces = 2;
            filterPrecision.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            filterPrecision.Location = new System.Drawing.Point(97, 132);
            filterPrecision.Margin = new System.Windows.Forms.Padding(4);
            filterPrecision.Name = "filterPrecision";
            filterPrecision.Size = new System.Drawing.Size(70, 23);
            filterPrecision.TabIndex = 6;
            filterPrecision.Value = new decimal(new int[] { 25, 0, 0, 131072 });
            // 
            // castToBone
            // 
            castToBone.AutoSize = true;
            castToBone.Location = new System.Drawing.Point(8, 24);
            castToBone.Margin = new System.Windows.Forms.Padding(4);
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
            exportAllNodes.CheckState = System.Windows.Forms.CheckState.Checked;
            exportAllNodes.Location = new System.Drawing.Point(8, 78);
            exportAllNodes.Margin = new System.Windows.Forms.Padding(4);
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
            eulerFilter.CheckState = System.Windows.Forms.CheckState.Checked;
            eulerFilter.Location = new System.Drawing.Point(8, 103);
            eulerFilter.Margin = new System.Windows.Forms.Padding(4);
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
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = Cancel;
            ClientSize = new System.Drawing.Size(658, 480);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(Cancel);
            Controls.Add(OKbutton);
            Margin = new System.Windows.Forms.Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ExportOptions";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Export options";
            TopMost = true;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)typesGridView).EndInit();
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
        private System.Windows.Forms.DataGridView typesGridView;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.DataGridView texsGridView;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckedListBox uvExportList;
        private System.Windows.Forms.DataGridViewTextBoxColumn TypeName;
        private System.Windows.Forms.DataGridViewCheckBoxColumn TypeParse;
        private System.Windows.Forms.DataGridViewCheckBoxColumn TypeExport;
        private System.Windows.Forms.DataGridViewTextBoxColumn TexName;
        private System.Windows.Forms.DataGridViewComboBoxColumn TexType;
    }
}
