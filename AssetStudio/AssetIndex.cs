using System.Collections.Generic;

namespace AssetStudio
{
    public record AssetIndex
    {
        public Dictionary<string, string> Types { get; set; }
        public record SubAssetInfo
        {
            public string Name { get; set; }
            public byte PathHashPre { get; set; }
            public uint PathHashLast { get; set; }
        }
        public Dictionary<int, List<SubAssetInfo>> SubAssets { get; set; }
        public Dictionary<int, List<int>> Dependencies { get; set; }
        public List<uint> PreloadBlocks { get; set; }
        public List<uint> PreloadShaderBlocks { get; set; }
        public record BlockInfo
        {
            public byte Language { get; set; }
            public uint Id { get; set; }
            public uint Offset { get; set; }
        }
        public Dictionary<int, BlockInfo> Assets { get; set; }
        public List<uint> SortList { get; set; }

        public AssetIndex()
        {
            Types = new Dictionary<string, string>();
            SubAssets = new Dictionary<int, List<SubAssetInfo>>();
            Dependencies = new Dictionary<int, List<int>>();
            PreloadBlocks = new List<uint>();
            PreloadShaderBlocks = new List<uint>();
            Assets = new Dictionary<int, BlockInfo>();
            SortList = new List<uint>();
        }
    }
}
