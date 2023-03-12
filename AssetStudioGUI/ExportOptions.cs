using AssetStudio;
using System;
using System.Windows.Forms;

namespace AssetStudioGUI
{
    public partial class ExportOptions : Form
    {
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
            exportAssetBundle.Checked = Properties.Settings.Default.exportAssetBundle;
            exportIndexObject.Checked = Properties.Settings.Default.exportIndexObject;
            disableRndrr.Checked = Properties.Settings.Default.disableRndrr;
            disableShader.Checked = Properties.Settings.Default.disableShader;
            key.Value = Properties.Settings.Default.key;
            enableXor.Checked = Properties.Settings.Default.enableXor;
            ignoreController.Checked = Properties.Settings.Default.ignoreController;
            eulerFilter.Checked = Properties.Settings.Default.eulerFilter;
            filterPrecision.Value = Properties.Settings.Default.filterPrecision;
            exportAllNodes.Checked = Properties.Settings.Default.exportAllNodes;
            exportSkins.Checked = Properties.Settings.Default.exportSkins;
            exportAnimations.Checked = Properties.Settings.Default.exportAnimations;
            exportBlendShape.Checked = Properties.Settings.Default.exportBlendShape;
            castToBone.Checked = Properties.Settings.Default.castToBone;
            exportAllUvsAsDiffuseMaps.Checked = Properties.Settings.Default.exportAllUvsAsDiffuseMaps;
            exportUV0UV1.Checked = Properties.Settings.Default.exportUV0UV1;
            boneSize.Value = Properties.Settings.Default.boneSize;
            scaleFactor.Value = Properties.Settings.Default.scaleFactor;
            fbxVersion.SelectedIndex = Properties.Settings.Default.fbxVersion;
            fbxFormat.SelectedIndex = Properties.Settings.Default.fbxFormat;
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
            Properties.Settings.Default.exportAssetBundle = exportAssetBundle.Checked;
            AssetBundle.Exportable = Properties.Settings.Default.exportAssetBundle;
            Properties.Settings.Default.exportIndexObject = exportIndexObject.Checked;
            IndexObject.Exportable = Properties.Settings.Default.exportAssetBundle;
            Properties.Settings.Default.disableRndrr = disableRndrr.Checked;
            Renderer.Parsable = !Properties.Settings.Default.disableRndrr;
            Properties.Settings.Default.disableShader = disableShader.Checked;
            Shader.Parsable = !Properties.Settings.Default.disableShader;
            Properties.Settings.Default.key = (byte)key.Value;
            Properties.Settings.Default.enableXor = enableXor.Checked;
            MiHoYoBinData.Key = (byte)key.Value;
            MiHoYoBinData.doXOR = enableXor.Checked;
            Properties.Settings.Default.ignoreController = ignoreController.Checked;
            Properties.Settings.Default.eulerFilter = eulerFilter.Checked;
            Properties.Settings.Default.filterPrecision = filterPrecision.Value;
            Properties.Settings.Default.exportAllNodes = exportAllNodes.Checked;
            Properties.Settings.Default.exportSkins = exportSkins.Checked;
            Properties.Settings.Default.exportAnimations = exportAnimations.Checked;
            Properties.Settings.Default.exportBlendShape = exportBlendShape.Checked;
            Properties.Settings.Default.castToBone = castToBone.Checked;
            Properties.Settings.Default.exportAllUvsAsDiffuseMaps = exportAllUvsAsDiffuseMaps.Checked;
            Properties.Settings.Default.exportUV0UV1 = exportUV0UV1.Checked;
            Properties.Settings.Default.boneSize = boneSize.Value;
            Properties.Settings.Default.scaleFactor = scaleFactor.Value;
            Properties.Settings.Default.fbxVersion = fbxVersion.SelectedIndex;
            Properties.Settings.Default.fbxFormat = fbxFormat.SelectedIndex;
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
