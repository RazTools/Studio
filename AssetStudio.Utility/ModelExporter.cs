namespace AssetStudio
{
    public static class ModelExporter
    {
        public static void ExportFbx(string path, IImported imported, Fbx.ExportOptions exportOptions)
        {
            Fbx.Exporter.Export(path, imported, exportOptions);
        }
    }
}
