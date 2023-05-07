using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using AssetStudio;

namespace AssetStudioGUI
{
    partial class AssetBrowser : Form
    {
        private readonly AssetStudioGUIForm _parent;
        public AssetBrowser(AssetStudioGUIForm form)
        {
            InitializeComponent();
            _parent = form;
        }

        private async void loadAssetMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileToolStripMenuItem.DropDown.Visible = false;

            var openFileDialog = new OpenFileDialog() { Multiselect = false, Filter = "AssetMap File|*.map" };
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                var path = openFileDialog.FileName;
                Logger.Info($"Loading AssetMap...");
                InvokeUpdate(loadAssetMapToolStripMenuItem, false);
                await Task.Run(() => ResourceMap.FromFile(path));
                assetListView.DataSource = ResourceMap.GetEntries();
                InvokeUpdate(loadAssetMapToolStripMenuItem, true);
            }
        }
        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResourceMap.Clear();
            assetListView.DataSource = null;
            Logger.Info($"Cleared !!");
        }
        private async void assetListView_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            var entry = assetListView.CurrentRow.DataBoundItem as AssetEntry;

            if (!string.IsNullOrEmpty(entry.Source))
            {
                Logger.Info("Loading...");
                _parent.Invoke(() => _parent.LoadPaths(entry.Source));
            }
        }
        private void InvokeUpdate(ToolStripItem item, bool value)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => { item.Enabled = value; }));
            }
            else
            {
                item.Enabled = value;
            }
        }

        private void toolStripTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                var value = searchTextBox.Text;
                if (!string.IsNullOrEmpty(value))
                {
                    var regex = new Regex(value);
                    assetListView.DataSource = Array.FindAll(ResourceMap.GetEntries(), x => x.Matches(regex));
                }
                else
                {
                    assetListView.DataSource = ResourceMap.GetEntries();
                }
            }
        }
    }
}
