namespace AssetStudioGUI
{
    partial class CNUnityForm
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
            this.specifyCNUnityList = new System.Windows.Forms.DataGridView();
            this.NameField = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.KeyField = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.specifyCNUnityList)).BeginInit();
            this.SuspendLayout();
            // 
            // specifyCNUnityList
            // 
            this.specifyCNUnityList.AllowUserToResizeColumns = false;
            this.specifyCNUnityList.AllowUserToResizeRows = false;
            this.specifyCNUnityList.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.specifyCNUnityList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.specifyCNUnityList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.NameField,
            this.KeyField});
            this.specifyCNUnityList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.specifyCNUnityList.Location = new System.Drawing.Point(0, 0);
            this.specifyCNUnityList.MultiSelect = false;
            this.specifyCNUnityList.Name = "specifyCNUnityList";
            this.specifyCNUnityList.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.specifyCNUnityList.RowTemplate.Height = 25;
            this.specifyCNUnityList.Size = new System.Drawing.Size(432, 229);
            this.specifyCNUnityList.TabIndex = 0;
            this.specifyCNUnityList.RowHeaderMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.specifyCNUnityList_RowHeaderMouseDoubleClick);
            // 
            // NameField
            // 
            this.NameField.HeaderText = "Name";
            this.NameField.Name = "NameField";
            this.NameField.Width = 140;
            // 
            // KeyField
            // 
            this.KeyField.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.KeyField.HeaderText = "Key";
            this.KeyField.Name = "KeyField";
            // 
            // CNUnityForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(432, 229);
            this.Controls.Add(this.specifyCNUnityList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CNUnityForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "CNUnityForm";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.specifyCNUnityList)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView specifyCNUnityList;
        private System.Windows.Forms.DataGridViewTextBoxColumn NameField;
        private System.Windows.Forms.DataGridViewTextBoxColumn KeyField;
    }
}