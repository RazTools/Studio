using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class TypeTreeNode
    {
        public string m_Type;
        public string m_Name;
        public int m_ByteSize;
        public int m_Index;
        public int m_TypeFlags; //m_IsArray
        public int m_Version;
        public int m_MetaFlag;
        public int m_Level;
        public uint m_TypeStrOffset;
        public uint m_NameStrOffset;
        public ulong m_RefTypeHash;

        public TypeTreeNode() { }

        public TypeTreeNode(string type, string name, int level, bool align)
        {
            m_Type = type;
            m_Name = name;
            m_Level = level;
            m_MetaFlag = align ? 0x4000 : 0;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"Type: {m_Type} | ");
            sb.Append($"Name: {m_Name} | ");
            sb.Append($"ByteSize: 0x{m_ByteSize:X8} | ");
            sb.Append($"Index: {m_Index} | ");
            sb.Append($"TypeFlags: {m_TypeFlags} | ");
            sb.Append($"Version: {m_Version} | ");
            sb.Append($"TypeStrOffset: 0x{m_TypeStrOffset:X8} | ");
            sb.Append($"NameStrOffset: 0x{m_NameStrOffset:X8}");
            return sb.ToString();
        }
    }
}
