using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace AssetStudio
{
    public partial class ExportOptions : Form
    {
        private Dictionary<int, string> texTypeMap = new Dictionary<int, string>()
        {
            { 0, "Diffuse" },
            { 1, "NormalMap" },
            { 2, "Specular" },
            { 3, "Bump" },
        };

        public ExportOptions()
        {
            InitializeComponent();
            assetGroupOptions.SelectedIndex = Properties.Settings.Default.assetGroupOption;
            restoreExtensionName.Checked = Properties.Settings.Default.restoreExtensionName;
            converttexture.Checked = Properties.Settings.Default.convertTexture;
            convertAudio.Checked = Properties.Settings.Default.convertAudio;
            panel1.Controls.OfType<RadioButton>().ElementAt(Properties.Settings.Default.convertType).Checked = true;
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
            for (int i = 0; i < uvExportList.Items.Count; i++)
            {
                uvExportList.SetItemChecked(i, Properties.Settings.Default.uvExport[i]);
            }
            var types = JsonConvert.DeserializeObject<Dictionary<ClassIDType, (bool, bool)>>(Properties.Settings.Default.types);
            foreach (var type in types)
            {
                var rowIdx = typesGridView.Rows.Add();

                typesGridView.Rows[rowIdx].Cells["TypeName"].Value = type.Key;
                typesGridView.Rows[rowIdx].Cells["TypeParse"].Value = type.Value.Item1;
                typesGridView.Rows[rowIdx].Cells["TypeExport"].Value = type.Value.Item2;
            }
            var texs = JsonConvert.DeserializeObject<Dictionary<string, int>>(Properties.Settings.Default.texs);
            foreach(var tex in texs)
            {
                var rowIdx = texsGridView.Rows.Add();

                texsGridView.Rows[rowIdx].Cells["TexName"].Value = tex.Key;
                texsGridView.Rows[rowIdx].Cells["TexType"].Value = texTypeMap[tex.Value];
            }
        }

        private void OKbutton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.assetGroupOption = assetGroupOptions.SelectedIndex;
            Properties.Settings.Default.restoreExtensionName = restoreExtensionName.Checked;
            Properties.Settings.Default.convertTexture = converttexture.Checked;
            Properties.Settings.Default.convertAudio = convertAudio.Checked;
            Properties.Settings.Default.convertType = panel1.Controls.OfType<RadioButton>().ToList().FindIndex(r => r.Checked);
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
            for (int i = 0; i < uvExportList.Items.Count; i++)
            {
                Properties.Settings.Default.uvExport[i] = uvExportList.GetItemChecked(i);
            }
            foreach (DataGridViewRow type in typesGridView.Rows)
            {
                var typeName = (ClassIDType)type.Cells["TypeName"].Value;
                var typeParse = (bool)type.Cells["TypeParse"].Value;
                var typeExport = (bool)type.Cells["TypeExport"].Value;

                AssetsManager.TypesInfo[typeName] = (typeParse, typeExport);
            }
            Properties.Settings.Default.types = JsonConvert.SerializeObject(AssetsManager.TypesInfo);
            var texs = new Dictionary<string, int>();
            foreach (DataGridViewRow row in texsGridView.Rows)
            {
                var name = row.Cells["TexName"].Value as string;
                var type = row.Cells["TexType"].Value as string;
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(type)) continue;
                texs.Add(name, texTypeMap.FirstOrDefault(x => x.Value == type).Key);
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
