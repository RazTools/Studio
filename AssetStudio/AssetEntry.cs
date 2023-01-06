namespace AssetStudio
{
    public class AssetEntry
    {
        public string Name { get; set; } 
        public string Container { get; set; }
        public string Source { get; set; }
        public long PathID { get; set; }
        public ClassIDType Type { get; set; }
    }
}
