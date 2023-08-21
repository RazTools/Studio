using System.Text;

namespace AssetStudio
{
    public class ObjectInfo
    {
        public long byteStart;
        public uint byteSize;
        public int typeID;
        public int classID;
        public ushort isDestroyed;
        public byte stripped;

        public long m_PathID;
        public SerializedType serializedType;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"byteStart: 0x{byteStart:X8} | ");
            sb.Append($"byteSize: 0x{byteSize:X8} | ");
            sb.Append($"typeID: {typeID} | ");
            sb.Append($"classID: {classID} | ");
            sb.Append($"isDestroyed: {isDestroyed} | ");
            sb.Append($"stripped: {stripped} | ");
            sb.Append($"PathID: {m_PathID}");
            return sb.ToString();
        }
    }
}
