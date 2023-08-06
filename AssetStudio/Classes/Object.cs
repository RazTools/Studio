using Newtonsoft.Json;
using System;
using System.Collections.Specialized;

namespace AssetStudio
{
    public class Object
    {
        [JsonIgnore]
        public SerializedFile assetsFile;
        [JsonIgnore]
        public ObjectInfo objInfo;
        [JsonIgnore]
        public int[] version;
        [JsonIgnore]
        protected BuildType buildType;
        [JsonIgnore]
        public BuildTarget platform;

        [JsonIgnore]
        public ObjectReader Reader => new ObjectReader(assetsFile.Reader, assetsFile, objInfo, assetsFile.game);
        [JsonIgnore]
        public long m_PathID => objInfo.m_PathID;
        [JsonIgnore]
        public ClassIDType type => objInfo.type;
        [JsonIgnore]
        public SerializedType serializedType => objInfo.serializedType;
        [JsonIgnore]
        public uint byteSize => objInfo.byteSize;

        public Object(ObjectReader reader)
        {
            reader.Reset();
            objInfo = reader.objInfo;
            assetsFile = reader.assetsFile;
            version = reader.version;
            buildType = reader.buildType;
            platform = reader.platform;

            if (platform == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
            }
        }

        public string Dump()
        {
            if (serializedType?.m_Type != null)
            {
                return TypeTreeHelper.ReadTypeString(serializedType.m_Type, Reader);
            }
            return null;
        }

        public string Dump(TypeTree m_Type)
        {
            if (m_Type != null)
            {
                return TypeTreeHelper.ReadTypeString(m_Type, Reader);
            }
            return null;
        }

        public OrderedDictionary ToType()
        {
            if (serializedType?.m_Type != null)
            {
                return TypeTreeHelper.ReadType(serializedType.m_Type, Reader);
            }
            return null;
        }

        public OrderedDictionary ToType(TypeTree m_Type)
        {
            if (m_Type != null)
            {
                return TypeTreeHelper.ReadType(m_Type, Reader);
            }
            return null;
        }

        public byte[] GetRawData()
        {
            var reader = Reader;
            reader.Reset();
            return reader.ReadBytes((int)byteSize);
        }
    }
}
