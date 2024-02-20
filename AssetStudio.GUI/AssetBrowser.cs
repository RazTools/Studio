using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AssetStudio.GUI.Studio;

namespace AssetStudio.GUI
{
    partial class AssetBrowser : Form
    {
        private readonly MainForm _parent;
        private readonly List<AssetEntry> _assetEntries;
        private readonly Dictionary<string, Regex> _filters;

        private SortOrder _sortOrder;
        private DataGridViewColumn _sortedColumn;

        public AssetBrowser(MainForm form)
        {
            InitializeComponent();
            _parent = form;
            _filters = new Dictionary<string, Regex>();
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

                var names = typeof(AssetEntry).GetProperties().Select(x => x.Name);

                _filters.Clear();
                foreach(var name in names)
                {
                    _filters.Add(name, new Regex(""));
                }

                _assetEntries.Clear();
                _assetEntries.AddRange(ResourceMap.GetEntries());

                assetDataGridView.Columns.Clear();
                assetDataGridView.Columns.AddRange(names.Select(x => new DataGridViewTextBoxColumn() { Name = x, HeaderText = x, SortMode = DataGridViewColumnSortMode.Programmatic }).ToArray());
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
        private async void exportSelected_Click(object sender, EventArgs e)
        {
            var saveFolderDialog = new OpenFolderDialog();
            if (saveFolderDialog.ShowDialog(this) == DialogResult.OK)
            {
                var entries = assetDataGridView.SelectedRows.Cast<DataGridViewRow>().Select(x => _assetEntries[x.Index]).ToArray();

                _parent.Invoke(_parent.ResetForm);

                var statusStripUpdate = StatusStripUpdate;
                assetsManager.Game = Studio.Game;
                StatusStripUpdate = Logger.Info;

                var files = new List<string>(entries.Select(x => x.Source).ToHashSet());
                await Task.Run(async () =>
                {
                    for (int i = 0; i < files.Count; i++)
                    {
                        var toExportAssets = new List<AssetItem>();

                        var file = files[i];
                        assetsManager.LoadFiles(file);
                        if (assetsManager.assetsFileList.Count > 0)
                        {
                            BuildAssetData(toExportAssets, entries);
                            await ExportAssets(saveFolderDialog.Folder, toExportAssets, ExportType.Convert, i == files.Count - 1);
                        }
                        toExportAssets.Clear();
                        assetsManager.Clear();
                    }
                });
                StatusStripUpdate = statusStripUpdate;
            }
        }
        private void BuildAssetData(List<AssetItem> exportableAssets, AssetEntry[] entries)
        {
            var objectAssetItemDic = new Dictionary<Object, AssetItem>();
            var mihoyoBinDataNames = new List<(PPtr<Object>, string)>();
            var containers = new List<(PPtr<Object>, string)>();
            foreach (var assetsFile in assetsManager.assetsFileList)
            {
                foreach (var asset in assetsFile.Objects)
                {
                    ProcessAssetData(asset, exportableAssets, objectAssetItemDic, mihoyoBinDataNames, containers);
                }
            }
            foreach ((var pptr, var name) in mihoyoBinDataNames)
            {
                if (pptr.TryGet<MiHoYoBinData>(out var obj))
                {
                    var assetItem = objectAssetItemDic[obj];
                    if (int.TryParse(name, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hash))
                    {
                        assetItem.Text = name;
                        assetItem.Container = hash.ToString();
                    }
                    else assetItem.Text = $"BinFile #{assetItem.m_PathID}";
                }
            }
            foreach ((var pptr, var container) in containers)
            {
                if (pptr.TryGet(out var obj))
                {
                    var item = objectAssetItemDic[obj];
                    item.Container = container;
                }
            }
            containers.Clear();

            var matches = exportableAssets.Where(asset => entries.Any(x => x.Container == asset.Container && x.Name == asset.Text && x.Type == asset.Type && x.PathID == asset.m_PathID)).ToArray();
            exportableAssets.Clear();
            exportableAssets.AddRange(matches);
        }
        private void ProcessAssetData(Object asset, List<AssetItem> exportableAssets, Dictionary<Object, AssetItem> objectAssetItemDic, List<(PPtr<Object>, string)> mihoyoBinDataNames, List<(PPtr<Object>, string)> containers)
        {
            var assetItem = new AssetItem(asset);
            objectAssetItemDic.Add(asset, assetItem);
            var exportable = false;
            switch (asset)
            {
                case GameObject m_GameObject:
                    exportable = ClassIDType.GameObject.CanExport() && m_GameObject.HasModel();
                    break;
                case Texture2D m_Texture2D:
                    if (!string.IsNullOrEmpty(m_Texture2D.m_StreamData?.path))
                        assetItem.FullSize = asset.byteSize + m_Texture2D.m_StreamData.size;
                    exportable = ClassIDType.Texture2D.CanExport();
                    break;
                case AudioClip m_AudioClip:
                    if (!string.IsNullOrEmpty(m_AudioClip.m_Source))
                        assetItem.FullSize = asset.byteSize + m_AudioClip.m_Size;
                    exportable = ClassIDType.AudioClip.CanExport();
                    break;
                case VideoClip m_VideoClip:
                    if (!string.IsNullOrEmpty(m_VideoClip.m_OriginalPath))
                        assetItem.FullSize = asset.byteSize + m_VideoClip.m_ExternalResources.m_Size;
                    exportable = ClassIDType.VideoClip.CanExport();
                    break;
                case MonoBehaviour m_MonoBehaviour:
                    exportable = ClassIDType.MonoBehaviour.CanExport();
                    break;
                case AssetBundle m_AssetBundle:
                    foreach (var m_Container in m_AssetBundle.m_Container)
                    {
                        var preloadIndex = m_Container.Value.preloadIndex;
                        var preloadSize = m_Container.Value.preloadSize;
                        var preloadEnd = preloadIndex + preloadSize;
                        for (int k = preloadIndex; k < preloadEnd; k++)
                        {
                            containers.Add((m_AssetBundle.m_PreloadTable[k], m_Container.Key));
                        }
                    }

                    exportable = ClassIDType.AssetBundle.CanExport();
                    break;
                case IndexObject m_IndexObject:
                    foreach (var index in m_IndexObject.AssetMap)
                    {
                        mihoyoBinDataNames.Add((index.Value.Object, index.Key));
                    }

                    exportable = ClassIDType.IndexObject.CanExport();
                    break;
                case ResourceManager m_ResourceManager:
                    foreach (var m_Container in m_ResourceManager.m_Container)
                    {
                        containers.Add((m_Container.Value, m_Container.Key));
                    }

                    exportable = ClassIDType.GameObject.CanExport();
                    break;
                case Mesh _ when ClassIDType.Mesh.CanExport():
                case TextAsset _ when ClassIDType.TextAsset.CanExport():
                case AnimationClip _ when ClassIDType.AnimationClip.CanExport():
                case Font _ when ClassIDType.Font.CanExport():
                case MovieTexture _ when ClassIDType.MovieTexture.CanExport():
                case Sprite _ when ClassIDType.Sprite.CanExport():
                case Material _ when ClassIDType.Material.CanExport():
                case MiHoYoBinData _ when ClassIDType.MiHoYoBinData.CanExport():
                case Shader _ when ClassIDType.Shader.CanExport():
                case Animator _ when ClassIDType.Animator.CanExport():
                    exportable = true;
                    break;
            }

            if (assetItem.Text == "")
            {
                assetItem.Text = assetItem.TypeString + assetItem.UniqueID;
            }

            if (exportable)
            {
                exportableAssets.Add(assetItem);
            }
        }
        private void FilterAssetDataGrid()
        {
            _assetEntries.Clear();
            _assetEntries.AddRange(ResourceMap.GetEntries().FindAll(x => x.Matches(_filters)));

            assetDataGridView.Rows.Clear();
            assetDataGridView.RowCount = _assetEntries.Count;
            assetDataGridView.Refresh();
        }
        private void TryAddFilter(string name, string value)
        {
            Regex regex;
            try
            {
                regex = new Regex(value, RegexOptions.IgnoreCase);
            }
            catch (Exception)
            {
                Logger.Error($"Invalid regex {value}");
                return;
            }

            if (!_filters.TryGetValue(name, out var filter))
            {
                _filters.Add(name, regex);
            }
            else if (filter != regex)
            {
                _filters[name] = regex;
            }
        }
        private void NameTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (sender is TextBox textBox && e.KeyChar == (char)Keys.Enter)
            {
                var value = textBox.Text;

                TryAddFilter("Name", value);
                FilterAssetDataGrid();
            }
        }
        private void ContainerTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (sender is TextBox textBox && e.KeyChar == (char)Keys.Enter)
            {
                var value = textBox.Text;

                TryAddFilter("Container", value);
                FilterAssetDataGrid();
            }
        }
        private void SourceTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (sender is TextBox textBox && e.KeyChar == (char)Keys.Enter)
            {
                var value = textBox.Text;

                TryAddFilter("Source", value);
                FilterAssetDataGrid();
            }
        }
        private void PathTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (sender is TextBox textBox && e.KeyChar == (char)Keys.Enter)
            {
                var value = textBox.Text;

                TryAddFilter("PathID", value);
                FilterAssetDataGrid();
            }
        }
        private void TypeTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (sender is TextBox textBox && e.KeyChar == (char)Keys.Enter)
            {
                var value = textBox.Text;

                TryAddFilter("Type", value);
                FilterAssetDataGrid();
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
