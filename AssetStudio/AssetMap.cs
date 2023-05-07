using MessagePack;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AssetStudio
{
    [MessagePackObject]
    public record AssetMap
    {
        [Key(0)]
        public GameType GameType { get; set; }
        [Key(1)]
        public AssetEntry[] AssetEntries { get; set; }
    }
    [MessagePackObject]
    public record AssetEntry
    {
        [Key(0)]
        public string Name { get; set; }
        [Key(1)]
        public string Container { get; set; }
        [Key(2)]
        public string Source { get; set; }
        [Key(3)]
        public long PathID { get; set; }
        [Key(4)]
        public ClassIDType Type { get; set; }

        public bool Matches(Regex regex) => regex.IsMatch(Name)
                || regex.IsMatch(Container)
                || regex.IsMatch(Source)
                || regex.IsMatch(PathID.ToString())
                || regex.IsMatch(Type.ToString());
    }
}
