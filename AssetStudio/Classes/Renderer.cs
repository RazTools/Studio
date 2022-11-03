using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class StaticBatchInfo
    {
        public ushort firstSubMesh;
        public ushort subMeshCount;

        public StaticBatchInfo(ObjectReader reader)
        {
            firstSubMesh = reader.ReadUInt16();
            subMeshCount = reader.ReadUInt16();
        }
    }

    public abstract class Renderer : Component
    {
        public static bool Parsable;
        public PPtr<Material>[] m_Materials;
        public StaticBatchInfo m_StaticBatchInfo;
        public uint[] m_SubsetIndices;
        private bool isNewHeader = false;
        protected Renderer(ObjectReader reader) : base(reader)
        {
            if (version[0] < 5) //5.0 down
            {
                var m_Enabled = reader.ReadBoolean();
                var m_CastShadows = reader.ReadBoolean();
                var m_ReceiveShadows = reader.ReadBoolean();
                var m_LightmapIndex = reader.ReadByte();
            }
            else //5.0 and up
            {
                if (version[0] > 5 || (version[0] == 5 && version[1] >= 4)) //5.4 and up
                {
                    if (reader.Game.Name == "GI")
                    {
                        CheckHeader(reader);
                    }
                    var m_Enabled = reader.ReadBoolean();
                    var m_CastShadows = reader.ReadByte();
                    var m_ReceiveShadows = reader.ReadByte();
                    if (version[0] > 2017 || (version[0] == 2017 && version[1] >= 2)) //2017.2 and up
                    {
                        var m_DynamicOccludee = reader.ReadByte();
                        if (reader.Game.Name == "BH3")
                        {
                            var m_AllowHalfResolution = reader.ReadByte();
                        }
                        else if (reader.Game.Name == "GI_CB3")
                        {
                            var m_ReceiveDecals = reader.ReadByte();
                            var m_EnableShadowCulling = reader.ReadByte();
                            var m_EnableGpuQuery = reader.ReadByte();
                            var m_AllowHalfResolution = reader.ReadByte();
                            var m_IsRainOccluder = reader.ReadByte();
                            var m_IsDynamicAOOccluder = reader.ReadByte();
                            var m_IsDynamic = reader.ReadByte();
                        }
                        else if (reader.Game.Name == "GI")
                        {
                            var m_ReceiveDecals = reader.ReadByte();
                            var m_EnableShadowCulling = reader.ReadByte();
                            var m_EnableGpuQuery = reader.ReadByte();
                            var m_AllowHalfResolution = reader.ReadByte();
                            var m_IsRainOccluder = reader.ReadByte();
                            var m_IsDynamicAOOccluder = reader.ReadByte();
                            var m_IsCloudObject = reader.ReadByte();
                            var m_IsInteriorVolume = reader.ReadByte();
                            var m_IsDynamic = reader.ReadByte();
                            var m_UseTessellation = reader.ReadByte();
                            var m_IsTerrainTessInfo = reader.ReadByte();
                            if (isNewHeader)
                            {
                                var m_UseVertexLightInForward = reader.ReadByte();
                                var m_CombineSubMeshInGeoPass = reader.ReadByte();
                                var m_AllowPerMaterialProp = reader.ReadByte();
                                var m_IsHQDynamicAOOccluder = reader.ReadByte();
                            }
                        }
                    }
                    if (version[0] >= 2021) //2021.1 and up
                    {
                        var m_StaticShadowCaster = reader.ReadByte();
                    }
                    if (reader.Game.Name == "GI_CB2")
                    {
                        var m_ReceiveDecals = reader.ReadByte();
                        var m_EnableShadowCulling = reader.ReadByte();
                        var m_EnableGpuQuery = reader.ReadByte();
                        var m_AllowHalfResolution = reader.ReadByte();
                        var m_IsRainOccluder = reader.ReadByte();
                        var m_IsDynamic = reader.ReadByte();
                    }
                    if (reader.Game.Name == "GI_CB1")
                    {
                        var m_ReceiveDecals = reader.ReadByte();
                        var m_EnableShadowCulling = reader.ReadByte();
                        var m_EnableGpuQuery = reader.ReadByte();
                        var m_AllowHalfResolution = reader.ReadByte();
                    }
                    var m_MotionVectors = reader.ReadByte();
                    var m_LightProbeUsage = reader.ReadByte();
                    var m_ReflectionProbeUsage = reader.ReadByte();
                    if (version[0] > 2019 || (version[0] == 2019 && version[1] >= 3)) //2019.3 and up
                    {
                        var m_RayTracingMode = reader.ReadByte();
                    }
                    if (version[0] >= 2020) //2020.1 and up
                    {
                        var m_RayTraceProcedural = reader.ReadByte();
                    }
                    if (reader.Game.Name == "GI" || reader.Game.Name == "GI_CB3")
                    {
                        var m_MeshShowQuality = reader.ReadByte();
                    }
                    reader.AlignStream();
                }
                else
                {
                    var m_Enabled = reader.ReadBoolean();
                    reader.AlignStream();
                    var m_CastShadows = reader.ReadByte();
                    var m_ReceiveShadows = reader.ReadBoolean();
                    reader.AlignStream();
                }

                if (version[0] >= 2018) //2018 and up
                {
                    var m_RenderingLayerMask = reader.ReadUInt32();
                }

                if (version[0] > 2018 || (version[0] == 2018 && version[1] >= 3)) //2018.3 and up
                {
                    var m_RendererPriority = reader.ReadInt32();
                }

                var m_LightmapIndex = reader.ReadInt16();
                var m_LightmapIndexDynamic = reader.ReadInt16();
                if (reader.Game.Name == "GI" || reader.Game.Name == "GI_CB1" || reader.Game.Name == "GI_CB2" || reader.Game.Name == "GI_CB3")
                {
                    if (m_LightmapIndex != -1 || m_LightmapIndexDynamic != -1)
                        throw new Exception("Not Supported !! skipping....");
                }
            }

            if (version[0] >= 3) //3.0 and up
            {
                var m_LightmapTilingOffset = reader.ReadVector4();
            }

            if (version[0] >= 5) //5.0 and up
            {
                var m_LightmapTilingOffsetDynamic = reader.ReadVector4();
            }

            if (reader.Game.Name == "GI" || reader.Game.Name == "GI_CB1" || reader.Game.Name == "GI_CB2" || reader.Game.Name == "GI_CB3")
            {
                var m_ViewDistanceRatio = reader.ReadSingle();
                var m_ShaderLODDistanceRatio = reader.ReadSingle();
            }
            var m_MaterialsSize = reader.ReadInt32();
            m_Materials = new PPtr<Material>[m_MaterialsSize];
            for (int i = 0; i < m_MaterialsSize; i++)
            {
                m_Materials[i] = new PPtr<Material>(reader);
            }

            if (version[0] < 3) //3.0 down
            {
                var m_LightmapTilingOffset = reader.ReadVector4();
            }
            else //3.0 and up
            {
                if (version[0] > 5 || (version[0] == 5 && version[1] >= 5)) //5.5 and up
                {
                    m_StaticBatchInfo = new StaticBatchInfo(reader);
                }
                else
                {
                    m_SubsetIndices = reader.ReadUInt32Array();
                }

                var m_StaticBatchRoot = new PPtr<Transform>(reader);
            }
            if (reader.Game.Name == "GI" || reader.Game.Name == "GI_CB1" || reader.Game.Name == "GI_CB2" || reader.Game.Name == "GI_CB3")
            {
                var m_MatLayers = reader.ReadInt32();
            }

            if (version[0] > 5 || (version[0] == 5 && version[1] >= 4)) //5.4 and up
            {
                var m_ProbeAnchor = new PPtr<Transform>(reader);
                var m_LightProbeVolumeOverride = new PPtr<GameObject>(reader);
            }
            else if (version[0] > 3 || (version[0] == 3 && version[1] >= 5)) //3.5 - 5.3
            {
                var m_UseLightProbes = reader.ReadBoolean();
                reader.AlignStream();

                if (version[0] >= 5)//5.0 and up
                {
                    var m_ReflectionProbeUsage = reader.ReadInt32();
                }

                var m_LightProbeAnchor = new PPtr<Transform>(reader); //5.0 and up m_ProbeAnchor
            }

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                if (version[0] == 4 && version[1] == 3) //4.3
                {
                    var m_SortingLayer = reader.ReadInt16();
                }
                else
                {
                    var m_SortingLayerID = reader.ReadInt32();
                    var m_SortingLayer = reader.ReadInt16();
                }

                //SInt16 m_SortingLayer 5.6 and up
                var m_SortingOrder = reader.ReadInt16();
                reader.AlignStream();
                if (reader.Game.Name == "GI" || reader.Game.Name == "GI_CB1" || reader.Game.Name == "GI_CB2" || reader.Game.Name == "GI_CB3" || reader.Game.Name == "BH3")
                {
                    var m_UseHighestMip = reader.ReadBoolean();
                    reader.AlignStream();
                }
                if (reader.Game.Name == "SR_CB3")
                {
                    var _RenderFlag = reader.ReadUInt32();
                    reader.AlignStream();
                }
            }
        }

        private void CheckHeader(ObjectReader reader)
        {
            short index = 0;
            var pos = reader.Position;
            while (index != -1 && reader.Position <= pos + 0x1A)
                index = reader.ReadInt16();
            isNewHeader = (reader.Position - pos) == 0x1A;
            reader.Position = pos;
        }

        public string FindMaterialPropertyNameByCRC28(uint crc)
        {
            foreach (PPtr<Material> materialPtr in m_Materials)
            {
                if (!materialPtr.TryGet(out var material))
                {
                    continue;
                }
                string property = material.FindPropertyNameByCRC28(crc);
                if (property == null)
                {
                    continue;
                }

                return property;
            }
            return null;
        }
    }
}
