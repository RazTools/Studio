using System;
using System.Collections.Generic;
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
            FormClosing += AssetBrowser_FormClosing;
        }

        private async void loadAssetMap_Click(object sender, EventArgs e)
        {
            loadAssetMap.Enabled = false;

            var openFileDialog = new OpenFileDialog() { Multiselect = false, Filter = "MessagePack AssetMap File|*.map" };
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
            Clear();
            Logger.Info($"Cleared !!");
        }
        private async void loadSelected_Click(object sender, EventArgs e)
        {
            var files = assetListView.SelectedRows.Cast<DataGridViewRow>().Select(x => x.DataBoundItem as AssetEntry).Select(x => x.Source).ToHashSet();
            var missingFiles = files.Where(x => !File.Exists(x));
            foreach (var file in missingFiles)
            {
                Logger.Warning($"Unable to find file {file}, skipping...");
                files.Remove(file);
            }
            if (files.Count != 0 && !files.Any(string.IsNullOrEmpty))
            {
                Logger.Info("Loading...");
                _parent.Invoke(() => _parent.LoadPaths(files.ToArray()));
            }
        }
        private void searchTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                var filters = new Dictionary<string, Regex>();
                var names = typeof(AssetEntry).GetProperties().Select(x => x.Name).ToList();

                var value = searchTextBox.Text;
                var options = value.Split(' ');
                for (int i = 0; i < options.Length; i++)
                {
                    var option = options[i];
                    var arguments = option.Split('=');
                    if (arguments.Length != 2)
                    {
                        Logger.Error($"Invalid argument at index {i + 1}");
                        continue;
                    }
                    var (name, regex) = (arguments[0], arguments[1]);
                    if (!names.Contains(name, StringComparer.OrdinalIgnoreCase))
                    {
                        Logger.Error($"Unknonw argument {name}");
                        continue;
                    }
                    filters[name] = new Regex(regex, RegexOptions.IgnoreCase);
                }

                var assets = ResourceMap.GetEntries();
                if (assets.Length != 0)
                {
                    var regex = new Regex(value, RegexOptions.IgnoreCase);
                    assetListView.DataSource = Array.FindAll(assets, x => x.Matches(filters));
                }
                else
                {
                    assetListView.DataSource = assets;
                }
            }
        }
        private void AssetBrowser_FormClosing(object sender, FormClosingEventArgs e)
        {
            Clear();
            base.OnClosing(e);
        }
        public void Clear()
        {
            ResourceMap.Clear();
            assetListView.DataSource = Array.Empty<AssetEntry>();
        }
    }
}
