using System.Configuration;
using System.Windows.Forms;

namespace AssetStudio.GUI
{
    partial class AssetBrowser
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
            tableLayoutPanel2 = new TableLayoutPanel();
            loadAssetMap = new Button();
            clear = new Button();
            loadSelected = new Button();
            exportSelected = new Button();
            assetDataGridView = new DataGridView();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel3 = new TableLayoutPanel();
            sourceTextBox = new TextBox();
            pathTextBox = new TextBox();
            nameTextBox = new TextBox();
            containerTextBox = new TextBox();
            typeTextBox = new TextBox();
            tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)assetDataGridView).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel2.ColumnCount = 4;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel2.Controls.Add(loadAssetMap, 0, 0);
            tableLayoutPanel2.Controls.Add(clear, 1, 0);
            tableLayoutPanel2.Controls.Add(loadSelected, 2, 0);
            tableLayoutPanel2.Controls.Add(exportSelected, 3, 0);
            tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Size = new System.Drawing.Size(518, 29);
            tableLayoutPanel2.TabIndex = 3;
            // 
            // loadAssetMap
            // 
            loadAssetMap.Dock = DockStyle.Fill;
            loadAssetMap.Location = new System.Drawing.Point(3, 3);
            loadAssetMap.Name = "loadAssetMap";
            loadAssetMap.Size = new System.Drawing.Size(114, 23);
            loadAssetMap.TabIndex = 0;
            loadAssetMap.Text = "Load AssetMap";
            loadAssetMap.UseVisualStyleBackColor = true;
            loadAssetMap.Click += loadAssetMap_Click;
            // 
            // clear
            // 
            clear.Dock = DockStyle.Fill;
            clear.Location = new System.Drawing.Point(123, 3);
            clear.Name = "clear";
            clear.Size = new System.Drawing.Size(54, 23);
            clear.TabIndex = 1;
            clear.Text = "Clear";
            clear.UseVisualStyleBackColor = true;
            clear.Click += clear_Click;
            // 
            // loadSelected
            // 
            loadSelected.Dock = DockStyle.Fill;
            loadSelected.Location = new System.Drawing.Point(183, 3);
            loadSelected.Name = "loadSelected";
            loadSelected.Size = new System.Drawing.Size(114, 23);
            loadSelected.TabIndex = 2;
            loadSelected.Text = "Load Selected";
            loadSelected.UseVisualStyleBackColor = true;
            loadSelected.Click += loadSelected_Click;
            // 
            // exportSelected
            // 
            exportSelected.Dock = DockStyle.Fill;
            exportSelected.Location = new System.Drawing.Point(303, 3);
            exportSelected.Name = "exportSelected";
            exportSelected.Size = new System.Drawing.Size(212, 23);
            exportSelected.TabIndex = 3;
            exportSelected.Text = "Export Selected";
            exportSelected.UseVisualStyleBackColor = true;
            exportSelected.Click += exportSelected_Click;
            // 
            // assetDataGridView
            // 
            assetDataGridView.AllowUserToAddRows = false;
            assetDataGridView.AllowUserToDeleteRows = false;
            assetDataGridView.AllowUserToResizeRows = false;
            assetDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            assetDataGridView.Dock = DockStyle.Fill;
            assetDataGridView.Location = new System.Drawing.Point(3, 73);
            assetDataGridView.Name = "assetDataGridView";
            assetDataGridView.ReadOnly = true;
            assetDataGridView.RowTemplate.Height = 25;
            assetDataGridView.Size = new System.Drawing.Size(518, 263);
            assetDataGridView.TabIndex = 2;
            assetDataGridView.VirtualMode = true;
            assetDataGridView.CellValueNeeded += AssetDataGridView_CellValueNeeded;
            assetDataGridView.ColumnHeaderMouseClick += AssetListView_ColumnHeaderMouseClick;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(assetDataGridView, 0, 2);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 0);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel3, 0, 1);
            tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new System.Drawing.Size(524, 333);
            tableLayoutPanel1.TabIndex = 3;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 5;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel3.Controls.Add(sourceTextBox, 0, 0);
            tableLayoutPanel3.Controls.Add(pathTextBox, 0, 0);
            tableLayoutPanel3.Controls.Add(nameTextBox, 0, 0);
            tableLayoutPanel3.Controls.Add(containerTextBox, 0, 0);
            tableLayoutPanel3.Controls.Add(typeTextBox, 4, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new System.Drawing.Point(3, 38);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Size = new System.Drawing.Size(518, 29);
            tableLayoutPanel3.TabIndex = 4;
            // 
            // sourceTextBox
            // 
            sourceTextBox.Dock = DockStyle.Fill;
            sourceTextBox.Location = new System.Drawing.Point(209, 3);
            sourceTextBox.Name = "sourceTextBox";
            sourceTextBox.PlaceholderText = "Source";
            sourceTextBox.Size = new System.Drawing.Size(97, 23);
            sourceTextBox.TabIndex = 6;
            sourceTextBox.KeyPress += SourceTextBox_KeyPress;
            // 
            // pathTextBox
            // 
            pathTextBox.Dock = DockStyle.Fill;
            pathTextBox.Location = new System.Drawing.Point(312, 3);
            pathTextBox.Name = "pathTextBox";
            pathTextBox.PlaceholderText = "PathID";
            pathTextBox.Size = new System.Drawing.Size(97, 23);
            pathTextBox.TabIndex = 7;
            pathTextBox.KeyPress += PathTextBox_KeyPress;
            // 
            // nameTextBox
            // 
            nameTextBox.Dock = DockStyle.Fill;
            nameTextBox.Location = new System.Drawing.Point(3, 3);
            nameTextBox.Name = "nameTextBox";
            nameTextBox.PlaceholderText = "Name";
            nameTextBox.Size = new System.Drawing.Size(97, 23);
            nameTextBox.TabIndex = 4;
            nameTextBox.KeyPress += NameTextBox_KeyPress;
            // 
            // containerTextBox
            // 
            containerTextBox.Dock = DockStyle.Fill;
            containerTextBox.Location = new System.Drawing.Point(106, 3);
            containerTextBox.Name = "containerTextBox";
            containerTextBox.PlaceholderText = "Container";
            containerTextBox.Size = new System.Drawing.Size(97, 23);
            containerTextBox.TabIndex = 5;
            containerTextBox.KeyPress += ContainerTextBox_KeyPress;
            // 
            // typeTextBox
            // 
            typeTextBox.Dock = DockStyle.Fill;
            typeTextBox.Location = new System.Drawing.Point(415, 3);
            typeTextBox.Name = "typeTextBox";
            typeTextBox.PlaceholderText = "Type";
            typeTextBox.Size = new System.Drawing.Size(100, 23);
            typeTextBox.TabIndex = 8;
            typeTextBox.KeyPress += TypeTextBox_KeyPress;
            // 
            // AssetBrowser
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(548, 357);
            Controls.Add(tableLayoutPanel1);
            Name = "AssetBrowser";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Asset Browser";
            FormClosing += AssetBrowser_FormClosing;
            tableLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)assetDataGridView).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            ResumeLayout(false);
        }



        #endregion

        private TableLayoutPanel tableLayoutPanel2;
        private Button loadAssetMap;
        private Button clear;
        private Button loadSelected;
        private Button exportSelected;
        private DataGridView assetDataGridView;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel3;
        private TextBox sourceTextBox;
        private TextBox pathTextBox;
        private TextBox nameTextBox;
        private TextBox containerTextBox;
        private TextBox typeTextBox;
    }
}