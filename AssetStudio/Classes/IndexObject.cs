using System.Collections.Generic;

namespace AssetStudio
{
    public class Index
    {
        public PPtr<Object> Object;
        public ulong Size;

        public Index(ObjectReader reader)
        {
            Object = new PPtr<Object>(reader);
            Size = reader.ReadUInt64();
        }
    }

    public sealed class IndexObject : NamedObject
    {
        public int Count;
        public List<KeyValuePair<string, Index>> AssetMap;

        public override string Name => "IndexObject";

        public IndexObject(ObjectReader reader) : base(reader)
        {
            Count = reader.ReadInt32();
            AssetMap = new List<KeyValuePair<string, Index>>();
            for (int i = 0; i < Count; i++)
            {
                AssetMap.Add(new KeyValuePair<string, Index>(reader.ReadAlignedString(), new Index(reader)));
            }
        }
    } 
}
