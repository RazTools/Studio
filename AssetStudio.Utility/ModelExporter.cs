namespace AssetStudio
{
    public static class ModelExporter
    {
        public static void ExportFbx(string path, IImported imported, bool eulerFilter, float filterPrecision,
            bool allNodes, bool skins, bool animation, bool blendShape, bool castToBone, float boneSize, bool exportAllUvsAsDiffuseMaps, bool exportUV0UV1, float scaleFactor, int versionIndex, bool isAscii)
        {
            Fbx.Exporter.Export(path, imported, eulerFilter, filterPrecision, allNodes, skins, animation, blendShape, castToBone, boneSize, exportAllUvsAsDiffuseMaps, exportUV0UV1, scaleFactor, versionIndex, isAscii);
        }
    }
}
