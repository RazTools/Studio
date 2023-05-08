using System;
using System.IO;
using System.Linq;
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

        private async void loadAssetMap_Click(object sender, EventArgs e)
        {
            loadAssetMap.Enabled = false;

            var openFileDialog = new OpenFileDialog() { Multiselect = false, Filter = "AssetMap File|*.map" };
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                var path = openFileDialog.FileName;
                Logger.Info($"Loading AssetMap...");
                await Task.Run(() => ResourceMap.FromFile(path));
                assetListView.DataSource = ResourceMap.GetEntries();
                assetListView.Columns.GetLastColumn(DataGridViewElementStates.None, DataGridViewElementStates.None).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            loadAssetMap.Enabled = true;
        }
        private void clear_Click(object sender, EventArgs e)
        {
            ResourceMap.Clear();
            assetListView.DataSource = null;
            Logger.Info($"Cleared !!");
        }
        private async void loadSelected_Click(object sender, EventArgs e)
        {
            var files = assetListView.SelectedRows.Cast<DataGridViewRow>().Select(x => x.DataBoundItem as AssetEntry).Select(x => x.Source).ToHashSet();

            if (files.Count != 0 && !files.Any(x => string.IsNullOrEmpty(x)))
            {
                Logger.Info("Loading...");
                _parent.Invoke(() => _parent.LoadPaths(files.ToArray()));
            }
        }

        private void searchTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                var value = searchTextBox.Text;
                var assets = ResourceMap.GetEntries();
                if (assets.Length != 0 && !string.IsNullOrEmpty(value))
                {
                    var regex = new Regex(value);
                    assetListView.DataSource = Array.FindAll(assets, x => x.Matches(regex));
                }
                else
                {
                    assetListView.DataSource = assets;
                }
            }
        }
    }
}
