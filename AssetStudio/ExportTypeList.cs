namespace AssetStudio
{
    public enum ExportListType
    {
        XML,
        JSON
    }
    
    public static class ExportListTypeExtensions
    {
        public static string GetExtension(this ExportListType type) => type switch
        {
            ExportListType.XML => ".xml",
            ExportListType.JSON => ".json",
            _ => throw new System.NotImplementedException(),
        };
    }
}
