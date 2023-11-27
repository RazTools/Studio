using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace AssetStudio.GUI
{
    public partial class ExportOptions : Form
    {
        private Dictionary<int, string> typeMap = new Dictionary<int, string>()
        {
            { 0, "Diffuse" },
            { 1, "NormalMap" },
            { 2, "Specular" },
            { 3, "Bump" },
            { 4, "Ambient" },
            { 5, "Emissive" },
            { 6, "Reflection" },
            { 7, "Displacement" },
        };
        public ExportOptions()
        {
            InitializeComponent();
            assetGroupOptions.SelectedIndex = Properties.Settings.Default.assetGroupOption;
            restoreExtensionName.Checked = Properties.Settings.Default.restoreExtensionName;
            converttexture.Checked = Properties.Settings.Default.convertTexture;
            convertAudio.Checked = Properties.Settings.Default.convertAudio;
            var str = Properties.Settings.Default.convertType.ToString();
            foreach (Control c in panel1.Controls)
            {
                if (c.Text == str)
                {
                    ((RadioButton)c).Checked = true;
                    break;
                }
            }
            openAfterExport.Checked = Properties.Settings.Default.openAfterExport;
            eulerFilter.Checked = Properties.Settings.Default.eulerFilter;
            filterPrecision.Value = Properties.Settings.Default.filterPrecision;
            exportAllNodes.Checked = Properties.Settings.Default.exportAllNodes;
            exportSkins.Checked = Properties.Settings.Default.exportSkins;
            exportAnimations.Checked = Properties.Settings.Default.exportAnimations;
            exportBlendShape.Checked = Properties.Settings.Default.exportBlendShape;
            castToBone.Checked = Properties.Settings.Default.castToBone;
            boneSize.Value = Properties.Settings.Default.boneSize;
            scaleFactor.Value = Properties.Settings.Default.scaleFactor;
            fbxVersion.SelectedIndex = Properties.Settings.Default.fbxVersion;
            fbxFormat.SelectedIndex = Properties.Settings.Default.fbxFormat;
            collectAnimations.Checked = Properties.Settings.Default.collectAnimations;
            encrypted.Checked = Properties.Settings.Default.encrypted;
            key.Value = Properties.Settings.Default.key;
            minimalAssetMap.Checked = Properties.Settings.Default.minimalAssetMap;
            var uvs = JsonConvert.DeserializeObject<Dictionary<string, (bool, int)>>(Properties.Settings.Default.uvs);
            foreach (var uv in uvs)
            {
                var rowIdx = uvsGridView.Rows.Add();

                uvsGridView.Rows[rowIdx].Cells["UVName"].Value = uv.Key;
                uvsGridView.Rows[rowIdx].Cells["UVEnabled"].Value = uv.Value.Item1;
                uvsGridView.Rows[rowIdx].Cells["UVType"].Value = typeMap[uv.Value.Item2];
            }
            var texs = JsonConvert.DeserializeObject<Dictionary<string, int>>(Properties.Settings.Default.texs);
            foreach (var tex in texs)
            {
                var rowIdx = texsGridView.Rows.Add();

                texsGridView.Rows[rowIdx].Cells["TexName"].Value = tex.Key;
                texsGridView.Rows[rowIdx].Cells["TexType"].Value = typeMap[tex.Value];
            }
        }

        private void OKbutton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.assetGroupOption = assetGroupOptions.SelectedIndex;
            Properties.Settings.Default.restoreExtensionName = restoreExtensionName.Checked;
            Properties.Settings.Default.convertTexture = converttexture.Checked;
            Properties.Settings.Default.convertAudio = convertAudio.Checked;
            foreach (Control c in panel1.Controls)
            {
                if (((RadioButton)c).Checked)
                {
                    Properties.Settings.Default.convertType = (ImageFormat)Enum.Parse(typeof(ImageFormat), c.Text);
                    break;
                }
            }
            Properties.Settings.Default.openAfterExport = openAfterExport.Checked;
            Properties.Settings.Default.eulerFilter = eulerFilter.Checked;
            Properties.Settings.Default.filterPrecision = filterPrecision.Value;
            Properties.Settings.Default.exportAllNodes = exportAllNodes.Checked;
            Properties.Settings.Default.exportSkins = exportSkins.Checked;
            Properties.Settings.Default.exportAnimations = exportAnimations.Checked;
            Properties.Settings.Default.exportBlendShape = exportBlendShape.Checked;
            Properties.Settings.Default.castToBone = castToBone.Checked;
            Properties.Settings.Default.boneSize = boneSize.Value;
            Properties.Settings.Default.scaleFactor = scaleFactor.Value;
            Properties.Settings.Default.fbxVersion = fbxVersion.SelectedIndex;
            Properties.Settings.Default.fbxFormat = fbxFormat.SelectedIndex;
            Properties.Settings.Default.collectAnimations = collectAnimations.Checked;
            Properties.Settings.Default.encrypted = encrypted.Checked;
            Properties.Settings.Default.key = (byte)key.Value;
            Properties.Settings.Default.minimalAssetMap = minimalAssetMap.Checked;
            var uvs = new Dictionary<string, (bool, int)>();
            foreach (DataGridViewRow row in uvsGridView.Rows)
            {
                var name = row.Cells["UVName"].Value as string;
                var enabled = (bool)row.Cells["UVEnabled"].Value;
                var type = row.Cells["UVType"].Value as string;
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(type)) continue;
                uvs.Add(name, (enabled, typeMap.FirstOrDefault(x => x.Value == type).Key));
            }
            Properties.Settings.Default.uvs = JsonConvert.SerializeObject(uvs);
            var texs = new Dictionary<string, int>();
            foreach (DataGridViewRow row in texsGridView.Rows)
            {
                var name = row.Cells["TexName"].Value as string;
                var type = row.Cells["TexType"].Value as string;
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(type)) continue;
                texs.Add(name, typeMap.FirstOrDefault(x => x.Value == type).Key);
            }
            Properties.Settings.Default.texs = JsonConvert.SerializeObject(texs);
            Properties.Settings.Default.Save();
            MiHoYoBinData.Key = (byte)key.Value;
            MiHoYoBinData.Encrypted = encrypted.Checked;
            AssetsHelper.Minimal = Properties.Settings.Default.minimalAssetMap;
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
