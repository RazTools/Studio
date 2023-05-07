namespace AssetStudio
{
    public enum ExportListType
    {
        XML,
        JSON,
        MessagePack
    }
    
    public static class ExportListTypeExtensions
    {
        public static string GetExtension(this ExportListType type) => type switch
        {
            ExportListType.XML => ".xml",
            ExportListType.JSON => ".json",
            ExportListType.MessagePack => ".map",
            _ => throw new System.NotImplementedException(),
        };
    }
}
