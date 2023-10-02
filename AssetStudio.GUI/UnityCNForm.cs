using System;
using System.Windows.Forms;
using System.Collections.Generic;
using AssetStudio;
using System.Linq;

namespace AssetStudio.GUI
{
    public partial class UnityCNForm : Form
    {
        public UnityCNForm()
        {
            InitializeComponent();

            var keys = UnityCNManager.GetEntries();

            for (int i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                var rowIdx = specifyUnityCNList.Rows.Add();

                specifyUnityCNList.Rows[rowIdx].Cells["NameField"].Value = key.Name;
                specifyUnityCNList.Rows[rowIdx].Cells["KeyField"].Value = key.Key;
            }

            var index = Properties.Settings.Default.selectedUnityCNKey;
            if (index >= specifyUnityCNList.RowCount)
            {
                index = 0;
            }
            specifyUnityCNList.CurrentCell = specifyUnityCNList.Rows[index].Cells[0];
        }

        private void specifyUnityCNList_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            var keys = new List<UnityCN.Entry>();
            for (int i = specifyUnityCNList.Rows.Count - 1; i >= 0; i--)
            {
                var row = specifyUnityCNList.Rows[i];
                var name = row.Cells["NameField"].Value as string;
                var key = row.Cells["KeyField"].Value as string;

                if (!(string.IsNullOrEmpty(name) || string.IsNullOrEmpty(key)))
                {
                    var unityCN = new UnityCN.Entry(name, key);

                    if (unityCN.Validate())
                    {
                        keys.Add(unityCN);
                        continue;
                    }
                }

                if (specifyUnityCNList.CurrentCell.RowIndex == row.Index)
                {
                    var previousRow = specifyUnityCNList.Rows.Cast<DataGridViewRow>().ElementAtOrDefault(i - 1);
                    if (previousRow != null)
                    {
                        specifyUnityCNList.CurrentCell = previousRow.Cells[0];
                    }
                }
                if (i != specifyUnityCNList.RowCount - 1)
                {
                    specifyUnityCNList.Rows.RemoveAt(i);
                }
            }
            UnityCNManager.SaveEntries(keys.Reverse<UnityCN.Entry>().ToList());

            if (Studio.Game.Type.IsUnityCN())
            {
                UnityCNManager.SetKey(specifyUnityCNList.CurrentRow.Index);
            }

            Properties.Settings.Default.selectedUnityCNKey = specifyUnityCNList.CurrentRow.Index;
            Properties.Settings.Default.Save();

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
