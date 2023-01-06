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
            this.Cancel = new System.Windows.Forms.Button();
            this.OKbutton = new System.Windows.Forms.Button();
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
            this.specifyCNUnityList.Location = new System.Drawing.Point(12, 12);
            this.specifyCNUnityList.MultiSelect = false;
            this.specifyCNUnityList.Name = "specifyCNUnityList";
            this.specifyCNUnityList.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.specifyCNUnityList.RowTemplate.Height = 25;
            this.specifyCNUnityList.Size = new System.Drawing.Size(332, 171);
            this.specifyCNUnityList.TabIndex = 0;
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
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(255, 190);
            this.Cancel.Margin = new System.Windows.Forms.Padding(4);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(88, 26);
            this.Cancel.TabIndex = 9;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // OKbutton
            // 
            this.OKbutton.Location = new System.Drawing.Point(159, 190);
            this.OKbutton.Margin = new System.Windows.Forms.Padding(4);
            this.OKbutton.Name = "OKbutton";
            this.OKbutton.Size = new System.Drawing.Size(88, 26);
            this.OKbutton.TabIndex = 8;
            this.OKbutton.Text = "OK";
            this.OKbutton.UseVisualStyleBackColor = true;
            this.OKbutton.Click += new System.EventHandler(this.OKbutton_Click);
            // 
            // CNUnityForm
            // 
            this.AcceptButton = this.OKbutton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(356, 229);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.OKbutton);
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
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button OKbutton;
        private System.Windows.Forms.DataGridViewTextBoxColumn NameField;
        private System.Windows.Forms.DataGridViewTextBoxColumn KeyField;
    }
}