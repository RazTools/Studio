namespace AssetStudioGUI
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
            assetListView = new System.Windows.Forms.DataGridView();
            fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            loadAssetMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            searchTextBox = new System.Windows.Forms.ToolStripTextBox();
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            ((System.ComponentModel.ISupportInitialize)assetListView).BeginInit();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // assetListView
            // 
            assetListView.AllowUserToAddRows = false;
            assetListView.AllowUserToDeleteRows = false;
            assetListView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            assetListView.Location = new System.Drawing.Point(12, 27);
            assetListView.Name = "assetListView";
            assetListView.ReadOnly = true;
            assetListView.RowTemplate.Height = 25;
            assetListView.Size = new System.Drawing.Size(524, 268);
            assetListView.TabIndex = 2;
            assetListView.RowHeaderMouseDoubleClick += assetListView_RowHeaderMouseDoubleClick;
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { loadAssetMapToolStripMenuItem, clearToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new System.Drawing.Size(37, 23);
            fileToolStripMenuItem.Text = "File";
            // 
            // loadAssetMapToolStripMenuItem
            // 
            loadAssetMapToolStripMenuItem.Name = "loadAssetMapToolStripMenuItem";
            loadAssetMapToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            loadAssetMapToolStripMenuItem.Text = "Load AssetMap";
            loadAssetMapToolStripMenuItem.Click += loadAssetMapToolStripMenuItem_Click;
            // 
            // clearToolStripMenuItem
            // 
            clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            clearToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            clearToolStripMenuItem.Text = "Clear";
            clearToolStripMenuItem.Click += clearToolStripMenuItem_Click;
            // 
            // searchTextBox
            // 
            searchTextBox.Name = "searchTextBox";
            searchTextBox.Size = new System.Drawing.Size(500, 23);
            searchTextBox.KeyPress += toolStripTextBox1_KeyPress;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileToolStripMenuItem, searchTextBox });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(548, 27);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // AssetBrowser
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(548, 307);
            Controls.Add(assetListView);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "AssetBrowser";
            ShowIcon = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Asset Browser";
            ((System.ComponentModel.ISupportInitialize)assetListView).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.DataGridView assetListView;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadAssetMapToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox searchTextBox;
        private System.Windows.Forms.MenuStrip menuStrip1;
    }
}