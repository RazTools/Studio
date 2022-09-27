using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SevenZip;
using System.Collections.Generic;

namespace AssetStudio
{
    public class UnityTexEnv
    {
        public PPtr<Texture> m_Texture;
        public Vector2 m_Scale;
        public Vector2 m_Offset;

        public UnityTexEnv(ObjectReader reader)
        {
            m_Texture = new PPtr<Texture>(reader);
            m_Scale = reader.ReadVector2();
            m_Offset = reader.ReadVector2();
        }
    }

    public class UnityPropertySheet
    {
        private const string HDRPostfixName = "_HDR";
        private const string STPostfixName = "_ST";
        private const string TexelSizePostfixName = "_TexelSize";

        public Dictionary<string, UnityTexEnv> m_TexEnvs;
        public Dictionary<string, int> m_Ints;
        public Dictionary<string, float> m_Floats;
        public Dictionary<string, Color> m_Colors;

        public UnityPropertySheet(ObjectReader reader)
        {
            var version = reader.version;

            int m_TexEnvsSize = reader.ReadInt32();
            m_TexEnvs = new Dictionary<string, UnityTexEnv>(m_TexEnvsSize);
            for (int i = 0; i < m_TexEnvsSize; i++)
            {
                m_TexEnvs.Add(reader.ReadAlignedString(), new UnityTexEnv(reader));
            }

            if (version[0] >= 2021) //2021.1 and up
            {
                int m_IntsSize = reader.ReadInt32();
                m_Ints = new Dictionary<string, int>(m_IntsSize);
                for (int i = 0; i < m_IntsSize; i++)
                {
                    m_Ints.Add(reader.ReadAlignedString(), reader.ReadInt32());
                }
            }

            int m_FloatsSize = reader.ReadInt32();
            m_Floats = new Dictionary<string, float>(m_FloatsSize);
            for (int i = 0; i < m_FloatsSize; i++)
            {
                m_Floats.Add(reader.ReadAlignedString(), reader.ReadSingle());
            }

            int m_ColorsSize = reader.ReadInt32();
            m_Colors = new Dictionary<string, Color>(m_ColorsSize);
            for (int i = 0; i < m_ColorsSize; i++)
            {
                m_Colors.Add(reader.ReadAlignedString(), reader.ReadColor4());
            }
        }

        public string FindPropertyNameByCRC28(uint crc)
        {
            foreach (var property in m_TexEnvs.Keys)
            {
                string hdrName = property + HDRPostfixName;
                if (CRC.Verify28DigestUTF8(hdrName, crc))
                {
                    return hdrName;
                }
                string stName = property + STPostfixName;
                if (CRC.Verify28DigestUTF8(stName, crc))
                {
                    return stName;
                }
                string texelName = property + TexelSizePostfixName;
                if (CRC.Verify28DigestUTF8(texelName, crc))
                {
                    return texelName;
                }
            }
            foreach (var property in m_Floats.Keys)
            {
                if (CRC.Verify28DigestUTF8(property, crc))
                {
                    return property;
                }
            }
            foreach (var property in m_Colors.Keys)
            {
                if (CRC.Verify28DigestUTF8(property, crc))
                {
                    return property;
                }
            }
            return null;
        }
    }

    public sealed class Material : NamedObject
    {
        public PPtr<Shader> m_Shader;
        public UnityPropertySheet m_SavedProperties;
        public Dictionary<string, string> m_StringTagMap;

        public Material(ObjectReader reader) : base(reader)
        {
            m_Shader = new PPtr<Shader>(reader);

            if (version[0] == 4 && version[1] >= 1) //4.x
            {
                var m_ShaderKeywords = reader.ReadStringArray();
            }

            if (version[0] >= 5) //5.0 and up
            {
                var m_ShaderKeywords = reader.ReadAlignedString();
                var m_LightmapFlags = reader.ReadUInt32();
            }

            if (version[0] > 5 || (version[0] == 5 && version[1] >= 6)) //5.6 and up
            {
                var m_EnableInstancingVariants = reader.ReadBoolean();
                var m_DoubleSidedGI = reader.ReadBoolean(); //2017 and up
                reader.AlignStream();
            }

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                var m_CustomRenderQueue = reader.ReadInt32();
            }

            if (version[0] > 5 || (version[0] == 5 && version[1] >= 1)) //5.1 and up
            {
                var stringTagMapSize = reader.ReadInt32();
                m_StringTagMap = new Dictionary<string, string>(stringTagMapSize);
                for (int i = 0; i < stringTagMapSize; i++)
                {
                    var first = reader.ReadAlignedString();
                    var second = reader.ReadAlignedString();
                    m_StringTagMap.Add(first, second);
                }
            }

            if (version[0] > 5 || (version[0] == 5 && version[1] >= 6)) //5.6 and up
            {
                var disabledShaderPasses = reader.ReadStringArray();
            }

            m_SavedProperties = new UnityPropertySheet(reader);

            //vector m_BuildTextureStacks 2020 and up
        }

        public string FindPropertyNameByCRC28(uint crc)
        {
            return m_SavedProperties.FindPropertyNameByCRC28(crc);
        }
    }
}
