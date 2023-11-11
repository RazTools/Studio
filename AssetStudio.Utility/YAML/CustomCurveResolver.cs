using System;
using SevenZip;
using System.Linq;
using System.Xml.Linq;

namespace AssetStudio
{
    public sealed class CustomCurveResolver
    {
        public CustomCurveResolver(AnimationClip clip)
        {
            if (clip == null)
            {
                throw new ArgumentNullException(nameof(clip));
            }
            m_clip = clip;
        }

        public string ToAttributeName(BindingCustomType type, uint attribute, string path)
        {
            switch (type)
            {
                case BindingCustomType.BlendShape:
                    {
                        const string Prefix = "blendShape.";
                        if (AnimationClipConverter.UnknownPathRegex.IsMatch(path))
                        {
                            return Prefix + attribute;
                        }

                        foreach (GameObject root in Roots)
                        {
                            var rootTransform = root.m_Transform;
                            var child = rootTransform.FindChild(path);
                            if (child == null)
                            {
                                continue;
                            }
                            SkinnedMeshRenderer skin = null;
                            if (child.m_GameObject.TryGet(out var go))
                            {
                                skin = go.m_SkinnedMeshRenderer;
                            }
                            if (skin == null)
                            {
                                continue;
                            }
                            if (!skin.m_Mesh.TryGet(out var mesh))
                            {
                                continue;
                            }
                            string shapeName = mesh.FindBlendShapeNameByCRC(attribute);
                            if (shapeName == null)
                            {
                                continue;
                            }

                            return Prefix + shapeName;
                        }
                        return Prefix + attribute;
                    }

                case BindingCustomType.Renderer:
                    return "m_Materials." + CommonString.StringBuffer[0x31] + "." + CommonString.StringBuffer[0x6A] + $"[{attribute}]";

                case BindingCustomType.RendererMaterial:
                    {
                        const string Prefix = "material.";
                        if (AnimationClipConverter.UnknownPathRegex.IsMatch(path))
                        {
                            return Prefix + attribute;
                        }

                        foreach (GameObject root in Roots)
                        {
                            Transform rootTransform = root.m_Transform;
                            Transform child = rootTransform.FindChild(path);
                            if (child == null)
                            {
                                continue;
                            }

                            uint crc28 = attribute & 0xFFFFFFF;
                            Renderer renderer = null;
                            if (child.m_GameObject.TryGet(out var go))
                            {
                                renderer = (Renderer)go.m_SkinnedMeshRenderer ?? go.m_MeshRenderer;
                            }
                            if (renderer == null)
                            {
                                continue;
                            }
                            string property = renderer.FindMaterialPropertyNameByCRC28(crc28);
                            if (property == null)
                            {
                                continue;
                            }

                            if ((attribute & 0x80000000) != 0)
                            {
                                return Prefix + property;
                            }
                            char subProperty;
                            uint subPropIndex = attribute >> 28 & 3;
                            bool isRgba = (attribute & 0x40000000) != 0;
                            switch (subPropIndex)
                            {
                                case 0:
                                    subProperty = isRgba ? 'r' : 'x';
                                    break;
                                case 1:
                                    subProperty = isRgba ? 'g' : 'y';
                                    break;
                                case 2:
                                    subProperty = isRgba ? 'b' : 'z';
                                    break;

                                default:
                                    subProperty = isRgba ? 'a' : 'w';
                                    break;
                            }
                            return Prefix + property + "." + subProperty;
                        }
                        return Prefix + attribute;
                    }

                case BindingCustomType.SpriteRenderer:
                    {
                        if (attribute == 0)
                        {
                            return "m_Sprite";
                        }
                    }
                    throw new ArgumentException($"Unknown attribute {attribute} for {type}");

                case BindingCustomType.MonoBehaviour:
                    {
                        if (attribute == CRC.CalculateDigestAscii("m_Enabled"))
                        {
                            return "m_Enabled";
                        }
                    }
                    throw new ArgumentException($"Unknown attribute {attribute} for {type}");

                case BindingCustomType.Light:
                    {
                        string ColorR = "m_Color.r";
                        if (attribute == CRC.CalculateDigestAscii(ColorR))
                        {
                            return ColorR;
                        }
                        string ColorG = "m_Color.g";
                        if (attribute == CRC.CalculateDigestAscii(ColorG))
                        {
                            return ColorG;
                        }
                        string ColorB = "m_Color.b";
                        if (attribute == CRC.CalculateDigestAscii(ColorB))
                        {
                            return ColorB;
                        }
                        string ColorA = "m_Color.a";
                        if (attribute == CRC.CalculateDigestAscii(ColorA))
                        {
                            return ColorA;
                        }
                        if (attribute == CRC.CalculateDigestAscii("m_CookieSize"))
                        {
                            return "m_CookieSize";
                        }
                        if (attribute == CRC.CalculateDigestAscii("m_DrawHalo"))
                        {
                            return "m_DrawHalo";
                        }
                        if (attribute == CRC.CalculateDigestAscii("m_Intensity"))
                        {
                            return "m_Intensity";
                        }
                        if (attribute == CRC.CalculateDigestAscii("m_Range"))
                        {
                            return "m_Range";
                        }
                        const string ShadowsStrength = "m_Shadows.m_Strength";
                        if (attribute == CRC.CalculateDigestAscii(ShadowsStrength))
                        {
                            return ShadowsStrength;
                        }
                        const string ShadowsBias = "m_Shadows.m_Bias";
                        if (attribute == CRC.CalculateDigestAscii(ShadowsBias))
                        {
                            return ShadowsBias;
                        }
                        const string ShadowsNormalBias = "m_Shadows.m_NormalBias";
                        if (attribute == CRC.CalculateDigestAscii(ShadowsNormalBias))
                        {
                            return ShadowsNormalBias;
                        }
                        const string ShadowsNearPlane = "m_Shadows.m_NearPlane";
                        if (attribute == CRC.CalculateDigestAscii(ShadowsNearPlane))
                        {
                            return ShadowsNearPlane;
                        }
                        if (attribute == CRC.CalculateDigestAscii("m_SpotAngle"))
                        {
                            return "m_SpotAngle";
                        }
                        if (attribute == CRC.CalculateDigestAscii("m_ColorTemperature"))
                        {
                            return "m_ColorTemperature";
                        }
                    }
                    throw new ArgumentException($"Unknown attribute {attribute} for {type}");

                case BindingCustomType.RendererShadows:
                    {
                        if (attribute == CRC.CalculateDigestAscii("m_ReceiveShadows"))
                        {
                            return "m_ReceiveShadows";
                        }
                        if (attribute == CRC.CalculateDigestAscii("m_SortingOrder"))
                        {
                            return "m_SortingOrder";
                        }
                    }
                    throw new ArgumentException($"Unknown attribute {attribute} for {type}");

#warning TODO:
                case BindingCustomType.ParticleSystem:
                    return "ParticleSystem_" + attribute;
                /*{
					// TODO: ordinal propertyName
				}
				throw new ArgumentException($"Unknown attribute {attribute} for {_this}");*/

                case BindingCustomType.RectTransform:
                    {
                        string LocalPositionZ = "m_LocalPosition.z";
                        if (attribute == CRC.CalculateDigestAscii(LocalPositionZ))
                        {
                            return LocalPositionZ;
                        }
                        string AnchoredPositionX = "m_AnchoredPosition.x";
                        if (attribute == CRC.CalculateDigestAscii(AnchoredPositionX))
                        {
                            return AnchoredPositionX;
                        }
                        string AnchoredPositionY = "m_AnchoredPosition.y";
                        if (attribute == CRC.CalculateDigestAscii(AnchoredPositionY))
                        {
                            return AnchoredPositionY;
                        }
                        string AnchorMinX = "m_AnchorMin.x";
                        if (attribute == CRC.CalculateDigestAscii(AnchorMinX))
                        {
                            return AnchorMinX;
                        }
                        string AnchorMinY = "m_AnchorMin.y";
                        if (attribute == CRC.CalculateDigestAscii(AnchorMinY))
                        {
                            return AnchorMinY;
                        }
                        string AnchorMaxX = "m_AnchorMax.x";
                        if (attribute == CRC.CalculateDigestAscii(AnchorMaxX))
                        {
                            return AnchorMaxX;
                        }
                        string AnchorMaxY = "m_AnchorMax.y";
                        if (attribute == CRC.CalculateDigestAscii(AnchorMaxY))
                        {
                            return AnchorMaxY;
                        }
                        string SizeDeltaX = "m_SizeDelta.x";
                        if (attribute == CRC.CalculateDigestAscii(SizeDeltaX))
                        {
                            return SizeDeltaX;
                        }
                        string SizeDeltaY = "m_SizeDelta.y";
                        if (attribute == CRC.CalculateDigestAscii(SizeDeltaY))
                        {
                            return SizeDeltaY;
                        }
                        string PivotX = "m_Pivot.x";
                        if (attribute == CRC.CalculateDigestAscii(PivotX))
                        {
                            return PivotX;
                        }
                        string PivotY = "m_Pivot.y";
                        if (attribute == CRC.CalculateDigestAscii(PivotY))
                        {
                            return PivotY;
                        }
                    }
                    throw new ArgumentException($"Unknown attribute {attribute} for {type}");

#warning TODO:
                case BindingCustomType.LineRenderer:
                    {
                        const string ParametersWidthMultiplier = "m_Parameters" + "." + "widthMultiplier";
                        if (attribute == CRC.CalculateDigestAscii(ParametersWidthMultiplier))
                        {
                            return ParametersWidthMultiplier;
                        }
                    }
                    // TODO: old versions animate all properties as custom curves
                    return "LineRenderer_" + attribute;

#warning TODO:
                case BindingCustomType.TrailRenderer:
                    {
                        const string ParametersWidthMultiplier = "m_Parameters" + "." + "widthMultiplier";
                        if (attribute == CRC.CalculateDigestAscii(ParametersWidthMultiplier))
                        {
                            return ParametersWidthMultiplier;
                        }
                    }
                    // TODO: old versions animate all properties as custom curves
                    return "TrailRenderer_" + attribute;

#warning TODO:
                case BindingCustomType.PositionConstraint:
                    {
                        uint property = attribute & 0xF;
                        switch (property)
                        {
                            case 0:
                                return "m_RestTranslation.x";
                            case 1:
                                return "m_RestTranslation.y";
                            case 2:
                                return "m_RestTranslation.z";
                            case 3:
                                return "m_Weight";
                            case 4:
                                return "m_TranslationOffset.x";
                            case 5:
                                return "m_TranslationOffset.y";
                            case 6:
                                return "m_TranslationOffset.z";
                            case 7:
                                return "m_AffectTranslationX";
                            case 8:
                                return "m_AffectTranslationY";
                            case 9:
                                return "m_AffectTranslationZ";
                            case 10:
                                return "m_Active";
                            case 11:
                                return $"m_Sources.Array.data[{attribute >> 8}].sourceTransform";
                            case 12:
                                return $"m_Sources.Array.data[{attribute >> 8}].weight";
                        }
                    }
                    throw new ArgumentException($"Unknown attribute {attribute} for {type}");

#warning TODO:
                case BindingCustomType.RotationConstraint:
                    {
                        uint property = attribute & 0xF;
                        switch (property)
                        {
                            case 0:
                                return "m_RestRotation.x";
                            case 1:
                                return "m_RestRotation.y";
                            case 2:
                                return "m_RestRotation.z";
                            case 3:
                                return "m_Weight";
                            case 4:
                                return "m_RotationOffset.x";
                            case 5:
                                return "m_RotationOffset.y";
                            case 6:
                                return "m_RotationOffset.z";
                            case 7:
                                return "m_AffectRotationX";
                            case 8:
                                return "m_AffectRotationY";
                            case 9:
                                return "m_AffectRotationZ";
                            case 10:
                                return "m_Active";
                            case 11:
                                return $"m_Sources.Array.data[{attribute >> 8}].sourceTransform";
                            case 12:
                                return $"m_Sources.Array.data[{attribute >> 8}].weight";
                        }
                    }
                    throw new ArgumentException($"Unknown attribute {attribute} for {type}");

#warning TODO:
                case BindingCustomType.ScaleConstraint:
                    {
                        uint property = attribute & 0xF;
                        switch (property)
                        {
                            case 0:
                                return "m_ScaleAtRest.x";
                            case 1:
                                return "m_ScaleAtRest.y";
                            case 2:
                                return "m_ScaleAtRest.z";
                            case 3:
                                return "m_Weight";
                            case 4:
                                return "m_ScalingOffset.x";
                            case 5:
                                return "m_ScalingOffset.y";
                            case 6:
                                return "m_ScalingOffset.z";
                            case 7:
                                return "m_AffectScalingX";
                            case 8:
                                return "m_AffectScalingY";
                            case 9:
                                return "m_AffectScalingZ";
                            case 10:
                                return "m_Active";
                            case 11:
                                return $"m_Sources.Array.data[{attribute >> 8}].sourceTransform";
                            case 12:
                                return $"m_Sources.Array.data[{attribute >> 8}].weight";
                        }
                    }
                    throw new ArgumentException($"Unknown attribute {attribute} for {type}");

#warning TODO:
                case BindingCustomType.AimConstraint:
                    {
                        uint property = attribute & 0xF;
                        switch (property)
                        {
                            case 0:
                                return "m_Weight";
                            case 1:
                                return "m_AffectRotationX";
                            case 2:
                                return "m_AffectRotationY";
                            case 3:
                                return "m_AffectRotationZ";
                            case 4:
                                return "m_Active";
                            case 5:
                                return "m_WorldUpObject";
                            case 6:
                                return $"m_Sources.Array.data[{attribute >> 8}].sourceTransform";
                            case 7:
                                return $"m_Sources.Array.data[{attribute >> 8}].weight";
                        }
                    }
                    throw new ArgumentException($"Unknown attribute {attribute} for {type}");

#warning TODO:
                case BindingCustomType.ParentConstraint:
                    {
                        uint property = attribute & 0xF;
                        switch (property)
                        {
                            case 0:
                                return "m_Weight";
                            case 1:
                                return "m_AffectTranslationX";
                            case 2:
                                return "m_AffectTranslationY";
                            case 3:
                                return "m_AffectTranslationZ";
                            case 4:
                                return "m_AffectRotationX";
                            case 5:
                                return "m_AffectRotationY";
                            case 6:
                                return "m_AffectRotationZ";
                            case 7:
                                return "m_Active";
                            case 8:
                                return $"m_TranslationOffsets.Array.data[{attribute >> 8}].x";
                            case 9:
                                return $"m_TranslationOffsets.Array.data[{attribute >> 8}].y";
                            case 10:
                                return $"m_TranslationOffsets.Array.data[{attribute >> 8}].z";
                            case 11:
                                return $"m_RotationOffsets.Array.data[{attribute >> 8}].x";
                            case 12:
                                return $"m_RotationOffsets.Array.data[{attribute >> 8}].y";
                            case 13:
                                return $"m_RotationOffsets.Array.data[{attribute >> 8}].z";
                            case 14:
                                return $"m_Sources.Array.data[{attribute >> 8}].sourceTransform";
                            case 15:
                                return $"m_Sources.Array.data[{attribute >> 8}].weight";
                        }
                    }
                    throw new ArgumentException($"Unknown attribute {attribute} for {type}");

#warning TODO:
                case BindingCustomType.LookAtConstraint:
                    {
                        uint property = attribute & 0xF;
                        switch (property)
                        {
                            case 0:
                                return "m_Weight";
                            case 1:
                                return "m_Active";
                            case 2:
                                return "m_WorldUpObject";
                            case 3:
                                return $"m_Sources.Array.data[{attribute >> 8}].sourceTransform";
                            case 4:
                                return $"m_Sources.Array.data[{attribute >> 8}].weight";
                            case 5:
                                return "m_Roll";
                        }
                    }
                    throw new ArgumentException($"Unknown attribute {attribute} for {type}");

                case BindingCustomType.Camera:
                    {
                        if (attribute == CRC.CalculateDigestAscii("field of view"))
                        {
                            return "field of view";
                        }
                        if (attribute == CRC.CalculateDigestAscii("m_FocalLength"))
                        {
                            return "m_FocalLength";
                        }
                    }
                    throw new ArgumentException($"Unknown attribute {attribute} for {type}");

                default:
                    throw new ArgumentException(type.ToString());
            }
        }

        private GameObject[] Roots
        {
            get
            {
                if (!m_rootInited)
                {
                    m_roots = m_clip.FindRoots().ToArray();
                    m_rootInited = true;
                }
                return m_roots;
            }
        }

        private readonly AnimationClip m_clip = null;

        private GameObject[] m_roots = null;
        private bool m_rootInited = false;
    }

    public static class CustomCurveResolverExtensions
    {
        public static Transform FindChild(this Transform transform, string path)
        {
            if (path.Length == 0)
            {
                return transform;
            }
            return transform.FindChild(path, 0);
        }

        private static Transform FindChild(this Transform transform, string path, int startIndex)
        {
            int separatorIndex = path.IndexOf('/', startIndex);
            string childName = separatorIndex == -1 ?
                path.Substring(startIndex, path.Length - startIndex) :
                path.Substring(startIndex, separatorIndex - startIndex);

            foreach (PPtr<Transform> childPtr in transform.m_Children)
            {
                if (childPtr.TryGet(out var child))
                {
                    if (child.m_GameObject.TryGet(out var childGO) && childGO.m_Name == childName)
                    {
                        return separatorIndex == -1 ? child : child.FindChild(path, separatorIndex + 1);
                    }
                }
            }
            return null;
        }

        public static string FindBlendShapeNameByCRC(this Mesh mesh, uint crc)
        {
            if (mesh.version[0] > 4 || (mesh.version[0] == 4 && mesh.version[1] >= 3))
            {
                return mesh.m_Shapes.FindShapeNameByCRC(crc);
            }
            else
            {
                foreach (var blendShape in mesh.m_Shapes.shapes)
                {
                    if (CRC.VerifyDigestUTF8(blendShape.name, crc))
                    {
                        return blendShape.name;
                    }
                }
                return null;
            }
        }

        public static string FindShapeNameByCRC(this BlendShapeData blendShapeData, uint crc)
        {
            foreach (var blendChannel in blendShapeData.channels)
            {
                if (blendChannel.nameHash == crc)
                {
                    return blendChannel.name;
                }
            }
            return null;
        }

        public static string FindMaterialPropertyNameByCRC28(this Renderer renderer, uint crc)
        {
            foreach (PPtr<Material> materialPtr in renderer.m_Materials)
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

        public static string FindPropertyNameByCRC28(this Material material, uint crc)
        {
            foreach (var property in material.m_SavedProperties.m_TexEnvs)
            {
                string hdrName = $"{property.Key}_HDR";
                if (CRC.Verify28DigestUTF8(hdrName, crc))
                {
                    return hdrName;
                }
                string stName = $"{property.Key}_ST";
                if (CRC.Verify28DigestUTF8(stName, crc))
                {
                    return stName;
                }
                string texelName = $"{property.Key}_TexelSize";
                if (CRC.Verify28DigestUTF8(texelName, crc))
                {
                    return texelName;
                }
            }
            foreach (var property in material.m_SavedProperties.m_Floats)
            {
                if (CRC.Verify28DigestUTF8(property.Key, crc))
                {
                    return property.Key;
                }
            }
            foreach (var property in material.m_SavedProperties.m_Colors)
            {
                if (CRC.Verify28DigestUTF8(property.Key, crc))
                {
                    return property.Key;
                }
            }
            return null;
        }
    }
}