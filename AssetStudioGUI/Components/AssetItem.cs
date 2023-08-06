using System.Windows.Forms;
using AssetStudio;

namespace AssetStudioGUI
{
    public class AssetItem : ListViewItem
    {
        public ObjectInfo ObjInfo;
        public SerializedFile SourceFile;
        public string TypeString;
        public long m_PathID;
        public long FullSize;
        public ClassIDType Type;
        public string InfoText;
        public string UniqueID;
        public GameObjectTreeNode TreeNode;

        public Object Asset => SourceFile.ReadObject(Type, m_PathID, false);

        public string Container
        {
            get => ObjInfo.container;
            set => ObjInfo.container = value;
        }

        public AssetItem(ObjectInfo objInfo, SerializedFile assetsFile)
        {
            ObjInfo = objInfo;
            SourceFile = assetsFile;
            Type = objInfo.type;
            TypeString = Type.ToString();
            m_PathID = objInfo.m_PathID;
            FullSize = objInfo.byteSize;
        }

        public void SetSubItems()
        {
            SubItems.AddRange(new[]
            {
                Container, //Container
                TypeString, //Type
                m_PathID.ToString(), //PathID
                FullSize.ToString(), //Size
            });
        }
    }
}
