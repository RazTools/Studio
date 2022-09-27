using SevenZip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public sealed class GameObject : EditorExtension
    {
        public PPtr<Component>[] m_Components;
        public string m_Name;
        public uint m_Tag;
        public bool m_IsActive;

        public Transform m_Transform;
        public MeshRenderer m_MeshRenderer;
        public MeshFilter m_MeshFilter;
        public SkinnedMeshRenderer m_SkinnedMeshRenderer;
        public Animator m_Animator;
        public Animation m_Animation;

        public GameObject(ObjectReader reader) : base(reader)
        {
            int m_Component_size = reader.ReadInt32();
            m_Components = new PPtr<Component>[m_Component_size];
            for (int i = 0; i < m_Component_size; i++)
            {
                if ((version[0] == 5 && version[1] < 5) || version[0] < 5) //5.5 down
                {
                    int first = reader.ReadInt32();
                }
                m_Components[i] = new PPtr<Component>(reader);
            }

            var m_Layer = reader.ReadInt32();
            m_Name = reader.ReadAlignedString();
            m_Tag = reader.ReadUInt16();
            m_IsActive = reader.ReadBoolean();
        }
        public Transform GetTransform()
        {
            foreach (PPtr<Component> ptr in FetchComponents())
            {
                if (!ptr.TryGet(out var comp))
                {
                    continue;
                }

                if (comp.type == ClassIDType.Transform)
                {
                    return comp as Transform;
                }
            }
            throw new Exception("Can't find transform component");
        }

        private List<PPtr<Component>> FetchComponents()
        {
            return m_Components.ToList();
        }

        public T FindComponent<T>()
            where T : Component
        {
            foreach (PPtr<Component> ptr in FetchComponents())
            {
                // component could has not impelemented asset type
                if (ptr.TryGet(out var comp) && comp is T t) 
                {
                    return t;
                }
            }
            return null;
        }

        public Dictionary<uint, string> BuildTOS()
        {
            Dictionary<uint, string> tos = new Dictionary<uint, string>() { { 0, string.Empty } };
            BuildTOS(this, string.Empty, tos);
            return tos;
        }
        private void BuildTOS(GameObject parent, string parentPath, Dictionary<uint, string> tos)
        {
            Transform transform = parent.GetTransform();
            foreach (PPtr<Transform> childPtr in transform.m_Children)
            {
                if (childPtr.TryGet(out var childTransform))
                {
                    if (childTransform.m_GameObject.TryGet(out var child))
                    {
                        string path = parentPath != string.Empty ? parentPath + '/' + child.m_Name : child.m_Name;
                        var pathHash = CRC.CalculateDigestUTF8(path);
                        tos[pathHash] = path;
                        BuildTOS(child, path, tos);
                    }
                }
            }
        }
    }
}
