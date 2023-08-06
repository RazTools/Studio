using System;

namespace AssetStudio
{
    public class ObjectInfo
    {
        public string name;
        public string container = string.Empty;
        public long byteStart;
        public uint byteSize;
        public int typeID;
        public int classID;
        public ushort isDestroyed;
        public byte stripped;

        public long m_PathID;
        public SerializedType serializedType;

        public ClassIDType type => Enum.IsDefined(typeof(ClassIDType), classID) ? (ClassIDType)classID : ClassIDType.UnknownType;
    }
}
