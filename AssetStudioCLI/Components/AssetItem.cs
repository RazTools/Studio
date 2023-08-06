using AssetStudio;
using static System.Net.Mime.MediaTypeNames;

namespace AssetStudioCLI
{
    public class AssetItem
    {
        public string Text;
        public ObjectInfo ObjInfo;
        public SerializedFile SourceFile;
        public string TypeString;
        public long m_PathID;
        public long FullSize;
        public ClassIDType Type;
        public string InfoText;
        public string UniqueID;

        public Object Asset => SourceFile.ReadObject(Type, m_PathID, false);

        public string Container
        {
            get => ObjInfo.container;
            set => ObjInfo.container = value;
        }

        public AssetItem(ObjectInfo objInfo, SerializedFile assetsFile)
        {
            Text = "";
            ObjInfo = objInfo;
            SourceFile = assetsFile;
            Type = objInfo.type;
            TypeString = Type.ToString();
            m_PathID = objInfo.m_PathID;
            FullSize = objInfo.byteSize;
        }
    }
}
