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
            assetDataGridView = new DataGridView();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            loadAssetMap = new Button();
            clear = new Button();
            loadSelected = new Button();
            searchTextBox = new TextBox();
            ((System.ComponentModel.ISupportInitialize)assetDataGridView).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            FormClosing += AssetBrowser_FormClosing;
            SuspendLayout();
            // 
            // assetDataGridView
            // 
            assetDataGridView.AllowUserToAddRows = false;
            assetDataGridView.AllowUserToDeleteRows = false;
            assetDataGridView.AllowUserToResizeRows = false;
            assetDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            assetDataGridView.Dock = DockStyle.Fill;
            assetDataGridView.Location = new System.Drawing.Point(3, 38);
            assetDataGridView.Name = "assetDataGridView";
            assetDataGridView.ReadOnly = true;
            assetDataGridView.RowTemplate.Height = 25;
            assetDataGridView.Size = new System.Drawing.Size(518, 250);
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
            tableLayoutPanel1.Controls.Add(assetDataGridView, 0, 1);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 0);
            tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new System.Drawing.Size(524, 283);
            tableLayoutPanel1.TabIndex = 3;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel2.ColumnCount = 4;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel2.Controls.Add(loadAssetMap, 0, 0);
            tableLayoutPanel2.Controls.Add(clear, 1, 0);
            tableLayoutPanel2.Controls.Add(loadSelected, 2, 0);
            tableLayoutPanel2.Controls.Add(searchTextBox, 3, 0);
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
            loadSelected.Size = new System.Drawing.Size(94, 23);
            loadSelected.TabIndex = 2;
            loadSelected.Text = "Load Selected";
            loadSelected.UseVisualStyleBackColor = true;
            loadSelected.Click += loadSelected_Click;
            // 
            // searchTextBox
            // 
            searchTextBox.Dock = DockStyle.Fill;
            searchTextBox.Location = new System.Drawing.Point(283, 3);
            searchTextBox.Name = "searchTextBox";
            searchTextBox.PlaceholderText = "Column Name=Regex{space}....";
            searchTextBox.Size = new System.Drawing.Size(232, 23);
            searchTextBox.TabIndex = 3;
            searchTextBox.KeyPress += searchTextBox_KeyPress;
            // 
            // AssetBrowser
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(548, 307);
            Controls.Add(tableLayoutPanel1);
            Name = "AssetBrowser";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Asset Browser";
            ((System.ComponentModel.ISupportInitialize)assetDataGridView).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            ResumeLayout(false);
        }



        #endregion
        private System.Windows.Forms.DataGridView assetDataGridView;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private Button loadAssetMap;
        private Button clear;
        private Button loadSelected;
        private TextBox searchTextBox;
    }
}