using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AssetStudio
{
    public class SerializedFile
    {
        public AssetsManager assetsManager;
        public Game game;
        public long offset = 0;
        public string fullName;
        public string originalPath;
        public string fileName;
        public int[] version = { 0, 0, 0, 0 };
        public BuildType buildType;
        public Dictionary<long, Object> ObjectsDic;

        public SerializedFileHeader header;
        private byte m_FileEndianess;
        public string unityVersion = "2.5.0f5";
        public BuildTarget m_TargetPlatform = BuildTarget.UnknownPlatform;
        private bool m_EnableTypeTree = true;
        public List<SerializedType> m_Types;
        public int bigIDEnabled = 0;
        public List<ObjectInfo> m_Objects;
        public Dictionary<long, ObjectInfo> m_ObjectsDic;
        private List<LocalSerializedObjectIdentifier> m_ScriptTypes;
        public List<FileIdentifier> m_Externals;
        public List<SerializedType> m_RefTypes;
        public string userInformation;

        public FileReader Reader 
        {
            get
            {
                FileReader reader;
                if (!assetsManager.serializedFileReaders.TryGetValue(fileName, out reader))
                {
                    reader = GetReader();
                    assetsManager.serializedFileReaders[fileName] = reader;
                }
                return reader;
            }
        }

        public SerializedFile(FileReader reader, AssetsManager assetsManager)
        {
            this.assetsManager = assetsManager;
            game = assetsManager.Game;
            fullName = reader.FullPath;
            fileName = reader.FileName;

            // ReadHeader
            header = new SerializedFileHeader();
            header.m_MetadataSize = reader.ReadUInt32();
            header.m_FileSize = reader.ReadUInt32();
            header.m_Version = (SerializedFileFormatVersion)reader.ReadUInt32();
            header.m_DataOffset = reader.ReadUInt32();

            if (header.m_Version >= SerializedFileFormatVersion.Unknown_9)
            {
                header.m_Endianess = reader.ReadByte();
                header.m_Reserved = reader.ReadBytes(3);
                m_FileEndianess = header.m_Endianess;
            }
            else
            {
                reader.Position = header.m_FileSize - header.m_MetadataSize;
                m_FileEndianess = reader.ReadByte();
            }

            if (header.m_Version >= SerializedFileFormatVersion.LargeFilesSupport)
            {
                header.m_MetadataSize = reader.ReadUInt32();
                header.m_FileSize = reader.ReadInt64();
                header.m_DataOffset = reader.ReadInt64();
                reader.ReadInt64(); // unknown
            }

            // ReadMetadata
            if (m_FileEndianess == 0)
            {
                reader.Endian = EndianType.LittleEndian;
            }
            if (header.m_Version >= SerializedFileFormatVersion.Unknown_7)
            {
                unityVersion = reader.ReadStringToNull();
                SetVersion(unityVersion);
            }
            if (header.m_Version >= SerializedFileFormatVersion.Unknown_8)
            {
                m_TargetPlatform = (BuildTarget)reader.ReadInt32();
                if (!Enum.IsDefined(typeof(BuildTarget), m_TargetPlatform))
                {
                    m_TargetPlatform = BuildTarget.UnknownPlatform;
                }
                else if (game.Type.IsMhyGroup())
                {
                    m_TargetPlatform = BuildTarget.StandaloneWindows64;
                }
            }
            if (header.m_Version >= SerializedFileFormatVersion.HasTypeTreeHashes)
            {
                m_EnableTypeTree = reader.ReadBoolean();
            }

            // Read Types
            int typeCount = reader.ReadInt32();
            m_Types = new List<SerializedType>(typeCount);
            for (int i = 0; i < typeCount; i++)
            {
                m_Types.Add(ReadSerializedType(reader, false));
            }

            if (header.m_Version >= SerializedFileFormatVersion.Unknown_7 && header.m_Version < SerializedFileFormatVersion.Unknown_14)
            {
                bigIDEnabled = reader.ReadInt32();
            }

            // Read Objects
            int objectCount = reader.ReadInt32();
            m_Objects = new List<ObjectInfo>(objectCount);
            m_ObjectsDic = new Dictionary<long, ObjectInfo>(objectCount);
            ObjectsDic = new Dictionary<long, Object>(objectCount);
            for (int i = 0; i < objectCount; i++)
            {
                var objectInfo = new ObjectInfo();
                if (bigIDEnabled != 0)
                {
                    objectInfo.m_PathID = reader.ReadInt64();
                }
                else if (header.m_Version < SerializedFileFormatVersion.Unknown_14)
                {
                    objectInfo.m_PathID = reader.ReadInt32();
                }
                else
                {
                    reader.AlignStream();
                    objectInfo.m_PathID = reader.ReadInt64();
                }

                if (header.m_Version >= SerializedFileFormatVersion.LargeFilesSupport)
                    objectInfo.byteStart = reader.ReadInt64();
                else
                    objectInfo.byteStart = reader.ReadUInt32();

                objectInfo.byteStart += header.m_DataOffset;
                objectInfo.byteSize = reader.ReadUInt32();
                objectInfo.typeID = reader.ReadInt32();
                if (header.m_Version < SerializedFileFormatVersion.RefactoredClassId)
                {
                    objectInfo.classID = reader.ReadUInt16();
                    objectInfo.serializedType = m_Types.Find(x => x.classID == objectInfo.typeID);
                }
                else
                {
                    var type = m_Types[objectInfo.typeID];
                    objectInfo.serializedType = type;
                    objectInfo.classID = type.classID;
                }
                if (header.m_Version < SerializedFileFormatVersion.HasScriptTypeIndex)
                {
                    objectInfo.isDestroyed = reader.ReadUInt16();
                }
                if (header.m_Version >= SerializedFileFormatVersion.HasScriptTypeIndex && header.m_Version < SerializedFileFormatVersion.RefactorTypeData)
                {
                    var m_ScriptTypeIndex = reader.ReadInt16();
                    if (objectInfo.serializedType != null)
                        objectInfo.serializedType.m_ScriptTypeIndex = m_ScriptTypeIndex;
                }
                if (header.m_Version == SerializedFileFormatVersion.SupportsStrippedObject || header.m_Version == SerializedFileFormatVersion.RefactoredClassId)
                {
                    objectInfo.stripped = reader.ReadByte();
                }
                m_Objects.Add(objectInfo);
                m_ObjectsDic.Add(objectInfo.m_PathID, objectInfo);
            }

            if (header.m_Version >= SerializedFileFormatVersion.HasScriptTypeIndex)
            {
                int scriptCount = reader.ReadInt32();
                m_ScriptTypes = new List<LocalSerializedObjectIdentifier>(scriptCount);
                for (int i = 0; i < scriptCount; i++)
                {
                    var m_ScriptType = new LocalSerializedObjectIdentifier();
                    m_ScriptType.localSerializedFileIndex = reader.ReadInt32();
                    if (header.m_Version < SerializedFileFormatVersion.Unknown_14)
                    {
                        m_ScriptType.localIdentifierInFile = reader.ReadInt32();
                    }
                    else
                    {
                        reader.AlignStream();
                        m_ScriptType.localIdentifierInFile = reader.ReadInt64();
                    }
                    m_ScriptTypes.Add(m_ScriptType);
                }
            }

            int externalsCount = reader.ReadInt32();
            m_Externals = new List<FileIdentifier>(externalsCount);
            for (int i = 0; i < externalsCount; i++)
            {
                var m_External = new FileIdentifier();
                if (header.m_Version >= SerializedFileFormatVersion.Unknown_6)
                {
                    var tempEmpty = reader.ReadStringToNull();
                }
                if (header.m_Version >= SerializedFileFormatVersion.Unknown_5)
                {
                    m_External.guid = new Guid(reader.ReadBytes(16));
                    m_External.type = reader.ReadInt32();
                }
                m_External.pathName = reader.ReadStringToNull();
                m_External.fileName = Path.GetFileName(m_External.pathName);
                m_Externals.Add(m_External);
            }

            if (header.m_Version >= SerializedFileFormatVersion.SupportsRefObject)
            {
                int refTypesCount = reader.ReadInt32();
                m_RefTypes = new List<SerializedType>(refTypesCount);
                for (int i = 0; i < refTypesCount; i++)
                {
                    m_RefTypes.Add(ReadSerializedType(reader, true));
                }
            }

            if (header.m_Version >= SerializedFileFormatVersion.Unknown_5)
            {
                userInformation = reader.ReadStringToNull();
            }

            //reader.AlignStream(16);
        }

        public void SetVersion(string stringVersion)
        {
            if (stringVersion != strippedVersion)
            {
                unityVersion = stringVersion;
                var buildSplit = Regex.Replace(stringVersion, @"\d", "").Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                buildType = new BuildType(buildSplit[0]);
                var versionSplit = Regex.Replace(stringVersion, @"\D", ".").Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                version = versionSplit.Select(int.Parse).ToArray();
            }
        }

        private SerializedType ReadSerializedType(FileReader reader, bool isRefType)
        {
            var type = new SerializedType();

            type.classID = reader.ReadInt32();

            if (game.Type.IsGIGroup() && BitConverter.ToBoolean(header.m_Reserved))
            {
                type.classID = DecodeClassID(type.classID);
            }

            if (header.m_Version >= SerializedFileFormatVersion.RefactoredClassId)
            {
                type.m_IsStrippedType = reader.ReadBoolean();
            }

            if (header.m_Version >= SerializedFileFormatVersion.RefactorTypeData)
            {
                type.m_ScriptTypeIndex = reader.ReadInt16();
            }

            if (header.m_Version >= SerializedFileFormatVersion.HasTypeTreeHashes)
            {
                if (isRefType && type.m_ScriptTypeIndex >= 0)
                {
                    type.m_ScriptID = reader.ReadBytes(16);
                }
                else if ((header.m_Version < SerializedFileFormatVersion.RefactoredClassId && type.classID < 0) || (header.m_Version >= SerializedFileFormatVersion.RefactoredClassId && type.classID == 114))
                {
                    type.m_ScriptID = reader.ReadBytes(16);
                }
                type.m_OldTypeHash = reader.ReadBytes(16);
            }

            if (m_EnableTypeTree)
            {
                type.m_Type = new TypeTree();
                type.m_Type.m_Nodes = new List<TypeTreeNode>();
                if (header.m_Version >= SerializedFileFormatVersion.Unknown_12 || header.m_Version == SerializedFileFormatVersion.Unknown_10)
                {
                    TypeTreeBlobRead(reader, type.m_Type);
                }
                else
                {
                    ReadTypeTree(reader, type.m_Type);
                }
                if (header.m_Version >= SerializedFileFormatVersion.StoresTypeDependencies)
                {
                    if (isRefType)
                    {
                        type.m_KlassName = reader.ReadStringToNull();
                        type.m_NameSpace = reader.ReadStringToNull();
                        type.m_AsmName = reader.ReadStringToNull();
                    }
                    else
                    {
                        type.m_TypeDependencies = reader.ReadInt32Array();
                    }
                }
            }

            return type;
        }

        private void ReadTypeTree(FileReader reader, TypeTree m_Type, int level = 0)
        {
            var typeTreeNode = new TypeTreeNode();
            m_Type.m_Nodes.Add(typeTreeNode);
            typeTreeNode.m_Level = level;
            typeTreeNode.m_Type = reader.ReadStringToNull();
            typeTreeNode.m_Name = reader.ReadStringToNull();
            typeTreeNode.m_ByteSize = reader.ReadInt32();
            if (header.m_Version == SerializedFileFormatVersion.Unknown_2)
            {
                var variableCount = reader.ReadInt32();
            }
            if (header.m_Version != SerializedFileFormatVersion.Unknown_3)
            {
                typeTreeNode.m_Index = reader.ReadInt32();
            }
            typeTreeNode.m_TypeFlags = reader.ReadInt32();
            typeTreeNode.m_Version = reader.ReadInt32();
            if (header.m_Version != SerializedFileFormatVersion.Unknown_3)
            {
                typeTreeNode.m_MetaFlag = reader.ReadInt32();
            }

            int childrenCount = reader.ReadInt32();
            for (int i = 0; i < childrenCount; i++)
            {
                ReadTypeTree(reader, m_Type, level + 1);
            }
        }

        private void TypeTreeBlobRead(FileReader reader, TypeTree m_Type)
        {
            int numberOfNodes = reader.ReadInt32();
            int stringBufferSize = reader.ReadInt32();
            for (int i = 0; i < numberOfNodes; i++)
            {
                var typeTreeNode = new TypeTreeNode();
                m_Type.m_Nodes.Add(typeTreeNode);
                typeTreeNode.m_Version = reader.ReadUInt16();
                typeTreeNode.m_Level = reader.ReadByte();
                typeTreeNode.m_TypeFlags = reader.ReadByte();
                typeTreeNode.m_TypeStrOffset = reader.ReadUInt32();
                typeTreeNode.m_NameStrOffset = reader.ReadUInt32();
                typeTreeNode.m_ByteSize = reader.ReadInt32();
                typeTreeNode.m_Index = reader.ReadInt32();
                typeTreeNode.m_MetaFlag = reader.ReadInt32();
                if (header.m_Version >= SerializedFileFormatVersion.TypeTreeNodeWithTypeFlags)
                {
                    typeTreeNode.m_RefTypeHash = reader.ReadUInt64();
                }
            }
            m_Type.m_StringBuffer = reader.ReadBytes(stringBufferSize);

            using (var stringBufferReader = new EndianBinaryReader(new MemoryStream(m_Type.m_StringBuffer), EndianType.LittleEndian))
            {
                for (int i = 0; i < numberOfNodes; i++)
                {
                    var m_Node = m_Type.m_Nodes[i];
                    m_Node.m_Type = ReadString(stringBufferReader, m_Node.m_TypeStrOffset);
                    m_Node.m_Name = ReadString(stringBufferReader, m_Node.m_NameStrOffset);
                }
            }

            string ReadString(EndianBinaryReader stringBufferReader, uint value)
            {
                var isOffset = (value & 0x80000000) == 0;
                if (isOffset)
                {
                    stringBufferReader.BaseStream.Position = value;
                    return stringBufferReader.ReadStringToNull();
                }
                var offset = value & 0x7FFFFFFF;
                if (CommonString.StringBuffer.TryGetValue(offset, out var str))
                {
                    return str;
                }
                return offset.ToString();
            }
        }

        public string ReadObjectName(ObjectInfo objInfo, bool cacheObject = false, bool skipContainer = false)
        {
            if (objInfo.name == null)
            {
                var objectReader = new ObjectReader(Reader, this, objInfo, game);
                objectReader.Reset();
                switch (objInfo.type)
                {
                    case ClassIDType.AssetBundle:
                        var assetBundle = ReadObject(objInfo, cacheObject) as AssetBundle;
                        if (!skipContainer)
                        {
                            foreach (var m_Container in assetBundle.m_Container)
                            {
                                var preloadIndex = m_Container.Value.preloadIndex;
                                var preloadSize = m_Container.Value.preloadSize;
                                var preloadEnd = preloadIndex + preloadSize;
                                for (int k = preloadIndex; k < preloadEnd; k++)
                                {
                                    if (assetBundle.m_PreloadTable[k].TryGetInfo(out var info))
                                    {
                                        info.container = m_Container.Key;
                                    }
                                }
                            }
                        }
                        objInfo.name = assetBundle.m_Name;
                        break;
                    case ClassIDType.ResourceManager:
                        var resourceManager = ReadObject(objInfo, cacheObject) as ResourceManager;
                        if (!skipContainer)
                        {
                            foreach (var m_Container in resourceManager.m_Container)
                            {
                                if (m_Container.Value.TryGetInfo(out var info))
                                {
                                    info.container = m_Container.Key;
                                }
                            }
                        }
                        objInfo.name = objInfo.type.ToString();
                        break;
                    case ClassIDType.GameObject:
                        var gameObject = ReadObject(objInfo, cacheObject) as GameObject;
                        objInfo.name = gameObject.m_Name;
                        break;
                    case ClassIDType.Shader when Shader.Parsable:
                        objInfo.name = objectReader.ReadAlignedString();
                        if (string.IsNullOrEmpty(objInfo.name))
                        {
                            var m_parsedForm = new SerializedShader(objectReader);
                            objInfo.name = m_parsedForm.m_Name;
                        }
                        break;
                    case ClassIDType.IndexObject:
                        var indexObject = ReadObject(ClassIDType.IndexObject, cacheObject: false) as IndexObject;
                        foreach(var index in indexObject.AssetMap)
                        {
                            if (index.Value.Object.TryGetInfo(out var mihoyoBinDataInfo))
                            {
                                if (int.TryParse(index.Key, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hash))
                                {
                                    mihoyoBinDataInfo.name = index.Key;
                                    mihoyoBinDataInfo.container = hash.ToString();
                                }
                                else mihoyoBinDataInfo.name = $"BinFile #{index.Value.Object.m_PathID}";
                            }
                        }
                        objInfo.name = objInfo.type.ToString();
                        break;
                    case ClassIDType.PlayerSettings:
                        var playerSettings = ReadObject(objInfo, cacheObject) as PlayerSettings;
                        objInfo.name = playerSettings.productName;
                        break;
                    case ClassIDType.MonoScript:
                        var monoScript = ReadObject(objInfo, cacheObject) as MonoScript;
                        objInfo.name = monoScript.m_ClassName;
                        break;
                    case ClassIDType.MonoBehaviour:
                        var monoBehaviour = ReadObject(objInfo, cacheObject) as MonoBehaviour;
                        if (monoBehaviour.m_Name == "" && monoBehaviour.m_Script.TryGetName(out var scriptName))
                        {
                            objInfo.name = scriptName;
                        }
                        else
                        {
                            objInfo.name = monoBehaviour.m_Name;
                        }
                        break;
                    case ClassIDType.Animator:
                        var animatorGameObject = new PPtr<Object>(objectReader);
                        if (animatorGameObject.TryGetName(out var gameObjectName))
                        {
                            objInfo.name = gameObjectName;
                        }
                        break;
                    case ClassIDType.Font:
                    case ClassIDType.Material:
                    case ClassIDType.Texture:
                    case ClassIDType.Mesh:
                    case ClassIDType.Sprite:
                    case ClassIDType.TextAsset:
                    case ClassIDType.Texture2D:
                    case ClassIDType.VideoClip:
                    case ClassIDType.AudioClip:
                    case ClassIDType.AnimationClip:
                        objInfo.name = objectReader.ReadAlignedString();
                        break;
                }
            }
        
            return objInfo.name;
        }

        public Object[] ReadObjects(ClassIDType type = ClassIDType.UnknownType)
        {
            var objects = new List<Object>();
            var objInfos = type != ClassIDType.UnknownType ? m_Objects.Where(x => x.type == type) : m_Objects;

            foreach (var objInfo in objInfos)
            {
                objects.Add(ReadObject(objInfo));
            }
        
            return objects.ToArray();
        }

        public Object ReadObject(ClassIDType type, long pathId = 0, bool cacheObject = true)
        {
            var objInfo = m_Objects.FirstOrDefault(x =>
            {
                var match = x.type == type;
                if (pathId != 0)
                {
                    match &= x.m_PathID == pathId;
                }
                return match;
            });
            return ReadObject(objInfo, cacheObject);
        }

        public Object ReadObject(ObjectInfo objInfo, bool cacheObject = true)
        {
            Object obj;

            if (assetsManager.CacheObjects && cacheObject)
            {
                if (!ObjectsDic.TryGetValue(objInfo.m_PathID, out obj))
                {
                    obj = ReadObjectInner(objInfo);
                    ObjectsDic[objInfo.m_PathID] = obj;
                }
            }
            else
            {
                obj = ReadObjectInner(objInfo);
            }
            
            
            return obj;
        }

        private Object ReadObjectInner(ObjectInfo objInfo)
        {
            Object obj = null;

            var objectReader = new ObjectReader(Reader, this, objInfo, game);
            try
            {
                obj = objectReader.type switch
                {
                    ClassIDType.Animation => new Animation(objectReader),
                    ClassIDType.AnimationClip => new AnimationClip(objectReader),
                    ClassIDType.Animator => new Animator(objectReader),
                    ClassIDType.AnimatorController => new AnimatorController(objectReader),
                    ClassIDType.AnimatorOverrideController => new AnimatorOverrideController(objectReader),
                    ClassIDType.AssetBundle => new AssetBundle(objectReader),
                    ClassIDType.AudioClip => new AudioClip(objectReader),
                    ClassIDType.Avatar => new Avatar(objectReader),
                    ClassIDType.Font => new Font(objectReader),
                    ClassIDType.GameObject => new GameObject(objectReader),
                    ClassIDType.IndexObject => new IndexObject(objectReader),
                    ClassIDType.Material => new Material(objectReader),
                    ClassIDType.Mesh => new Mesh(objectReader),
                    ClassIDType.MeshFilter => new MeshFilter(objectReader),
                    ClassIDType.MeshRenderer when Renderer.Parsable => new MeshRenderer(objectReader),
                    ClassIDType.MiHoYoBinData => new MiHoYoBinData(objectReader),
                    ClassIDType.MonoBehaviour => new MonoBehaviour(objectReader),
                    ClassIDType.MonoScript => new MonoScript(objectReader),
                    ClassIDType.MovieTexture => new MovieTexture(objectReader),
                    ClassIDType.PlayerSettings => new PlayerSettings(objectReader),
                    ClassIDType.RectTransform => new RectTransform(objectReader),
                    ClassIDType.Shader when Shader.Parsable => new Shader(objectReader),
                    ClassIDType.SkinnedMeshRenderer when Renderer.Parsable => new SkinnedMeshRenderer(objectReader),
                    ClassIDType.Sprite => new Sprite(objectReader),
                    ClassIDType.SpriteAtlas => new SpriteAtlas(objectReader),
                    ClassIDType.TextAsset => new TextAsset(objectReader),
                    ClassIDType.Texture2D => new Texture2D(objectReader),
                    ClassIDType.Transform => new Transform(objectReader),
                    ClassIDType.VideoClip => new VideoClip(objectReader),
                    ClassIDType.ResourceManager => new ResourceManager(objectReader),
                    _ => new Object(objectReader),
                };
            }
            catch (Exception e)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Unable to load object")
                    .AppendLine($"Assets {fileName}")
                    .AppendLine($"Path {originalPath}")
                    .AppendLine($"Type {objectReader.type}")
                    .AppendLine($"PathID {objInfo.m_PathID}")
                    .Append(e);
                Logger.Error(sb.ToString());
            }

            return obj;
        }

        private FileReader GetReader()
        {
            var path = Path.GetRelativePath(originalPath, fullName);
            var paths = new Stack<string>(path.Split(Path.DirectorySeparatorChar).Reverse());
            var reader = assetsManager.GetReader(originalPath, paths, game, offset);
            reader.Endian = m_FileEndianess == 0 ? EndianType.LittleEndian : EndianType.BigEndian;
            return reader;
        }

        private static int DecodeClassID(int value)
        {
            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            value = BitConverter.ToInt32(bytes, 0);
            return (value ^ 0x23746FBE) - 3;
        }

        public bool IsVersionStripped => unityVersion == strippedVersion;

        private const string strippedVersion = "0.0.0";
    }
}
