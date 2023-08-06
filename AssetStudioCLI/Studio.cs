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
using System.Globalization;
using System.Xml;

namespace AssetStudioCLI
{
    [Flags]
    public enum MapOpType
    {
        None,
        Load,
        CABMap,
        AssetMap = 4,
        Both = 8,
        All = Both | Load,
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
        public static Game Game;
        public static bool ModelOnly = false;
        public static bool SkipContainer = false;
        public static AssetsManager assetsManager = new AssetsManager() { ResolveDependencies = false };
        public static AssemblyLoader assemblyLoader = new AssemblyLoader();
        public static List<AssetItem> exportableAssets = new List<AssetItem>();

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
            var reader = new FileReader(fileName);
            reader = reader.PreProcessing(Game);
            if (reader.FileType == FileType.BundleFile)
                extractedCount += ExtractBundleFile(reader, savePath);
            else if (reader.FileType == FileType.WebFile)
                extractedCount += ExtractWebDataFile(reader, savePath);
            else if (reader.FileType == FileType.BlkFile)
                extractedCount += ExtractBlkFile(reader, savePath);
            else if (reader.FileType == FileType.BlockFile)
                extractedCount += ExtractBlockFile(reader, savePath);
            else
                reader.Dispose();
            return extractedCount;
        }

        private static int ExtractBundleFile(FileReader reader, string savePath)
        {
            Logger.Info($"Decompressing {reader.FileName} ...");
            try
            {
                var bundleFile = new BundleFile(reader, Game);
                reader.Dispose();
                if (bundleFile.fileList.Length > 0)
                {
                    var extractPath = Path.Combine(savePath, reader.FileName + "_unpacked");
                    return ExtractStreamFile(extractPath, bundleFile.fileList);
                }
            }
            catch (InvalidCastException)
            {
                Logger.Error($"Game type mismatch, Expected {nameof(Mr0k)} but got {Game.Name} ({Game.GetType().Name}) !!");
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

        private static int ExtractBlkFile(FileReader reader, string savePath)
        {
            int total = 0;
            Logger.Info($"Decompressing {reader.FileName} ...");
            try
            {
                using var stream = BlkUtils.Decrypt(reader, (Blk)Game);
                do
                {
                    stream.Offset = stream.AbsolutePosition;
                    var dummyPath = Path.Combine(reader.FullPath, stream.AbsolutePosition.ToString("X8"));
                    var subReader = new FileReader(dummyPath, stream, true);
                    var subSavePath = Path.Combine(savePath, reader.FileName + "_unpacked");
                    switch (subReader.FileType)
                    {
                        case FileType.BundleFile:
                            total += ExtractBundleFile(subReader, subSavePath);
                            break;
                        case FileType.Mhy0File:
                            total += ExtractMhy0File(subReader, subSavePath);
                            break;
                    }
                } while (stream.Remaining > 0);
            }
            catch (InvalidCastException)
            {
                Logger.Error($"Game type mismatch, Expected {nameof(Blk)} but got {Game.Name} ({Game.GetType().Name}) !!");
            }
            return total;
        }

        private static int ExtractBlockFile(FileReader reader, string savePath)
        {
            int total = 0;
            Logger.Info($"Decompressing {reader.FileName} ...");
            using var stream = new OffsetStream(reader.BaseStream, 0);
            do
            {
                stream.Offset = stream.AbsolutePosition;
                var subSavePath = Path.Combine(savePath, reader.FileName + "_unpacked");
                var dummyPath = Path.Combine(reader.FullPath, stream.AbsolutePosition.ToString("X8"));
                var subReader = new FileReader(dummyPath, stream, true);
                total += ExtractBundleFile(subReader, subSavePath);
            } while (stream.Remaining > 0);
            return total;
        }

        private static int ExtractMhy0File(FileReader reader, string savePath)
        {
            Logger.Info($"Decompressing {reader.FileName} ...");
            try
            {
                var mhy0File = new Mhy0File(reader, reader.FullPath, (Mhy0)Game);
                reader.Dispose();
                if (mhy0File.fileList.Length > 0)
                {
                    var extractPath = Path.Combine(savePath, reader.FileName + "_unpacked");
                    return ExtractStreamFile(extractPath, mhy0File.fileList);
                }
            }
            catch (InvalidCastException)
            {
                Logger.Error($"Game type mismatch, Expected {nameof(Mhy0)} but got {Game.Name} ({Game.GetType().Name}) !!");
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

        public static void UpdateContainers()
        {
            if (exportableAssets.Count > 0)
            {
                Logger.Info("Updating Containers...");
                foreach (var asset in exportableAssets)
                {
                    if (int.TryParse(asset.Container, out var value))
                    {
                        var last = unchecked((uint)value);
                        var name = Path.GetFileNameWithoutExtension(asset.SourceFile.originalPath);
                        if (uint.TryParse(name, out var id))
                        {
                            var path = ResourceIndex.GetContainer(id, last);
                            if (!string.IsNullOrEmpty(path))
                            {
                                asset.Container = path;
                                if (asset.Type == ClassIDType.MiHoYoBinData)
                                {
                                    asset.Text = Path.GetFileNameWithoutExtension(path);
                                }
                            }
                        }
                    }
                }
                Logger.Info("Updated !!");
            }
        }

        public static void BuildAssetData(ClassIDType[] typeFilters, Regex[] nameFilters, Regex[] containerFilters, ref int i)
        {
            foreach (var assetsFile in assetsManager.assetsFileList)
            {
                foreach (var objInfo in assetsFile.m_Objects)
                {
                    var assetItem = new AssetItem(objInfo, assetsFile);
                    assetItem.UniqueID = "#" + i++;
                    var exportable = objInfo.type == ClassIDType.Texture2D && !ModelOnly
                        || objInfo.type == ClassIDType.AudioClip
                        || objInfo.type == ClassIDType.VideoClip
                        || objInfo.type == ClassIDType.Shader && !Properties.Settings.Default.disableShader
                        || objInfo.type == ClassIDType.Mesh
                        || objInfo.type == ClassIDType.TextAsset
                        || objInfo.type == ClassIDType.AnimationClip
                        || objInfo.type == ClassIDType.Font
                        || objInfo.type == ClassIDType.MovieTexture
                        || objInfo.type == ClassIDType.Sprite
                        || objInfo.type == ClassIDType.Animator && !ModelOnly
                        || objInfo.type == ClassIDType.MonoBehaviour
                        || objInfo.type == ClassIDType.MiHoYoBinData;
                    var isFilteredType = typeFilters.IsNullOrEmpty() || typeFilters.Contains(objInfo.type);
                    if (exportable && !isFilteredType) continue;
                    switch (objInfo.type)
                    {
                        case ClassIDType.Texture2D:
                            var m_Texture2D = assetsFile.ReadObject(objInfo) as Texture2D;
                            if (!string.IsNullOrEmpty(m_Texture2D.m_StreamData?.path))
                                assetItem.FullSize = objInfo.byteSize + m_Texture2D.m_StreamData.size;
                            goto case ClassIDType.NamedObject;
                        case ClassIDType.AudioClip:
                            var m_AudioClip = assetsFile.ReadObject(objInfo) as AudioClip;
                            if (!string.IsNullOrEmpty(m_AudioClip.m_Source))
                                assetItem.FullSize = objInfo.byteSize + m_AudioClip.m_Size;
                            goto case ClassIDType.NamedObject;
                        case ClassIDType.VideoClip:
                            var m_VideoClip = assetsFile.ReadObject(objInfo) as VideoClip;
                            if (!string.IsNullOrEmpty(m_VideoClip.m_OriginalPath))
                                assetItem.FullSize = objInfo.byteSize + (long)m_VideoClip.m_ExternalResources.m_Size;
                            goto case ClassIDType.NamedObject;
                        case ClassIDType.Shader:
                        case ClassIDType.Mesh:
                        case ClassIDType.TextAsset:
                        case ClassIDType.AnimationClip:
                        case ClassIDType.Font:
                        case ClassIDType.MovieTexture:
                        case ClassIDType.Sprite:
                        case ClassIDType.Animator:
                        case ClassIDType.MonoBehaviour:
                        case ClassIDType.MiHoYoBinData:
                        case ClassIDType.AssetBundle:
                        case ClassIDType.IndexObject:
                        case ClassIDType.GameObject:
                        case ClassIDType.PlayerSettings:
                        case ClassIDType.ResourceManager:
                        case ClassIDType.NamedObject:
                            assetItem.Text = assetsFile.ReadObjectName(objInfo, skipContainer: SkipContainer);
                            break;
                    }
                    if (assetItem.Text == "")
                    {
                        assetItem.Text = assetItem.TypeString + assetItem.UniqueID;
                    }
                    var isMatchRegex = nameFilters.IsNullOrEmpty() || nameFilters.Any(x => x.IsMatch(assetItem.Text));
                    if (isMatchRegex && exportable)
                    {
                        exportableAssets.Add(assetItem);
                    }
                }
            }
            if (!containerFilters.IsNullOrEmpty())
            {
                for (int j = 0; j < exportableAssets.Count; j++)
                {
                    var asset = exportableAssets[i];
                    if (containerFilters.Any(x => x.IsMatch(asset.Container)))
                    {
                        exportableAssets.Remove(asset);
                    }
                }
            }
            if (!SkipContainer)
            {
                if (Game.Type.IsGISubGroup())
                {
                    UpdateContainers();
                }
            }
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
                    var settings = new XmlWriterSettings() { Indent = true };
                    using (XmlWriter writer = XmlWriter.Create(filename, settings))
                    {
                        writer.WriteStartDocument();
                        writer.WriteStartElement("Assets");
                        writer.WriteAttributeString("filename", filename);
                        writer.WriteAttributeString("createdAt", DateTime.UtcNow.ToString("s"));
                        foreach (var asset in toExportAssets)
                        {
                            writer.WriteStartElement("Asset");
                            writer.WriteElementString("Name", asset.Name);
                            writer.WriteElementString("Container", asset.Container);
                            writer.WriteStartElement("Type");
                            writer.WriteAttributeString("id", ((int)asset.Type).ToString());
                            writer.WriteValue(asset.Type.ToString());
                            writer.WriteEndElement();
                            writer.WriteElementString("PathID", asset.PathID.ToString());
                            writer.WriteElementString("Source", asset.Source);
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                        writer.WriteEndDocument();
                    }
                    break;
                case ExportListType.JSON:
                    filename = Path.Combine(savePath, $"{exportListName}.json");
                    using (StreamWriter file = File.CreateText(filename))
                    {
                        JsonSerializer serializer = new JsonSerializer() { Formatting = Newtonsoft.Json.Formatting.Indented };
                        serializer.Converters.Add(new StringEnumConverter());
                        serializer.Serialize(file, toExportAssets);
                    }
                    break;
            }

            var statusText = $"Finished exporting asset list with {toExportAssets.Count()} items.";

            Logger.Info(statusText);

            Logger.Info($"AssetMap build successfully !!");
        }

        public static TypeTree MonoBehaviourToTypeTree(MonoBehaviour m_MonoBehaviour)
        {
            return m_MonoBehaviour.ConvertToTypeTree(assemblyLoader);
        }
    }
}
