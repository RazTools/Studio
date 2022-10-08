using Newtonsoft.Json;
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
        public static bool Exportable;

        public int Count;
        public Dictionary<string, Index> AssetMap;
        public Dictionary<long, string> Names = new Dictionary<long, string>();

        public IndexObject(ObjectReader reader) : base(reader)
        {
            Count = reader.ReadInt32();
            AssetMap = new Dictionary<string, Index>(Count);
            for (int i = 0; i < Count; i++)
            {
                var key = reader.ReadAlignedString();
                var value = new Index(reader);

                AssetMap.Add(key, value);

                if (value.Object.m_FileID == 0)
                    Names.Add(value.Object.m_PathID, key);
            }
        }
    } 
}
