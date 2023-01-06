using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public PPtr<Object>[] m_PreloadTable;
        public KeyValuePair<string, AssetInfo>[] m_Container;
        //public AssetInfo m_MainAsset;
        //public uint m_RuntimeComaptability;
        //public string m_AssetBundleName;
        //public string[] m_Dependencies;
        //public bool m_IsStreamedSceneAssetBundle;
        //public int m_ExplicitDataLayout;
        //public int m_PathFlags;
        //public KeyValuePair<string, string>[] m_SceneHashes;

        public AssetBundle(ObjectReader reader) : base(reader)
        {
            var m_PreloadTableSize = reader.ReadInt32();
            m_PreloadTable = new PPtr<Object>[m_PreloadTableSize];
            for (int i = 0; i < m_PreloadTableSize; i++)
            {
                m_PreloadTable[i] = new PPtr<Object>(reader);
            }

            var m_ContainerSize = reader.ReadInt32();
            m_Container = new KeyValuePair<string, AssetInfo>[m_ContainerSize];
            for (int i = 0; i < m_ContainerSize; i++)
            {
                m_Container[i] = new KeyValuePair<string, AssetInfo>(reader.ReadAlignedString(), new AssetInfo(reader));
            }

            //if (reader.Game.Type.IsMhyGroup())
            //{
            //    m_MainAsset = new AssetInfo(reader);
            //    m_RuntimeComaptability = reader.ReadUInt32();
            //    m_AssetBundleName = reader.ReadAlignedString();
            //    var dependencyCount = reader.ReadInt32();
            //    m_Dependencies = new string[dependencyCount];
            //    for (int k = 0; k < dependencyCount; k++)
            //    {
            //        m_Dependencies[k] = reader.ReadAlignedString();
            //    }
            //    if (reader.Game.Type.IsGIGroup())
            //    {
            //        m_IsStreamedSceneAssetBundle = reader.ReadBoolean();
            //        reader.AlignStream();
            //        if (reader.Game.Type.IsGICB3() || reader.Game.Type.IsGI())
            //        {
            //            m_ExplicitDataLayout = reader.ReadInt32();
            //        }
            //        m_PathFlags = reader.ReadInt32();
            //        if (reader.Game.Type.IsGI())
            //        {
            //            var sceneHashCount = reader.ReadInt32();
            //            m_SceneHashes = new KeyValuePair<string, string>[sceneHashCount];
            //            for (int l = 0; l < sceneHashCount; l++)
            //            {
            //                m_SceneHashes[l] = new KeyValuePair<string, string>(reader.ReadAlignedString(), reader.ReadAlignedString());
            //            }
            //        }
            //    }
            //}
        }
    }
}
