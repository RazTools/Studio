using System.Collections.Generic;
using System.Linq;

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

        public static List<ObjectInfo> Filter(List<ObjectInfo> objects) => objects.Where(x => x.IsExportableType()).OrderBy(x => ExportableTypes.IndexOf((ClassIDType)x.typeID)).ToList();

        private bool IsExportableType()
        {
            var typeID = (ClassIDType)classID;
            var isExportableType = ExportableTypes.Contains(typeID);
            return typeID switch
            {
                ClassIDType.IndexObject or ClassIDType.MiHoYoBinData => isExportableType && MiHoYoBinData.Exportable,
                _ => isExportableType,
            };
        }

        private readonly static List<ClassIDType> ExportableTypes = new List<ClassIDType> { ClassIDType.GameObject, ClassIDType.IndexObject, ClassIDType.Material, ClassIDType.Texture2D, ClassIDType.Mesh, ClassIDType.Shader, ClassIDType.TextAsset, ClassIDType.AnimationClip, ClassIDType.Font, ClassIDType.Sprite, ClassIDType.Animator, ClassIDType.MiHoYoBinData, ClassIDType.AssetBundle };
    }
}
