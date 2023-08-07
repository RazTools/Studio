using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Globalization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Text;
using MessagePack;

namespace AssetStudio
{
    public static class AssetsHelper
    {
        public const string MapName = "Maps";

        public static bool Minimal = true;
        public static CancellationTokenSource tokenSource = new CancellationTokenSource();

        private static string BaseFolder = "";
        private static Dictionary<string, Entry> CABMap = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, HashSet<long>> Offsets = new Dictionary<string, HashSet<long>>();
        private static AssetsManager assetsManager = new AssetsManager() { SkipProcess = true, ResolveDependencies = false };

        public record Entry
        {
            public string Path { get; set; }
            public long Offset { get; set; }
            public string[] Dependencies { get; set; }
        }

        public static string[] GetMaps()
        {
            Directory.CreateDirectory(MapName);
            var files = Directory.GetFiles(MapName, "*.bin", SearchOption.TopDirectoryOnly);
            return files.Select(Path.GetFileNameWithoutExtension).ToArray();
        }

        public static void Clear()
        {
            CABMap.Clear();
            Offsets.Clear();
            assetsManager.Clear();
            BaseFolder = string.Empty;

            tokenSource.Dispose();
            tokenSource = new CancellationTokenSource();

            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        public static void ClearOffsets() => Offsets.Clear();

        public static void Remove(string path) => Offsets.Remove(path);

        public static bool TryAdd(string name, out string path)
        {
            if (CABMap.TryGetValue(name, out var entry))
            {
                path = Path.Combine(BaseFolder, entry.Path);
                if (!Offsets.ContainsKey(path))
                {
                    Offsets.Add(path, new HashSet<long>());
                }
                Offsets[path].Add(entry.Offset);
                return true;
            }
            path = string.Empty;
            return false;
        }

        public static bool TryGet(string path, out long[] offsets)
        {
            if (Offsets.TryGetValue(path, out var list))
            {
                offsets = list.ToArray();
                return true;
            }
            offsets = Array.Empty<long>();
            return false;
        }

        public static void BuildCABMap(string[] files, string mapName, string baseFolder, Game game)
        {
            Logger.Info("Building CABMap...");
            try
            {
                CABMap.Clear();
                Progress.Reset();
                var collision = 0;
                BaseFolder = baseFolder;
                assetsManager.Game = game;
                assetsManager.LoadFiles(files);
                BuildCABMap(ref collision);
                DumpCABMap(mapName);
                Logger.Info($"CABMap build successfully !! {collision} collisions found");
                assetsManager.Clear();
            }
            catch (Exception e)
            {
                Logger.Warning($"CABMap was not build, {e}");
            }
        }

        private static void BuildCABMap(ref int collision)
        {
            Logger.Info("Building CABMap...");
            foreach (var assetsFile in assetsManager.assetsFileList)
            {
                var relativePath = Path.GetRelativePath(BaseFolder, assetsFile.originalPath);
                if (tokenSource.IsCancellationRequested)
                {
                    Logger.Info("Building CABMap has been cancelled !!");
                    return;
                }
                var entry = new Entry()
                {
                    Path = relativePath,
                    Offset = assetsFile.offset,
                    Dependencies = assetsFile.m_Externals.Select(x => x.fileName).ToArray()
                };

                if (CABMap.ContainsKey(assetsFile.fileName))
                {
                    collision++;
                    continue;
                }
                CABMap.Add(assetsFile.fileName, entry);
            }
        }

        private static void DumpCABMap(string mapName)
        {
            CABMap = CABMap.OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
            var outputFile = Path.Combine(MapName, $"{mapName}.bin");

            Directory.CreateDirectory(Path.GetDirectoryName(outputFile));

            using (var binaryFile = File.OpenWrite(outputFile))
            using (var writer = new BinaryWriter(binaryFile))
            {
                writer.Write(BaseFolder);
                writer.Write(CABMap.Count);
                foreach (var kv in CABMap)
                {
                    writer.Write(kv.Key);
                    writer.Write(kv.Value.Path);
                    writer.Write(kv.Value.Offset);
                    writer.Write(kv.Value.Dependencies.Length);
                    foreach (var cab in kv.Value.Dependencies)
                    {
                        writer.Write(cab);
                    }
                }
            }
        }

        public static void LoadCABMap(string mapName)
        {
            Logger.Info($"Loading {mapName}");
            try
            {
                CABMap.Clear();
                using (var fs = File.OpenRead(Path.Combine(MapName, $"{mapName}.bin")))
                using (var reader = new BinaryReader(fs))
                {
                    BaseFolder = reader.ReadString();
                    var count = reader.ReadInt32();
                    for (int i = 0; i < count; i++)
                    {
                        var cab = reader.ReadString();
                        var path = reader.ReadString();
                        var offset = reader.ReadInt64();
                        var depCount = reader.ReadInt32();
                        var dependencies = new string[depCount];
                        for (int j = 0; j < depCount; j++)
                        {
                            dependencies[j] = reader.ReadString();
                        }
                        var entry = new Entry()
                        {
                            Path = path,
                            Offset = offset,
                            Dependencies = dependencies
                        };
                        CABMap.Add(cab, entry);
                    }
                }
                Logger.Info($"Loaded {mapName} !!");
            }
            catch (Exception e)
            {
                Logger.Warning($"{mapName} was not loaded, {e}"); 
            }
        }

        public static void BuildAssetMap(string[] files, string mapName, Game game, string savePath, ExportListType exportListType, ManualResetEvent resetEvent = null, ClassIDType[] typeFilters = null, Regex[] nameFilters = null, Regex[] containerFilters = null)
        {
            Logger.Info("Building AssetMap...");
            try
            {
                Progress.Reset();
                assetsManager.Game = game;
                var assets = new List<AssetEntry>();
                assetsManager.LoadFiles(files);
                BuildAssetMap(assets, typeFilters, nameFilters, containerFilters);
                UpdateContainers(assets, game);

                ExportAssetsMap(assets.ToArray(), game, mapName, savePath, exportListType, resetEvent);
                assetsManager.Clear();
            }
            catch(Exception e)
            {
                Logger.Warning($"AssetMap was not build, {e}");
            }
            
        }

        private static void BuildAssetMap(List<AssetEntry> assets, ClassIDType[] typeFilters = null, Regex[] nameFilters = null, Regex[] containerFilters = null)
        {
            Logger.Info("Building AssetMap...");
            foreach (var assetsFile in assetsManager.assetsFileList)
            {
                foreach (var objInfo in assetsFile.m_Objects)
                {
                    if (tokenSource.IsCancellationRequested)
                    {
                        Logger.Info("Building AssetMap has been cancelled !!");
                        return;
                    }
                    var asset = new AssetEntry()
                    {
                        ObjInfo = objInfo,
                        Source = assetsFile.originalPath,
                        PathID = objInfo.m_PathID,
                        Type = objInfo.type,
                    };

                    var exportable = false;
                    try
                    {
                        switch (objInfo.type)
                        {
                            case ClassIDType.Font:
                            case ClassIDType.Material:
                            case ClassIDType.Texture:
                            case ClassIDType.Mesh:
                            case ClassIDType.Sprite:
                            case ClassIDType.TextAsset:
                            case ClassIDType.Texture2D:
                            case ClassIDType.VideoClip:
                            case ClassIDType.AudioClip:
                            case ClassIDType.Animator:
                            case ClassIDType.AnimationClip:
                            case ClassIDType.MiHoYoBinData:
                            case ClassIDType.Shader when Shader.Parsable:
                                asset.Name = assetsFile.ReadObjectName(objInfo);
                                exportable = true;
                                break;
                            default:
                                asset.Name = assetsFile.ReadObjectName(objInfo);
                                exportable = !Minimal;
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("Unable to load object")
                            .AppendLine($"Assets {assetsFile.fileName}")
                            .AppendLine($"Path {assetsFile.originalPath}")
                            .AppendLine($"Type {objInfo.type}")
                            .AppendLine($"PathID {objInfo.m_PathID}")
                            .Append(e);
                        Logger.Error(sb.ToString());
                    }
                    var isNaneMatchRegex = nameFilters.IsNullOrEmpty() || nameFilters.Any(x => x.IsMatch(asset.Name));
                    var isFilteredType = typeFilters.IsNullOrEmpty() || typeFilters.Contains(asset.Type);
                    var isContainerMatchRegex = containerFilters.IsNullOrEmpty() || containerFilters.Any(x => x.IsMatch(asset.Container));
                    if (isNaneMatchRegex && isContainerMatchRegex && isFilteredType && exportable)
                    {
                        assets.Add(asset);
                    }
                }

                assetsManager.RemoveReader(assetsFile.fileName);
            }
        }

        private static void UpdateContainers(List<AssetEntry> assets, Game game)
        {
            if (game.Type.IsGISubGroup() && assets.Count > 0)
            {
                Logger.Info("Updating Containers...");
                foreach (var asset in assets)
                {
                    if (int.TryParse(asset.Container, out var value))
                    {
                        var last = unchecked((uint)value);
                        var name = Path.GetFileNameWithoutExtension(asset.Source);
                        if (uint.TryParse(name, out var id))
                        {
                            var path = ResourceIndex.GetContainer(id, last);
                            if (!string.IsNullOrEmpty(path))
                            {
                                asset.Container = path;
                                if (asset.Type == ClassIDType.MiHoYoBinData)
                                {
                                    asset.Name = Path.GetFileNameWithoutExtension(path);
                                }
                            }
                        }
                    }
                }
                Logger.Info("Updated !!");
            }
        }

        private static void ExportAssetsMap(AssetEntry[] toExportAssets, Game game, string name, string savePath, ExportListType exportListType, ManualResetEvent resetEvent = null)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

                Progress.Reset();

                string filename = Path.Combine(savePath, $"{name}{exportListType.GetExtension()}");
                switch (exportListType)
                {
                    case ExportListType.XML:
                        var xmlSettings = new XmlWriterSettings() { Indent = true };
                        using (XmlWriter writer = XmlWriter.Create(filename, xmlSettings))
                        {
                            writer.WriteStartDocument();
                            writer.WriteStartElement("Assets");
                            writer.WriteAttributeString("filename", filename);
                            writer.WriteAttributeString("createdAt", DateTime.UtcNow.ToString("s"));
                            foreach(var asset in toExportAssets)
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
                        using (StreamWriter file = File.CreateText(filename))
                        {
                            var serializer = new JsonSerializer() { Formatting = Newtonsoft.Json.Formatting.Indented };
                            serializer.Converters.Add(new StringEnumConverter());
                            serializer.Serialize(file, toExportAssets);
                        }
                        break;
                    case ExportListType.MessagePack:
                        using (var file = File.Create(filename))
                        {
                            var assetMap = new AssetMap
                            {
                                GameType = game.Type,
                                AssetEntries = toExportAssets
                            };
                            MessagePackSerializer.Serialize(file, assetMap, MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray));
                        }
                        break;
                }

                Logger.Info($"Finished buidling AssetMap with {toExportAssets.Length} assets.");

                resetEvent?.Set();
            });
        }
        public static void BuildBoth(string[] files, string mapName, string baseFolder, Game game, string savePath, ExportListType exportListType, ManualResetEvent resetEvent = null, ClassIDType[] typeFilters = null, Regex[] nameFilters = null, Regex[] containerFilters = null)
        {
            Logger.Info($"Building Both...");
            CABMap.Clear();
            Progress.Reset();
            var collision = 0;
            BaseFolder = baseFolder;
            assetsManager.Game = game;
            var assets = new List<AssetEntry>();
            assetsManager.LoadFiles(files);
            BuildCABMap(ref collision);
            BuildAssetMap(assets, typeFilters, nameFilters, containerFilters);

            UpdateContainers(assets, game);
            DumpCABMap(mapName);

            Logger.Info($"Map build successfully !! {collision} collisions found");
            ExportAssetsMap(assets.ToArray(), game, mapName, savePath, exportListType, resetEvent);
            assetsManager.Clear();
        }
    }
}
