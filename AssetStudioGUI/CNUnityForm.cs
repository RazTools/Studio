using System;
using System.Windows.Forms;
using System.Collections.Generic;
using AssetStudio;
using System.Linq;

namespace AssetStudioGUI
{
    public partial class CNUnityForm : Form
    {
        public CNUnityForm()
        {
            InitializeComponent();

            var keys = CNUnityKeyManager.GetEntries();

            for (int i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                var rowIdx = specifyCNUnityList.Rows.Add();

                specifyCNUnityList.Rows[rowIdx].Cells["NameField"].Value = key.Name;
                specifyCNUnityList.Rows[rowIdx].Cells["KeyField"].Value = key.Key;
            }

            var index = Properties.Settings.Default.selectedCNUnityKey;
            if (index >= specifyCNUnityList.RowCount)
            {
                index = 0;
            }
            specifyCNUnityList.CurrentCell = specifyCNUnityList.Rows[index].Cells[0];
        }

        private void OKbutton_Click(object sender, EventArgs e)
        {
            var keys = new List<CNUnity.Entry>();
            for (int i = specifyCNUnityList.Rows.Count - 1; i >= 0; i--)
            {
                var row = specifyCNUnityList.Rows[i];
                var name = row.Cells["NameField"].Value as string;
                var key = row.Cells["KeyField"].Value as string;

                if (!(string.IsNullOrEmpty(name) || string.IsNullOrEmpty(key)))
                {
                    var cnunity = new CNUnity.Entry(name, key);

                    if (cnunity.Validate())
                    {
                        keys.Add(cnunity);
                        continue;
                    }
                }

                if (specifyCNUnityList.CurrentCell.RowIndex == row.Index)
                {
                    var previousRow = specifyCNUnityList.Rows.Cast<DataGridViewRow>().ElementAtOrDefault(i - 1);
                    if (previousRow != null)
                    {
                        specifyCNUnityList.CurrentCell = previousRow.Cells[0];
                    }
                }
                if (i != specifyCNUnityList.RowCount - 1)
                {
                    specifyCNUnityList.Rows.RemoveAt(i);
                }
            }
            CNUnityKeyManager.SaveEntries(keys.Reverse<CNUnity.Entry>().ToList());
            CNUnityKeyManager.SetKey(specifyCNUnityList.CurrentRow.Index);

            Properties.Settings.Default.selectedCNUnityKey = specifyCNUnityList.CurrentRow.Index;
            Properties.Settings.Default.Save();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
