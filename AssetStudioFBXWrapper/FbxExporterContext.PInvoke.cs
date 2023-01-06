using System.Runtime.InteropServices;

namespace AssetStudio.FbxInterop
{
    partial class FbxExporterContext
    {

        [LibraryImport(FbxDll.DllName)]
        private static partial nint AsFbxCreateContext();

        [LibraryImport(FbxDll.DllName, StringMarshalling = StringMarshalling.Utf8)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool AsFbxInitializeContext(nint context, string fileName, float scaleFactor, int versionIndex, [MarshalAs(UnmanagedType.Bool)] bool isAscii, [MarshalAs(UnmanagedType.Bool)] bool is60Fps, out string errorMessage);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxDisposeContext(ref nint ppContext);

        private static void AsFbxSetFramePaths(nint context, string[] framePaths) => AsFbxSetFramePaths(context, framePaths, framePaths.Length);

        [LibraryImport(FbxDll.DllName, StringMarshalling = StringMarshalling.Utf8)]
        private static partial void AsFbxSetFramePaths(nint context, string[] framePaths, int count);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxExportScene(nint context);

        [LibraryImport(FbxDll.DllName)]
        private static partial nint AsFbxGetSceneRootNode(nint context);

        private static nint AsFbxExportSingleFrame(nint context, nint parentNode, string framePath, string frameName, in Vector3 localPosition, in Vector3 localRotation, in Vector3 localScale)
        {
            return AsFbxExportSingleFrame(context, parentNode, framePath, frameName, localPosition.X, localPosition.Y, localPosition.Z, localRotation.X, localRotation.Y, localRotation.Z, localScale.X, localScale.Y, localScale.Z);
        }

        [LibraryImport(FbxDll.DllName, StringMarshalling = StringMarshalling.Utf8)]
        private static partial nint AsFbxExportSingleFrame(nint context, nint parentNode, string strFramePath, string strFrameName, float localPositionX, float localPositionY, float localPositionZ, float localRotationX, float localRotationY, float localRotationZ, float localScaleX, float localScaleY, float localScaleZ);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxSetJointsNode_CastToBone(nint context, nint node, float boneSize);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxSetJointsNode_BoneInPath(nint context, nint node, float boneSize);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxSetJointsNode_Generic(nint context, nint node);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxPrepareMaterials(nint context, int materialCount, int textureCount);

        [LibraryImport(FbxDll.DllName, StringMarshalling = StringMarshalling.Utf8)]
        private static partial nint AsFbxCreateTexture(nint context, string matTexName);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxLinkTexture(int dest, nint texture, nint material, float offsetX, float offsetY, float scaleX, float scaleY);

        [LibraryImport(FbxDll.DllName)]
        private static partial nint AsFbxMeshCreateClusterArray(int boneCount);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxMeshDisposeClusterArray(ref nint ppArray);

        [LibraryImport(FbxDll.DllName)]
        private static partial nint AsFbxMeshCreateCluster(nint context, nint boneNode);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxMeshAddCluster(nint array, nint cluster);

        [LibraryImport(FbxDll.DllName)]
        private static partial nint AsFbxMeshCreateMesh(nint context, nint frameNode);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxMeshInitControlPoints(nint mesh, int vertexCount);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxMeshCreateElementNormal(nint mesh);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxMeshCreateDiffuseUV(nint mesh, int uv);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxMeshCreateNormalMapUV(nint mesh, int uv);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxMeshCreateElementTangent(nint mesh);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxMeshCreateElementVertexColor(nint mesh);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxMeshCreateElementMaterial(nint mesh);

        private static nint AsFbxCreateMaterial(nint pContext, string matName, in Color diffuse, in Color ambient, in Color emissive, in Color specular, in Color reflection, float shininess, float transparency)
        {
            return AsFbxCreateMaterial(pContext, matName, diffuse.R, diffuse.G, diffuse.B, ambient.R, ambient.G, ambient.B, emissive.R, emissive.G, emissive.B, specular.R, specular.G, specular.B, reflection.R, reflection.G, reflection.B, shininess, transparency);
        }

        [LibraryImport(FbxDll.DllName, StringMarshalling = StringMarshalling.Utf8)]
        private static partial nint AsFbxCreateMaterial(nint pContext, string pMatName,
            float diffuseR, float diffuseG, float diffuseB,
            float ambientR, float ambientG, float ambientB,
            float emissiveR, float emissiveG, float emissiveB,
            float specularR, float specularG, float specularB,
            float reflectR, float reflectG, float reflectB,
            float shininess, float transparency);

        [LibraryImport(FbxDll.DllName)]
        private static partial int AsFbxAddMaterialToFrame(nint frameNode, nint material);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxSetFrameShadingModeToTextureShading(nint frameNode);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxMeshSetControlPoint(nint mesh, int index, float x, float y, float z);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxMeshAddPolygon(nint mesh, int materialIndex, int index0, int index1, int index2);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxMeshElementNormalAdd(nint mesh, int elementIndex, float x, float y, float z);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxMeshElementUVAdd(nint mesh, int elementIndex, float u, float v);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxMeshElementTangentAdd(nint mesh, int elementIndex, float x, float y, float z, float w);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxMeshElementVertexColorAdd(nint mesh, int elementIndex, float r, float g, float b, float a);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxMeshSetBoneWeight(nint pClusterArray, int boneIndex, int vertexIndex, float weight);

        [LibraryImport(FbxDll.DllName)]
        private static partial nint AsFbxMeshCreateSkinContext(nint context, nint frameNode);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxMeshDisposeSkinContext(ref nint ppSkinContext);

        [LibraryImport(FbxDll.DllName)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool FbxClusterArray_HasItemAt(nint pClusterArray, int index);

        [LibraryImport(FbxDll.DllName)]
        private static unsafe partial void AsFbxMeshSkinAddCluster(nint pSkinContext, nint pClusterArray, int index, float* pBoneMatrix);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxMeshAddDeformer(nint pSkinContext, nint pMesh);

        [LibraryImport(FbxDll.DllName)]
        private static partial nint AsFbxAnimCreateContext([MarshalAs(UnmanagedType.Bool)] bool eulerFilter);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxAnimDisposeContext(ref nint ppAnimContext);

        [LibraryImport(FbxDll.DllName, StringMarshalling = StringMarshalling.Utf8)]
        private static partial void AsFbxAnimPrepareStackAndLayer(nint pContext, nint pAnimContext, string takeName);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxAnimLoadCurves(nint pNode, nint pAnimContext);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxAnimBeginKeyModify(nint pAnimContext);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxAnimEndKeyModify(nint pAnimContext);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxAnimAddScalingKey(nint pAnimContext, float time, float x, float y, float z);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxAnimAddRotationKey(nint pAnimContext, float time, float x, float y, float z);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxAnimAddTranslationKey(nint pAnimContext, float time, float x, float y, float z);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxAnimApplyEulerFilter(nint pAnimContext, float filterPrecision);

        [LibraryImport(FbxDll.DllName)]
        private static partial int AsFbxAnimGetCurrentBlendShapeChannelCount(nint pAnimContext, nint pNode);

        [LibraryImport(FbxDll.DllName, StringMarshalling = StringMarshalling.Utf8)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool AsFbxAnimIsBlendShapeChannelMatch(nint pAnimContext, int channelIndex, string channelName);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxAnimBeginBlendShapeAnimCurve(nint pAnimContext, int channelIndex);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxAnimEndBlendShapeAnimCurve(nint pAnimContext);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxAnimAddBlendShapeKeyframe(nint pAnimContext, float time, float value);

        [LibraryImport(FbxDll.DllName)]
        private static partial nint AsFbxMorphCreateContext();

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxMorphInitializeContext(nint pContext, nint pMorphContext, nint pNode);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxMorphDisposeContext(ref nint ppMorphContext);

        [LibraryImport(FbxDll.DllName, StringMarshalling = StringMarshalling.Utf8)]
        private static partial void AsFbxMorphAddBlendShapeChannel(nint pContext, nint pMorphContext, string channelName);

        [LibraryImport(FbxDll.DllName, StringMarshalling = StringMarshalling.Utf8)]
        private static partial void AsFbxMorphAddBlendShapeChannelShape(nint pContext, nint pMorphContext, float weight, string shapeName);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxMorphCopyBlendShapeControlPoints(nint pMorphContext);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxMorphSetBlendShapeVertex(nint pMorphContext, uint index, float x, float y, float z);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxMorphCopyBlendShapeControlPointsNormal(nint pMorphContext);

        [LibraryImport(FbxDll.DllName)]
        private static partial void AsFbxMorphSetBlendShapeVertexNormal(nint pMorphContext, uint index, float x, float y, float z);

    }
}
