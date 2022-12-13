using AssetStudio;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static AssetStudioCLI.Exporter;
using Object = AssetStudio.Object;

namespace AssetStudioCLI
{
    [Flags]
    public enum MapOpType
    {
        None,
        AssetMap,
        CABMap,
        Both
    }
    public enum ExportListType
    {
        XML,
        JSON
    }
    public enum AssetGroupOption
    {
        ByType,
        ByContainer,
        BySource,
        None,
    }

    internal static class Studio
    {
        public static AssetsManager assetsManager = new AssetsManager() { ResolveDependancies = false };
        public static List<AssetItem> exportableAssets = new List<AssetItem>();
        public static Game Game;

        public static int ExtractFolder(string path, string savePath)
        {
            int extractedCount = 0;
            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                var fileOriPath = Path.GetDirectoryName(file);
                var fileSavePath = fileOriPath.Replace(path, savePath);
                extractedCount += ExtractFile(file, fileSavePath);
            }
            return extractedCount;
        }

        public static int ExtractFile(string[] fileNames, string savePath)
        {
            int extractedCount = 0;
            for (var i = 0; i < fileNames.Length; i++)
            {
                var fileName = fileNames[i];
                extractedCount += ExtractFile(fileName, savePath);
            }
            return extractedCount;
        }

        public static int ExtractFile(string fileName, string savePath)
        {
            int extractedCount = 0;
            var reader = new FileReader(fileName, Game);
            if (reader.FileType == FileType.BundleFile)
                extractedCount += ExtractBundleFile(reader, savePath);
            else if (reader.FileType == FileType.WebFile)
                extractedCount += ExtractWebDataFile(reader, savePath);
            else if (reader.FileType == FileType.GameFile)
                extractedCount += ExtractGameFile(reader, savePath);
            else
                reader.Dispose();
            return extractedCount;
        }

        private static int ExtractBundleFile(FileReader reader, string savePath)
        {
            Logger.Info($"Decompressing {reader.FileName} ...");
            var bundleFile = new BundleFile(reader);
            reader.Dispose();
            if (bundleFile.FileList.Length > 0)
            {
                var extractPath = Path.Combine(savePath, reader.FileName + "_unpacked");
                return ExtractStreamFile(extractPath, bundleFile.FileList);
            }
            return 0;
        }

        private static int ExtractWebDataFile(FileReader reader, string savePath)
        {
            Logger.Info($"Decompressing {reader.FileName} ...");
            var webFile = new WebFile(reader);
            reader.Dispose();
            if (webFile.fileList.Length > 0)
            {
                var extractPath = Path.Combine(savePath, reader.FileName + "_unpacked");
                return ExtractStreamFile(extractPath, webFile.fileList);
            }
            return 0;
        }

        private static int ExtractGameFile(FileReader reader, string savePath)
        {
            Logger.Info($"Decompressing {reader.FileName}...");
            var gameFile = new GameFile(reader);
            reader.Dispose();
            var fileList = gameFile.Bundles.SelectMany(x => x.Value).ToList();
            if (fileList.Count > 0)
            {
                var extractPath = Path.Combine(savePath, Path.GetFileNameWithoutExtension(reader.FileName));
                return ExtractStreamFile(extractPath, fileList.ToArray());
            }
            return 0;
        }

        private static int ExtractStreamFile(string extractPath, StreamFile[] fileList)
        {
            int extractedCount = 0;
            foreach (var file in fileList)
            {
                var filePath = Path.Combine(extractPath, file.path);
                var fileDirectory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(fileDirectory))
                {
                    Directory.CreateDirectory(fileDirectory);
                }
                if (!File.Exists(filePath))
                {
                    using (var fileStream = File.Create(filePath))
                    {
                        file.stream.CopyTo(fileStream);
                    }
                    extractedCount += 1;
                }
                file.stream.Dispose();
            }
            return extractedCount;
        }

        public static List<AssetEntry> BuildAssetMap(List<string> files, ClassIDType[] typeFilters, Regex[] nameFilters, Regex[] containerFilters)
        {
            var assets = new List<AssetEntry>();
            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                var reader = new FileReader(file, Game);
                var gameFile = new GameFile(reader);
                reader.Dispose();

                foreach (var bundle in gameFile.Bundles)
                {
                    foreach (var cab in bundle.Value)
                    {
                        var dummyPath = Path.Combine(Path.GetDirectoryName(file), cab.fileName);
                        using (var cabReader = new FileReader(dummyPath, cab.stream, Game))
                        {
                            if (cabReader.FileType == FileType.AssetsFile)
                            {
                                var assetsFile = new SerializedFile(cabReader, assetsManager, file);
                                assetsManager.assetsFileList.Add(assetsFile);

                                assetsFile.m_Objects = assetsFile.m_Objects.Where(x => x.HasExportableType()).ToList();

                                IndexObject indexObject = null;
                                var containers = new List<(PPtr<Object>, string)>(assetsFile.m_Objects.Count);
                                var animators = new List<(PPtr<GameObject>, AssetEntry)>(assetsFile.m_Objects.Count);
                                var objectAssetItemDic = new Dictionary<Object, AssetEntry>(assetsFile.m_Objects.Count);
                                foreach (var objInfo in assetsFile.m_Objects)
                                {
                                    var objectReader = new ObjectReader(assetsFile.reader, assetsFile, objInfo);
                                    var obj = new Object(objectReader);
                                    var asset = new AssetEntry()
                                    {
                                        Source = file,
                                        PathID = objectReader.m_PathID,
                                        Type = objectReader.type,
                                        Container = ""
                                    };

                                    var exportable = true;
                                    switch (objectReader.type)
                                    {
                                        case ClassIDType.AssetBundle:
                                            var assetBundle = new AssetBundle(objectReader);
                                            foreach (var m_Container in assetBundle.Container)
                                            {
                                                var preloadIndex = m_Container.Value.preloadIndex;
                                                var preloadSize = m_Container.Value.preloadSize;
                                                var preloadEnd = preloadIndex + preloadSize;
                                                for (int k = preloadIndex; k < preloadEnd; k++)
                                                {
                                                    if (Game.Name == "GI" || Game.Name == "GI_CB2" || Game.Name == "GI_CB3")
                                                    {
                                                        if (long.TryParse(m_Container.Key, out var containerValue))
                                                        {
                                                            var last = unchecked((uint)containerValue);
                                                            var path = ResourceIndex.GetBundlePath(last);
                                                            if (!string.IsNullOrEmpty(path))
                                                            {
                                                                containers.Add((assetBundle.PreloadTable[k], path));
                                                                continue;
                                                            }
                                                        }
                                                    }
                                                    containers.Add((assetBundle.PreloadTable[k], m_Container.Key));
                                                }
                                            }
                                            obj = null;
                                            asset.Name = assetBundle.m_Name;
                                            exportable = AssetBundle.Exportable;
                                            break;
                                        case ClassIDType.GameObject:
                                            var gameObject = new GameObject(objectReader);
                                            obj = gameObject;
                                            asset.Name = gameObject.m_Name;
                                            exportable = false;
                                            break;
                                        case ClassIDType.Shader:
                                            asset.Name = objectReader.ReadAlignedString();
                                            if (string.IsNullOrEmpty(asset.Name))
                                            {
                                                var m_parsedForm = new SerializedShader(objectReader);
                                                asset.Name = m_parsedForm.m_Name;
                                            }
                                            break;
                                        case ClassIDType.Animator:
                                            var component = new PPtr<GameObject>(objectReader);
                                            animators.Add((component, asset));
                                            asset.Name = "Animator";
                                            break;
                                        case ClassIDType.MiHoYoBinData:
                                            if (indexObject.Names.TryGetValue(objectReader.m_PathID, out var binName))
                                            {
                                                var path = ResourceIndex.GetContainerFromBinName(binName);
                                                asset.Container = path;
                                                asset.Name = !string.IsNullOrEmpty(path) ? Path.GetFileName(path) : binName;
                                            }
                                            exportable = IndexObject.Exportable;
                                            break;
                                        case ClassIDType.IndexObject:
                                            indexObject = new IndexObject(objectReader);
                                            obj = null;
                                            asset.Name = "IndexObject";
                                            exportable = IndexObject.Exportable;
                                            break;
                                        default:
                                            asset.Name = objectReader.ReadAlignedString();
                                            break;
                                    }
                                    if (obj != null)
                                    {
                                        objectAssetItemDic.Add(obj, asset);
                                        assetsFile.AddObject(obj);
                                    }
                                    var isMatchRegex = nameFilters.Length == 0 || nameFilters.Any(x => x.IsMatch(asset.Name) || asset.Type == ClassIDType.Animator);
                                    var isFilteredType = typeFilters.Length == 0 || typeFilters.Contains(asset.Type) || asset.Type == ClassIDType.Animator;
                                    if (isMatchRegex && isFilteredType && exportable)
                                    {
                                        assets.Add(asset);
                                    }
                                }
                                foreach (var pair in animators)
                                {
                                    if (pair.Item1.TryGet(out var gameObject) && gameObject is GameObject && (nameFilters.Length == 0 || nameFilters.Any(x => x.IsMatch(gameObject.m_Name))) && (typeFilters.Length == 0 || typeFilters.Contains(pair.Item2.Type)))
                                    {
                                        pair.Item2.Name = gameObject.m_Name;
                                    }
                                    else
                                    {
                                        assets.Remove(pair.Item2);
                                    }
                                }
                                foreach ((var pptr, var container) in containers)
                                {
                                    if (pptr.TryGet(out var obj))
                                    {
                                        var item = objectAssetItemDic[obj];
                                        if (containerFilters.Length == 0 || containerFilters.Any(x => x.IsMatch(container)))
                                        {
                                            item.Container = container;
                                        }
                                        else
                                        {
                                            assets.Remove(item);
                                        }
                                    }
                                }
                                assetsManager.assetsFileList.Clear();
                            }
                        }
                    }
                }

                Logger.Info($"[{i + 1}/{files.Count}] Processed {Path.GetFileName(file)}");
                Progress.Report(i + 1, files.Count);
            }

            return assets;
        }

        public static void BuildAssetData(ClassIDType[] typeFilters, Regex[] nameFilters, Regex[] containerFilters, ref int i)
        {
            string productName = null;
            var objectCount = assetsManager.assetsFileList.Sum(x => x.Objects.Count);
            var objectAssetItemDic = new Dictionary<Object, AssetItem>(objectCount);
            var containers = new List<(PPtr<Object>, string)>();
            foreach (var assetsFile in assetsManager.assetsFileList)
            {
                foreach (var asset in assetsFile.Objects)
                {
                    var assetItem = new AssetItem(asset);
                    objectAssetItemDic.Add(asset, assetItem);
                    assetItem.UniqueID = "#" + i;
                    assetItem.Text = "";
                    var exportable = false;
                    switch (asset)
                    {
                        case GameObject m_GameObject:
                            assetItem.Text = m_GameObject.m_Name;
                            break;
                        case Texture2D m_Texture2D:
                            if (!string.IsNullOrEmpty(m_Texture2D.m_StreamData?.path))
                                assetItem.FullSize = asset.byteSize + m_Texture2D.m_StreamData.size;
                            assetItem.Text = m_Texture2D.m_Name;
                            exportable = true;
                            break;
                        case AudioClip m_AudioClip:
                            if (!string.IsNullOrEmpty(m_AudioClip.m_Source))
                                assetItem.FullSize = asset.byteSize + m_AudioClip.m_Size;
                            assetItem.Text = m_AudioClip.m_Name;
                            break;
                        case VideoClip m_VideoClip:
                            if (!string.IsNullOrEmpty(m_VideoClip.m_OriginalPath))
                                assetItem.FullSize = asset.byteSize + (long)m_VideoClip.m_ExternalResources.m_Size;
                            assetItem.Text = m_VideoClip.m_Name;
                            break;
                        case Shader m_Shader:
                            assetItem.Text = m_Shader.m_ParsedForm?.m_Name ?? m_Shader.m_Name;
                            exportable = true;
                            break;
                        case Mesh _:
                        case TextAsset _:
                        case AnimationClip _:
                        case Font _:
                        case MovieTexture _:
                        case Sprite _:
                        case Material _:
                            assetItem.Text = ((NamedObject)asset).m_Name;
                            exportable = true;
                            break;
                        case Animator m_Animator:
                            if (m_Animator.m_GameObject.TryGet(out var gameObject))
                            {
                                assetItem.Text = gameObject.m_Name;
                            }
                            break;
                        case MonoBehaviour m_MonoBehaviour:
                            if (m_MonoBehaviour.m_Name == "" && m_MonoBehaviour.m_Script.TryGet(out var m_Script))
                            {
                                assetItem.Text = m_Script.m_ClassName;
                            }
                            else
                            {
                                assetItem.Text = m_MonoBehaviour.m_Name;
                            }
                            break;
                        case PlayerSettings m_PlayerSettings:
                            productName = m_PlayerSettings.productName;
                            break;
                        case AssetBundle m_AssetBundle:
                            foreach (var m_Container in m_AssetBundle.Container)
                            {
                                var preloadIndex = m_Container.Value.preloadIndex;
                                var preloadSize = m_Container.Value.preloadSize;
                                var preloadEnd = preloadIndex + preloadSize;
                                for (int k = preloadIndex; k < preloadEnd; k++)
                                {
                                    if (Game.Name == "GI" || Game.Name == "GI_CB2" || Game.Name == "GI_CB3")
                                    {
                                        if (long.TryParse(m_Container.Key, out var containerValue))
                                        {
                                            var last = unchecked((uint)containerValue);
                                            var path = ResourceIndex.GetBundlePath(last);
                                            if (!string.IsNullOrEmpty(path))
                                            {
                                                containers.Add((m_AssetBundle.PreloadTable[k], path));
                                                continue;
                                            }
                                        }
                                    }
                                    containers.Add((m_AssetBundle.PreloadTable[k], m_Container.Key));
                                }
                            }
                            assetItem.Text = m_AssetBundle.m_Name;
                            exportable = AssetBundle.Exportable;
                            break;
                        case IndexObject m_IndexObject:
                            assetItem.Text = "IndexObject";
                            exportable = IndexObject.Exportable;
                            break;
                        case ResourceManager m_ResourceManager:
                            foreach (var m_Container in m_ResourceManager.m_Container)
                            {
                                containers.Add((m_Container.Value, m_Container.Key));
                            }
                            break;
                        case MiHoYoBinData m_MiHoYoBinData:
                            if (m_MiHoYoBinData.assetsFile.ObjectsDic.TryGetValue(2, out var obj) && obj is IndexObject indexObject)
                            {
                                if (indexObject.Names.TryGetValue(m_MiHoYoBinData.m_PathID, out var binName))
                                {
                                    string path = "";
                                    var game = GameManager.GetGame("GI");
                                    if (Path.GetExtension(assetsFile.originalPath) == game.Extension)
                                    {
                                        var blkName = Path.GetFileNameWithoutExtension(assetsFile.originalPath);
                                        var blk = Convert.ToUInt64(blkName);
                                        var lastHex = Convert.ToUInt32(binName, 16);
                                        var blkHash = (blk << 32) | lastHex;
                                        var index = ResourceIndex.GetAssetIndex(blkHash);
                                        var bundleInfo = ResourceIndex.GetBundleInfo(index);
                                        path = bundleInfo != null ? bundleInfo.Path : "";
                                    }
                                    else
                                    {
                                        var last = Convert.ToUInt32(binName, 16);
                                        path = ResourceIndex.GetBundlePath(last) ?? "";
                                    }
                                    assetItem.Container = path;
                                    assetItem.Text = !string.IsNullOrEmpty(path) ? Path.GetFileName(path) : binName;
                                }
                            }
                            else assetItem.Text = string.Format("BinFile #{0}", assetItem.m_PathID);
                            exportable = IndexObject.Exportable;
                            break;
                        case NamedObject m_NamedObject:
                            assetItem.Text = m_NamedObject.m_Name;
                            exportable = true;
                            break;
                    }
                    if (assetItem.Text == "")
                    {
                        assetItem.Text = assetItem.TypeString + assetItem.UniqueID;
                    }
                    var isMatchRegex = nameFilters.Length == 0 || nameFilters.Any(x => x.IsMatch(assetItem.Text));
                    var isFilteredType = typeFilters.Length == 0 || typeFilters.Contains(assetItem.Asset.type);
                    if (isMatchRegex && isFilteredType && exportable)
                    {
                        exportableAssets.Add(assetItem);
                        i++;
                    }
                }
            }
            foreach ((var pptr, var container) in containers)
            {
                if (pptr.TryGet(out var obj))
                {
                    var item = objectAssetItemDic[obj];
                    if (containerFilters.Length == 0 || containerFilters.Any(x => x.IsMatch(container)))
                    {
                        item.Container = container;
                    }
                    else
                    {
                        exportableAssets.Remove(item);
                    }
                }
            }
            containers.Clear();
        }

        public static void ExportAssets(string savePath, List<AssetItem> toExportAssets, AssetGroupOption assetGroupOption)
        {
            int toExportCount = toExportAssets.Count;
            int exportedCount = 0;
            foreach (var asset in toExportAssets)
            {
                string exportPath;
                switch (assetGroupOption)
                {
                    case AssetGroupOption.ByType: //type name
                        exportPath = Path.Combine(savePath, asset.TypeString);
                        break;
                    case AssetGroupOption.ByContainer: //container path
                        if (!string.IsNullOrEmpty(asset.Container))
                        {
                            exportPath = Path.HasExtension(asset.Container) ? Path.Combine(savePath, Path.GetDirectoryName(asset.Container)) : Path.Combine(savePath, asset.Container);
                        }
                        else
                        {
                            exportPath = Path.Combine(savePath, asset.TypeString);
                        }
                        break;
                    case AssetGroupOption.BySource: //source file
                        if (string.IsNullOrEmpty(asset.SourceFile.originalPath))
                        {
                            exportPath = Path.Combine(savePath, asset.SourceFile.fileName + "_export");
                        }
                        else
                        {
                            exportPath = Path.Combine(savePath, Path.GetFileName(asset.SourceFile.originalPath) + "_export", asset.SourceFile.fileName);
                        }
                        break;
                    default:
                        exportPath = savePath;
                        break;
                }
                exportPath += Path.DirectorySeparatorChar;
                Logger.Info($"[{exportedCount}/{toExportCount}] Exporting {asset.TypeString}: {asset.Text}");
                try
                {
                    if (ExportConvertFile(asset, exportPath))
                    {
                        exportedCount++;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Export {asset.Type}:{asset.Text} error\r\n{ex.Message}\r\n{ex.StackTrace}");
                }
            }

            var statusText = exportedCount == 0 ? "Nothing exported." : $"Finished exporting {exportedCount} assets.";

            if (toExportCount > exportedCount)
            {
                statusText += $" {toExportCount - exportedCount} assets skipped (not extractable or files already exist)";
            }

            Logger.Info(statusText);
        }

        public static void ExportAssetsMap(string savePath, List<AssetEntry> toExportAssets, string exportListName, ExportListType exportListType)
        {
            string filename;
            switch (exportListType)
            {
                case ExportListType.XML:
                    filename = Path.Combine(savePath, $"{exportListName}.xml");
                    var doc = new XDocument(
                        new XElement("Assets",
                            new XAttribute("filename", filename),
                            new XAttribute("createdAt", DateTime.UtcNow.ToString("s")),
                            toExportAssets.Select(
                                asset => new XElement("Asset",
                                    new XElement("Name", asset.Name),
                                    new XElement("Container", asset.Container),
                                    new XElement("Type", new XAttribute("id", (int)asset.Type), asset.Type.ToString()),
                                    new XElement("PathID", asset.PathID),
                                    new XElement("Source", asset.Source)
                                )
                            )
                        )
                    );
                    doc.Save(filename);
                    break;
                case ExportListType.JSON:
                    filename = Path.Combine(savePath, $"{exportListName}.json");
                    using (StreamWriter file = File.CreateText(filename))
                    {
                        JsonSerializer serializer = new JsonSerializer() { Formatting = Formatting.Indented };
                        serializer.Converters.Add(new StringEnumConverter());
                        serializer.Serialize(file, toExportAssets);
                    }
                    break;
            }

            var statusText = $"Finished exporting asset list with {toExportAssets.Count()} items.";

            Logger.Info(statusText);

            Logger.Info($"AssetMap build successfully !!");
        }
    }
}
