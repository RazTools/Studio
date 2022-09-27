using System;
using System.Collections.Generic;
using System.Linq;
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

        public bool HasExportableType()
        {
            var typeID = (ClassIDType)classID;
            var isExportableType = ExportableTypes.Contains(typeID);
            switch (typeID)
            {
                case ClassIDType.IndexObject:
                case ClassIDType.MiHoYoBinData:
                    return isExportableType && IndexObject.Exportable;
                default:
                    return isExportableType;
            }
        }

        public static ClassIDType[] ExportableTypes = new ClassIDType[]
        {
            ClassIDType.GameObject,
            ClassIDType.Material,
            ClassIDType.Texture2D,
            ClassIDType.Mesh,
            ClassIDType.Shader,
            ClassIDType.TextAsset,
            ClassIDType.AnimationClip,
            ClassIDType.Animator,
            ClassIDType.Font,
            ClassIDType.AssetBundle,
            ClassIDType.Sprite,
            ClassIDType.MiHoYoBinData,
            ClassIDType.IndexObject
        };
    }
}
