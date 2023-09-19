using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class SerializedType
    {
        public int classID;
        public bool m_IsStrippedType;
        public short m_ScriptTypeIndex = -1;
        public TypeTree m_Type;
        public byte[] m_ScriptID; //Hash128
        public byte[] m_OldTypeHash; //Hash128
        public int[] m_TypeDependencies;
        public string m_KlassName;
        public string m_NameSpace;
        public string m_AsmName;

        public bool Match(params string[] hashes) => hashes.Any(x => x == Convert.ToHexString(m_OldTypeHash));
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"classID: {classID} | ");
            sb.Append($"IsStrippedType: {m_IsStrippedType} | ");
            sb.Append($"ScriptTypeIndex: {m_ScriptTypeIndex} | ");
            sb.Append($"KlassName: {m_KlassName} | ");
            sb.Append($"NameSpace: {m_NameSpace} | ");
            sb.Append($"AsmName: {m_AsmName}");
            return sb.ToString();
        }
    }
}
