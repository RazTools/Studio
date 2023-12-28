using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AssetStudio.GUI
{
    partial class AssetBrowser : Form
    {
        private readonly MainForm _parent;
        private readonly List<AssetEntry> _assetEntries;
        private readonly List<string> _columnNames;

        private SortOrder _sortOrder;
        private DataGridViewColumn _sortedColumn;

        public AssetBrowser(MainForm form)
        {
            InitializeComponent();
            _parent = form;
            _columnNames = new List<string>();
            _assetEntries = new List<AssetEntry>();
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

                _sortedColumn = null;
                _columnNames.Clear();
                _columnNames.AddRange(typeof(AssetEntry).GetProperties().Select(x => x.Name).ToList());

                _assetEntries.Clear();
                _assetEntries.AddRange(ResourceMap.GetEntries());

                assetDataGridView.Columns.Clear();
                assetDataGridView.Columns.AddRange(_columnNames.Select(x => new DataGridViewTextBoxColumn() { Name = x, HeaderText = x, SortMode = DataGridViewColumnSortMode.Programmatic }).ToArray());
                assetDataGridView.Columns.GetLastColumn(DataGridViewElementStates.None, DataGridViewElementStates.None).AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                assetDataGridView.Rows.Clear();
                assetDataGridView.RowCount = _assetEntries.Count;
                assetDataGridView.Refresh();
            }
            loadAssetMap.Enabled = true;
        }
        private void clear_Click(object sender, EventArgs e)
        {
            Clear();
            Logger.Info($"Cleared !!");
        }
        private void loadSelected_Click(object sender, EventArgs e)
        {
            var files = assetDataGridView.SelectedRows.Cast<DataGridViewRow>().Select(x => _assetEntries[x.Index]?.Source).ToHashSet();
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
                    if (!_columnNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                    {
                        Logger.Error($"Unknonw argument {name}");
                        continue;
                    }

                    try
                    {
                        filters[name] = new Regex(regex, RegexOptions.IgnoreCase);
                    }
                    catch (Exception)
                    {
                        Logger.Error($"Invalid regex {regex} at argument {name}");
                        continue;
                    }
                }

                _assetEntries.Clear();
                _assetEntries.AddRange(ResourceMap.GetEntries().FindAll(x => x.Matches(filters)));

                assetDataGridView.Rows.Clear();
                assetDataGridView.RowCount = _assetEntries.Count;
                assetDataGridView.Refresh();
            }
        }
        private void AssetDataGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (_assetEntries.Count != 0 && e.RowIndex <= _assetEntries.Count)
            {
                var assetEntry = _assetEntries[e.RowIndex];
                e.Value = e.ColumnIndex switch
                {
                    0 => assetEntry.Name,
                    1 => assetEntry.Container,
                    2 => assetEntry.Source,
                    3 => assetEntry.PathID,
                    4 => assetEntry.Type,
                    _ => ""
                };
            }
        }
        private void AssetListView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex <= assetDataGridView.Columns.Count)
            {
                ListSortDirection direction;
                var column = assetDataGridView.Columns[e.ColumnIndex];

                if (_sortedColumn != null)
                {
                    if (_sortedColumn != column)
                    {
                        direction = ListSortDirection.Ascending;
                        _sortedColumn.HeaderCell.SortGlyphDirection = SortOrder.None;
                        _sortedColumn = column;
                    }
                    else
                    {
                        direction = _sortOrder == SortOrder.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                    }
                }
                else
                {
                    direction = ListSortDirection.Ascending;
                    _sortedColumn = column;
                }

                _sortedColumn.HeaderCell.SortGlyphDirection = _sortOrder = direction == ListSortDirection.Ascending ? SortOrder.Ascending : SortOrder.Descending;

                Func<AssetEntry, object> keySelector = e.ColumnIndex switch
                {
                    0 => x => x.Name,
                    1 => x => x.Container,
                    2 => x => x.Source,
                    3 => x => x.PathID,
                    4 => x => x.Type.ToString(),
                    _ => x => ""
                };

                var sorted = direction == ListSortDirection.Ascending ? _assetEntries.OrderBy(keySelector).ToList() : _assetEntries.OrderByDescending(keySelector).ToList();

                _assetEntries.Clear();
                _assetEntries.AddRange(sorted);

                assetDataGridView.Rows.Clear();
                assetDataGridView.RowCount = _assetEntries.Count;
                assetDataGridView.Refresh();
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
            assetDataGridView.Rows.Clear();
        }
    }
}
