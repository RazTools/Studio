namespace AssetStudio.GUI
{
    partial class UnityCNForm
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
            this.specifyUnityCNList = new System.Windows.Forms.DataGridView();
            this.NameField = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.KeyField = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.specifyUnityCNList)).BeginInit();
            this.SuspendLayout();
            // 
            // specifyUnityCNList
            // 
            this.specifyUnityCNList.AllowUserToResizeColumns = false;
            this.specifyUnityCNList.AllowUserToResizeRows = false;
            this.specifyUnityCNList.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.specifyUnityCNList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.specifyUnityCNList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.NameField,
            this.KeyField});
            this.specifyUnityCNList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.specifyUnityCNList.Location = new System.Drawing.Point(0, 0);
            this.specifyUnityCNList.MultiSelect = false;
            this.specifyUnityCNList.Name = "specifyUnityCNList";
            this.specifyUnityCNList.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.specifyUnityCNList.RowTemplate.Height = 25;
            this.specifyUnityCNList.Size = new System.Drawing.Size(408, 204);
            this.specifyUnityCNList.TabIndex = 0;
            this.specifyUnityCNList.RowHeaderMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.specifyUnityCNList_RowHeaderMouseDoubleClick);
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
            // UnityCNForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(408, 204);
            this.Controls.Add(this.specifyUnityCNList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UnityCNForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "UnityCNForm";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.specifyUnityCNList)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView specifyUnityCNList;
        private System.Windows.Forms.DataGridViewTextBoxColumn NameField;
        private System.Windows.Forms.DataGridViewTextBoxColumn KeyField;
    }
}