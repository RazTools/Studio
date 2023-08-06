using System.Windows.Forms;
using AssetStudio;

namespace AssetStudioGUI
{
    public class GameObjectTreeNode : TreeNode
    {
        private SerializedFile assetsFile;
        private ObjectInfo objInfo;
        public GameObject gameObject
        {
            get
            {
                var gameObject = assetsFile.ReadObject(objInfo) as GameObject;
                return gameObject;
            }
        }

        public GameObjectTreeNode(ObjectInfo objInfo, SerializedFile assetsFile)
        {
            this.objInfo = objInfo;
            this.assetsFile = assetsFile;
            Text = this.assetsFile.ReadObjectName(this.objInfo);
        }
    }
}
