using AssetStudio;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public sealed class GameObject : EditorExtension
    {
        public List<PPtr<Component>> m_Components;
        public string m_Name;

        public Transform m_Transform;
        public MeshRenderer m_MeshRenderer;
        public MeshFilter m_MeshFilter;
        public SkinnedMeshRenderer m_SkinnedMeshRenderer;
        public Animator m_Animator;
        public Animation m_Animation;

        public override string Name => m_Name;
        public GameObject(ObjectReader reader) : base(reader)
        {
            int m_Component_size = reader.ReadInt32();
            m_Components = new List<PPtr<Component>>();
            for (int i = 0; i < m_Component_size; i++)
            {
                if ((version[0] == 5 && version[1] < 5) || version[0] < 5) //5.5 down
                {
                    int first = reader.ReadInt32();
                }
                m_Components.Add(new PPtr<Component>(reader));
            }

            var m_Layer = reader.ReadInt32();
            m_Name = reader.ReadAlignedString();
        }

        public bool HasModel() => HasMesh(m_Transform, new List<bool>());
        private static bool HasMesh(Transform m_Transform, List<bool> meshes)
        {
            try
            {
                m_Transform.m_GameObject.TryGet(out var m_GameObject);

                if (m_GameObject.m_MeshRenderer != null)
                {
                    var mesh = GetMesh(m_GameObject.m_MeshRenderer);
                    meshes.Add(mesh != null);
                }

                if (m_GameObject.m_SkinnedMeshRenderer != null)
                {
                    var mesh = GetMesh(m_GameObject.m_SkinnedMeshRenderer);
                    meshes.Add(mesh != null);
                }

                foreach (var pptr in m_Transform.m_Children)
                {
                    if (pptr.TryGet(out var child))
                        meshes.Add(HasMesh(child, meshes));
                }

                return meshes.Any(x => x == true);
            }
            catch(Exception e)
            {
                Logger.Warning($"Unable to verify if {m_Transform?.Name} has meshes, skipping...");
                return false;
            }
        }

        private static Mesh GetMesh(Renderer meshR)
        {
            if (meshR is SkinnedMeshRenderer sMesh)
            {
                if (sMesh.m_Mesh.TryGet(out var m_Mesh))
                {
                    return m_Mesh;
                }
            }
            else
            {
                meshR.m_GameObject.TryGet(out var m_GameObject);
                if (m_GameObject.m_MeshFilter != null)
                {
                    if (m_GameObject.m_MeshFilter.m_Mesh.TryGet(out var m_Mesh))
                    {
                        return m_Mesh;
                    }
                }
            }

            return null;
        }
    }
}
