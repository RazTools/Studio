using System;
using System.Runtime.InteropServices;

namespace AssetStudio.FbxInterop
{
    partial class FbxExporterContext
    {

        [DllImport(FbxDll.DllName)]
        private static extern IntPtr AsFbxCreateContext();

        [DllImport(FbxDll.DllName)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AsFbxInitializeContext(IntPtr context, [MarshalAs(UnmanagedType.LPUTF8Str)] string fileName, float scaleFactor, int versionIndex, [MarshalAs(UnmanagedType.Bool)] bool isAscii, [MarshalAs(UnmanagedType.Bool)] bool is60Fps, out string errorMessage);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxDisposeContext(ref IntPtr ppContext);

        private static void AsFbxSetFramePaths(IntPtr context, string[] framePaths) => AsFbxSetFramePaths(context, framePaths, framePaths.Length);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxSetFramePaths(IntPtr context, [MarshalAs(UnmanagedType.LPUTF8Str)] string[] framePaths, int count);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxExportScene(IntPtr context);

        [DllImport(FbxDll.DllName)]
        private static extern IntPtr AsFbxGetSceneRootNode(IntPtr context);

        private static IntPtr AsFbxExportSingleFrame(IntPtr context, IntPtr parentNode, string framePath, string frameName, in Vector3 localPosition, in Quaternion localRotation, in Vector3 localScale)
        {
            var localRotationEuler = Fbx.QuaternionToEuler(localRotation);
            return AsFbxExportSingleFrame(context, parentNode, framePath, frameName, localPosition.X, localPosition.Y, localPosition.Z, localRotationEuler.X, localRotationEuler.Y, localRotationEuler.Z, localScale.X, localScale.Y, localScale.Z);
        }

        [DllImport(FbxDll.DllName)]
        private static extern IntPtr AsFbxExportSingleFrame(IntPtr context, IntPtr parentNode, [MarshalAs(UnmanagedType.LPUTF8Str)] string strFramePath, [MarshalAs(UnmanagedType.LPUTF8Str)] string strFrameName, float localPositionX, float localPositionY, float localPositionZ, float localRotationX, float localRotationY, float localRotationZ, float localScaleX, float localScaleY, float localScaleZ);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxSetJointsNode_CastToBone(IntPtr context, IntPtr node, float boneSize);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxSetJointsNode_BoneInPath(IntPtr context, IntPtr node, float boneSize);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxSetJointsNode_Generic(IntPtr context, IntPtr node);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxPrepareMaterials(IntPtr context, int materialCount, int textureCount);

        [DllImport(FbxDll.DllName)]
        private static extern IntPtr AsFbxCreateTexture(IntPtr context, [MarshalAs(UnmanagedType.LPUTF8Str)] string matTexName);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxLinkTexture(int dest, IntPtr texture, IntPtr material, float offsetX, float offsetY, float scaleX, float scaleY);

        [DllImport(FbxDll.DllName)]
        private static extern IntPtr AsFbxMeshCreateClusterArray(int boneCount);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxMeshDisposeClusterArray(ref IntPtr ppArray);

        [DllImport(FbxDll.DllName)]
        private static extern IntPtr AsFbxMeshCreateCluster(IntPtr context, IntPtr boneNode);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxMeshAddCluster(IntPtr array, IntPtr cluster);

        [DllImport(FbxDll.DllName)]
        private static extern IntPtr AsFbxMeshCreateMesh(IntPtr context, IntPtr frameNode);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxMeshInitControlPoints(IntPtr mesh, int vertexCount);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxMeshCreateElementNormal(IntPtr mesh);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxMeshCreateUV(IntPtr mesh, int uv, int uvType);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxMeshCreateElementTangent(IntPtr mesh);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxMeshCreateElementVertexColor(IntPtr mesh);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxMeshCreateElementMaterial(IntPtr mesh);

        private static IntPtr AsFbxCreateMaterial(IntPtr pContext, string matName, in Color diffuse, in Color ambient, in Color emissive, in Color specular, in Color reflection, float shininess, float transparency)
        {
            return AsFbxCreateMaterial(pContext, matName, diffuse.R, diffuse.G, diffuse.B, ambient.R, ambient.G, ambient.B, emissive.R, emissive.G, emissive.B, specular.R, specular.G, specular.B, reflection.R, reflection.G, reflection.B, shininess, transparency);
        }

        [DllImport(FbxDll.DllName)]
        private static extern IntPtr AsFbxCreateMaterial(IntPtr pContext, [MarshalAs(UnmanagedType.LPUTF8Str)] string pMatName,
            float diffuseR, float diffuseG, float diffuseB,
            float ambientR, float ambientG, float ambientB,
            float emissiveR, float emissiveG, float emissiveB,
            float specularR, float specularG, float specularB,
            float reflectR, float reflectG, float reflectB,
            float shininess, float transparency);

        [DllImport(FbxDll.DllName)]
        private static extern int AsFbxAddMaterialToFrame(IntPtr frameNode, IntPtr material);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxSetFrameShadingModeToTextureShading(IntPtr frameNode);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxMeshSetControlPoint(IntPtr mesh, int index, float x, float y, float z);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxMeshAddPolygon(IntPtr mesh, int materialIndex, int index0, int index1, int index2);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxMeshElementNormalAdd(IntPtr mesh, int elementIndex, float x, float y, float z);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxMeshElementUVAdd(IntPtr mesh, int elementIndex, float u, float v);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxMeshElementTangentAdd(IntPtr mesh, int elementIndex, float x, float y, float z, float w);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxMeshElementVertexColorAdd(IntPtr mesh, int elementIndex, float r, float g, float b, float a);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxMeshSetBoneWeight(IntPtr pClusterArray, int boneIndex, int vertexIndex, float weight);

        [DllImport(FbxDll.DllName)]
        private static extern IntPtr AsFbxMeshCreateSkinContext(IntPtr context, IntPtr frameNode);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxMeshDisposeSkinContext(ref IntPtr ppSkinContext);

        [DllImport(FbxDll.DllName)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FbxClusterArray_HasItemAt(IntPtr pClusterArray, int index);

        [DllImport(FbxDll.DllName)]
        private static unsafe extern void AsFbxMeshSkinAddCluster(IntPtr pSkinContext, IntPtr pClusterArray, int index, float* pBoneMatrix);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxMeshAddDeformer(IntPtr pSkinContext, IntPtr pMesh);

        [DllImport(FbxDll.DllName)]
        private static extern IntPtr AsFbxAnimCreateContext([MarshalAs(UnmanagedType.Bool)] bool eulerFilter);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxAnimDisposeContext(ref IntPtr ppAnimContext);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxAnimPrepareStackAndLayer(IntPtr pContext, IntPtr pAnimContext, [MarshalAs(UnmanagedType.LPUTF8Str)] string takeName);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxAnimLoadCurves(IntPtr pNode, IntPtr pAnimContext);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxAnimBeginKeyModify(IntPtr pAnimContext);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxAnimEndKeyModify(IntPtr pAnimContext);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxAnimAddScalingKey(IntPtr pAnimContext, float time, float x, float y, float z);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxAnimAddRotationKey(IntPtr pAnimContext, float time, float x, float y, float z);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxAnimAddTranslationKey(IntPtr pAnimContext, float time, float x, float y, float z);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxAnimApplyEulerFilter(IntPtr pAnimContext, float filterPrecision);

        [DllImport(FbxDll.DllName)]
        private static extern int AsFbxAnimGetCurrentBlendShapeChannelCount(IntPtr pAnimContext, IntPtr pNode);

        [DllImport(FbxDll.DllName)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AsFbxAnimIsBlendShapeChannelMatch(IntPtr pAnimContext, int channelIndex, [MarshalAs(UnmanagedType.LPUTF8Str)] string channelName);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxAnimBeginBlendShapeAnimCurve(IntPtr pAnimContext, int channelIndex);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxAnimEndBlendShapeAnimCurve(IntPtr pAnimContext);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxAnimAddBlendShapeKeyframe(IntPtr pAnimContext, float time, float value);

        [DllImport(FbxDll.DllName)]
        private static extern IntPtr AsFbxMorphCreateContext();

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxMorphInitializeContext(IntPtr pContext, IntPtr pMorphContext, IntPtr pNode);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxMorphDisposeContext(ref IntPtr ppMorphContext);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxMorphAddBlendShapeChannel(IntPtr pContext, IntPtr pMorphContext, [MarshalAs(UnmanagedType.LPUTF8Str)] string channelName);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxMorphAddBlendShapeChannelShape(IntPtr pContext, IntPtr pMorphContext, float weight, [MarshalAs(UnmanagedType.LPUTF8Str)] string shapeName);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxMorphCopyBlendShapeControlPoints(IntPtr pMorphContext);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxMorphSetBlendShapeVertex(IntPtr pMorphContext, uint index, float x, float y, float z);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxMorphCopyBlendShapeControlPointsNormal(IntPtr pMorphContext);

        [DllImport(FbxDll.DllName)]
        private static extern void AsFbxMorphSetBlendShapeVertexNormal(IntPtr pMorphContext, uint index, float x, float y, float z);

    }
}
