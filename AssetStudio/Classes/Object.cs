using Newtonsoft.Json;
using System.Collections.Specialized;

namespace AssetStudio
{
    public class Object
    {
        [JsonIgnore]
        public SerializedFile assetsFile;
        [JsonIgnore]
        public ObjectReader reader;
        [JsonIgnore]
        public long m_PathID;
        [JsonIgnore]
        public int[] version;
        [JsonIgnore]
        protected BuildType buildType;
        [JsonIgnore]
        public BuildTarget platform;
        [JsonIgnore]
        public ClassIDType type;
        [JsonIgnore]
        public SerializedType serializedType;
        [JsonIgnore]
        public uint byteSize;

        public Object(ObjectReader reader)
        {
            this.reader = reader;
            reader.Reset();
            assetsFile = reader.assetsFile;
            type = reader.type;
            m_PathID = reader.m_PathID;
            version = reader.version;
            buildType = reader.buildType;
            platform = reader.platform;
            serializedType = reader.serializedType;
            byteSize = reader.byteSize;

            if (platform == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
            }
        }

        public string Dump()
        {
            if (serializedType?.m_Type != null)
            {
                return TypeTreeHelper.ReadTypeString(serializedType.m_Type, reader);
            }
            return null;
        }

        public string Dump(TypeTree m_Type)
        {
            if (m_Type != null)
            {
                return TypeTreeHelper.ReadTypeString(m_Type, reader);
            }
            return null;
        }

        public OrderedDictionary ToType()
        {
            if (serializedType?.m_Type != null)
            {
                return TypeTreeHelper.ReadType(serializedType.m_Type, reader);
            }
            return null;
        }

        public OrderedDictionary ToType(TypeTree m_Type)
        {
            if (m_Type != null)
            {
                return TypeTreeHelper.ReadType(m_Type, reader);
            }
            return null;
        }

        public byte[] GetRawData()
        {
            reader.Reset();
            return reader.ReadBytes((int)byteSize);
        }
    }
}
