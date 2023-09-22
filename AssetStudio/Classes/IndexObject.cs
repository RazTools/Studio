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
        public KeyValuePair<string, Index>[] AssetMap;

        public IndexObject(ObjectReader reader) : base(reader)
        {
            Count = reader.ReadInt32();
            AssetMap = new KeyValuePair<string, Index>[Count];
            for (int i = 0; i < Count; i++)
            {
                AssetMap[i] = new KeyValuePair<string, Index>(reader.ReadAlignedString(), new Index(reader));
            }
        }
    } 
}
