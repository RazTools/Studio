using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class SerializedFileHeader
    {
        public uint m_MetadataSize;
        public long m_FileSize;
        public SerializedFileFormatVersion m_Version;
        public long m_DataOffset;
        public byte m_Endianess;
        public byte[] m_Reserved;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"MetadataSize: 0x{m_MetadataSize:X8} | ");
            sb.Append($"FileSize: 0x{m_FileSize:X8} | ");
            sb.Append($"Version: {m_Version} | ");
            sb.Append($"DataOffset: 0x{m_DataOffset:X8} | ");
            sb.Append($"Endianness: {(EndianType)m_Endianess}");
            return sb.ToString();
        }
    }
}
