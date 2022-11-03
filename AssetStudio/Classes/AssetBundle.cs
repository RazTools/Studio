using Newtonsoft.Json;
using System.Collections.Generic;

namespace AssetStudio
{
    public class AssetInfo
    {
        public int preloadIndex;
        public int preloadSize;
        public PPtr<Object> asset;

        public AssetInfo(ObjectReader reader)
        {
            preloadIndex = reader.ReadInt32();
            preloadSize = reader.ReadInt32();
            asset = new PPtr<Object>(reader);
        }
    }

    public sealed class AssetBundle : NamedObject
    {
        public static bool Exportable;

        public PPtr<Object>[] PreloadTable;
        public KeyValuePair<string, AssetInfo>[] Container;
        public AssetInfo MainAsset;
        public uint RuntimeComaptability;
        public string AssetBundleName;
        public int DependencyCount;
        public string[] Dependencies;
        public bool IsStreamedScenessetBundle;
        public int ExplicitDataLayout;
        public int PathFlags;
        public int SceneHashCount;
        public KeyValuePair<string, string>[] SceneHashes;

        public AssetBundle(ObjectReader reader) : base(reader)
        {
            var m_PreloadTableSize = reader.ReadInt32();
            PreloadTable = new PPtr<Object>[m_PreloadTableSize];
            for (int i = 0; i < m_PreloadTableSize; i++)
            {
                PreloadTable[i] = new PPtr<Object>(reader);
            }

            var m_ContainerSize = reader.ReadInt32();
            Container = new KeyValuePair<string, AssetInfo>[m_ContainerSize];
            for (int i = 0; i < m_ContainerSize; i++)
            {
                Container[i] = new KeyValuePair<string, AssetInfo>(reader.ReadAlignedString(), new AssetInfo(reader));
            }

            if (reader.Game.Name == "GI" || reader.Game.Name == "GI_CB1" || reader.Game.Name == "GI_CB2" || reader.Game.Name == "GI_CB3")
            {
                MainAsset = new AssetInfo(reader);
                RuntimeComaptability = reader.ReadUInt32();
            }

            AssetBundleName = reader.ReadAlignedString();
            DependencyCount = reader.ReadInt32();
            Dependencies = new string[DependencyCount];
            for (int k = 0; k < DependencyCount; k++)
            {
                Dependencies[k] = reader.ReadAlignedString();
            }
            if (reader.Game.Name == "GI_CB1" || reader.Game.Name == "GI_CB2")
            {
                IsStreamedScenessetBundle = reader.ReadBoolean();
                reader.AlignStream();
                PathFlags = reader.ReadInt32();
            }
            else if (reader.Game.Name == "GI_CB3")
            {
                IsStreamedScenessetBundle = reader.ReadBoolean();
                reader.AlignStream();
                ExplicitDataLayout = reader.ReadInt32();
                PathFlags = reader.ReadInt32();
            }
            else if (reader.Game.Name == "GI")
            {
                IsStreamedScenessetBundle = reader.ReadBoolean();
                reader.AlignStream();
                ExplicitDataLayout = reader.ReadInt32();
                PathFlags = reader.ReadInt32();
                SceneHashCount = reader.ReadInt32();
                SceneHashes = new KeyValuePair<string, string>[SceneHashCount];
                for (int l = 0; l < SceneHashCount; l++)
                {
                    SceneHashes[l] = new KeyValuePair<string, string>(reader.ReadAlignedString(), reader.ReadAlignedString());
                }
            }
        }
    }
}
