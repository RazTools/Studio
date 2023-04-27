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

namespace AssetStudio
{
    public static class AssetsHelper
    {
        public const string MapName = "Maps";

        public static CancellationTokenSource tokenSource = new CancellationTokenSource();

        private static string BaseFolder = "";
        private static Dictionary<string, Entry> Map = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, HashSet<long>> Offsets = new Dictionary<string, HashSet<long>>();
        private static AssetsManager assetsManager = new AssetsManager() { Silent = true, SkipProcess = true, ResolveDependencies = false };

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
            return files.Select(x => Path.GetFileNameWithoutExtension(x)).ToArray();
        }

        public static void Clear()
        {
            Map.Clear();
            Offsets.Clear();
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
            if (Map.TryGetValue(name, out var entry))
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

        public static void BuildMap(string[] files, string mapName, string baseFolder, Game game)
        {
            Logger.Info($"Building Map...");
            try
            {
                Map.Clear();
                Progress.Reset();
                var collision = 0;
                BaseFolder = baseFolder;
                assetsManager.Game = game;
                for (int i = 0; i < files.Length; i++)
                {
                    var file = files[i];
                    assetsManager.LoadFiles(file);
                    if (assetsManager.assetsFileList.Count > 0)
                    {
                        var relativePath = Path.GetRelativePath(BaseFolder, file);
                        foreach (var assetsFile in assetsManager.assetsFileList)
                        {
                            if (tokenSource.IsCancellationRequested)
                            {
                                Logger.Info("Building Map has been aborted !!");
                                return;
                            }
                            var dependencies = assetsFile.m_Externals.Select(x => x.fileName).ToArray();
                            var entry = new Entry()
                            {
                                Path = relativePath,
                                Offset = assetsFile.offset,
                                Dependencies = dependencies
                            };

                            if (Map.ContainsKey(assetsFile.fileName))
                            {
                                collision++;
                                continue;
                            }
                            Map.Add(assetsFile.fileName, entry);
                        }
                        Logger.Info($"[{i + 1}/{files.Length}] Processed {Path.GetFileName(file)}");
                        Progress.Report(i + 1, files.Length);
                    }
                    assetsManager.Clear();
                }

                Map = Map.OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
                var outputFile = Path.Combine(MapName, $"{mapName}.bin");

                Directory.CreateDirectory(Path.GetDirectoryName(outputFile));

                using (var binaryFile = File.OpenWrite(outputFile))
                using (var writer = new BinaryWriter(binaryFile))
                {
                    writer.Write(BaseFolder);
                    writer.Write(Map.Count);
                    foreach (var kv in Map)
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

                Logger.Info($"Map build successfully !! {collision} collisions found");
            }
            catch (Exception e)
            {
                Logger.Warning($"Map was not build, {e}");
            }
        }

        public static void BuildBoth(string[] files, string mapName, string baseFolder, Game game, string savePath, ExportListType exportListType, ManualResetEvent resetEvent = null, Regex[] nameFilters = null, Regex[] containerFilters = null)
        {
            Logger.Info($"Building Both...");
            Map.Clear();
            Progress.Reset();
            var collision = 0;
            BaseFolder = baseFolder;
            assetsManager.Game = game;
            var assets = new List<AssetEntry>();
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                assetsManager.LoadFiles(file);
                if (assetsManager.assetsFileList.Count > 0)
                {
                    var relativePath = Path.GetRelativePath(BaseFolder, file);
                    var containers = new List<(PPtr<Object>, string)>();
                    var mihoyoBinDataNames = new List<(PPtr<Object>, string)>();
                    var objectAssetItemDic = new Dictionary<Object, AssetEntry>();
                    var animators = new List<(PPtr<Object>, AssetEntry)>();
                    foreach (var assetsFile in assetsManager.assetsFileList)
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            Logger.Info("Building Map has been aborted !!");
                            return;
                        }
                        var dependencies = assetsFile.m_Externals.Select(x => x.fileName).ToArray();
                        var entry = new Entry()
                        {
                            Path = relativePath,
                            Offset = assetsFile.offset,
                            Dependencies = dependencies
                        };

                        if (Map.ContainsKey(assetsFile.fileName))
                        {
                            collision++;
                            continue;
                        }
                        Map.Add(assetsFile.fileName, entry);

                        foreach (var objInfo in assetsFile.m_Objects)
                        {
                            var objectReader = new ObjectReader(assetsFile.reader, assetsFile, objInfo, game);
                            var obj = new Object(objectReader);
                            var asset = new AssetEntry()
                            {
                                Source = file,
                                PathID = objectReader.m_PathID,
                                Type = objectReader.type,
                                Container = string.Empty,
                                Name = string.Empty
                            };

                            var exportable = true;
                            try
                            {
                                switch (objectReader.type)
                                {
                                    case ClassIDType.AssetBundle:
                                        var assetBundle = new AssetBundle(objectReader);
                                        foreach (var m_Container in assetBundle.m_Container)
                                        {
                                            var preloadIndex = m_Container.Value.preloadIndex;
                                            var preloadSize = m_Container.Value.preloadSize;
                                            var preloadEnd = preloadIndex + preloadSize;
                                            for (int k = preloadIndex; k < preloadEnd; k++)
                                            {
                                                containers.Add((assetBundle.m_PreloadTable[k], m_Container.Key));
                                            }
                                        }
                                        obj = null;
                                        asset.Name = assetBundle.m_Name;
                                        exportable = false;
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
                                        var component = new PPtr<Object>(objectReader);
                                        animators.Add((component, asset));
                                        break;
                                    case ClassIDType.MiHoYoBinData:
                                        var MiHoYoBinData = new MiHoYoBinData(objectReader);
                                        obj = MiHoYoBinData;
                                        exportable = true;
                                        break;
                                    case ClassIDType.IndexObject:
                                        var indexObject = new IndexObject(objectReader);
                                        obj = null;
                                        foreach (var index in indexObject.AssetMap)
                                        {
                                            mihoyoBinDataNames.Add((index.Value.Object, index.Key));
                                        }
                                        asset.Name = "IndexObject";
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
                                        asset.Name = objectReader.ReadAlignedString();
                                        break;
                                    default:
                                        exportable = false;
                                        break;
                                }
                            }
                            catch (Exception e)
                            {
                                var sb = new StringBuilder();
                                sb.AppendLine("Unable to load object")
                                    .AppendLine($"Assets {assetsFile.fileName}")
                                    .AppendLine($"Path {assetsFile.originalPath}")
                                    .AppendLine($"Type {objectReader.type}")
                                    .AppendLine($"PathID {objectReader.m_PathID}")
                                    .Append(e);
                                Logger.Error(sb.ToString());
                            }
                            if (obj != null)
                            {
                                objectAssetItemDic.Add(obj, asset);
                                assetsFile.AddObject(obj);
                            }
                            var isMatchRegex = nameFilters.IsNullOrEmpty() || nameFilters.Any(x => x.IsMatch(asset.Name) || asset.Type == ClassIDType.Animator);
                            if (isMatchRegex && exportable)
                            {
                                assets.Add(asset);
                            }
                        }
                    }
                    foreach ((var pptr, var asset) in animators)
                    {
                        if (pptr.TryGet<GameObject>(out var gameObject) && (nameFilters.IsNullOrEmpty() || nameFilters.Any(x => x.IsMatch(gameObject.m_Name))))
                        {
                            asset.Name = gameObject.m_Name;
                        }
                    }
                    foreach ((var pptr, var name) in mihoyoBinDataNames)
                    {
                        if (pptr.TryGet<MiHoYoBinData>(out var miHoYoBinData))
                        {
                            var asset = objectAssetItemDic[miHoYoBinData];
                            if (int.TryParse(name, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hash))
                            {
                                asset.Name = name;
                                asset.Container = hash.ToString();
                            }
                            else asset.Name = $"BinFile #{asset.PathID}";
                        }
                    }
                    foreach ((var pptr, var container) in containers)
                    {
                        if (pptr.TryGet(out var obj))
                        {
                            var item = objectAssetItemDic[obj];
                            if (containerFilters.IsNullOrEmpty() || containerFilters.Any(x => x.IsMatch(container)))
                            {
                                item.Container = container;
                            }
                            else
                            {
                                assets.Remove(item);
                            }
                        }
                    }
                    Logger.Info($"[{i + 1}/{files.Length}] Processed {Path.GetFileName(file)}");
                    Progress.Report(i + 1, files.Length);
                }
                assetsManager.Clear();
            }
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

            Map = Map.OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
            var outputFile = Path.Combine(MapName, $"{mapName}.bin");

            Directory.CreateDirectory(Path.GetDirectoryName(outputFile));

            using (var binaryFile = File.OpenWrite(outputFile))
            using (var writer = new BinaryWriter(binaryFile))
            {
                writer.Write(BaseFolder);
                writer.Write(Map.Count);
                foreach (var kv in Map)
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

            Logger.Info($"Map build successfully !! {collision} collisions found");
            ExportAssetsMap(assets.ToArray(), mapName, savePath, exportListType, resetEvent);
        }

        public static bool LoadMap(string mapName)
        {
            Logger.Info($"Loading {mapName}");
            try
            {
                Map.Clear();
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
                        var dependencies = new List<string>();
                        for (int j = 0; j < depCount; j++)
                        {
                            var dependancy = reader.ReadString();
                            dependencies.Add(dependancy);
                        }
                        var entry = new Entry()
                        {
                            Path = path,
                            Offset = offset,
                            Dependencies = dependencies.ToArray()
                        };
                        Map.Add(cab, entry);
                    }
                }
                Logger.Info($"Loaded {mapName} !!");
                return true;
            }
            catch (Exception e)
            {
                Logger.Warning($"{mapName} was not loaded, {e}"); 
            }

            return false;
        }

        public static AssetEntry[] BuildAssetMap(string[] files, Game game, Regex[] nameFilters = null, Regex[] containerFilters = null)
        {
            Progress.Reset();
            assetsManager.Game = game;
            var assets = new List<AssetEntry>();
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                assetsManager.LoadFiles(file);
                if (assetsManager.assetsFileList.Count > 0)
                {
                    var containers = new List<(PPtr<Object>, string)>();
                    var mihoyoBinDataNames = new List<(PPtr<Object>, string)>();
                    var objectAssetItemDic = new Dictionary<Object, AssetEntry>();
                    var animators = new List<(PPtr<Object>, AssetEntry)>();
                    foreach (var assetsFile in assetsManager.assetsFileList)
                    {
                        foreach (var objInfo in assetsFile.m_Objects)
                        {
                            var objectReader = new ObjectReader(assetsFile.reader, assetsFile, objInfo, game);
                            var obj = new Object(objectReader);
                            var asset = new AssetEntry()
                            {
                                Source = file,
                                PathID = objectReader.m_PathID,
                                Type = objectReader.type,
                                Container = ""
                            };
                        
                            var exportable = true;
                            try
                            {
                                switch (objectReader.type)
                                {
                                    case ClassIDType.AssetBundle:
                                        var assetBundle = new AssetBundle(objectReader);
                                        foreach (var m_Container in assetBundle.m_Container)
                                        {
                                            var preloadIndex = m_Container.Value.preloadIndex;
                                            var preloadSize = m_Container.Value.preloadSize;
                                            var preloadEnd = preloadIndex + preloadSize;
                                            for (int k = preloadIndex; k < preloadEnd; k++)
                                            {
                                                containers.Add((assetBundle.m_PreloadTable[k], m_Container.Key));
                                            }
                                        }
                                        obj = null;
                                        asset.Name = assetBundle.m_Name;
                                        exportable = false;
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
                                        var component = new PPtr<Object>(objectReader);
                                        animators.Add((component, asset));
                                        break;
                                    case ClassIDType.MiHoYoBinData:
                                        var MiHoYoBinData = new MiHoYoBinData(objectReader);
                                        obj = MiHoYoBinData;
                                        exportable = true;
                                        break;
                                    case ClassIDType.IndexObject:
                                        var indexObject = new IndexObject(objectReader);
                                        obj = null;
                                        foreach (var index in indexObject.AssetMap)
                                        {
                                            mihoyoBinDataNames.Add((index.Value.Object, index.Key));
                                        }
                                        asset.Name = "IndexObject";
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
                                        asset.Name = objectReader.ReadAlignedString();
                                        break;
                                    default:
                                        exportable = false;
                                        break;
                                }
                            }
                            catch (Exception e)
                            {
                                var sb = new StringBuilder();
                                sb.AppendLine("Unable to load object")
                                    .AppendLine($"Assets {assetsFile.fileName}")
                                    .AppendLine($"Path {assetsFile.originalPath}")
                                    .AppendLine($"Type {objectReader.type}")
                                    .AppendLine($"PathID {objectReader.m_PathID}")
                                    .Append(e);
                                Logger.Error(sb.ToString());
                            }
                            if (obj != null)
                            {
                                objectAssetItemDic.Add(obj, asset);
                                assetsFile.AddObject(obj);
                            }
                            var isMatchRegex = nameFilters.IsNullOrEmpty() || nameFilters.Any(x => x.IsMatch(asset.Name) || asset.Type == ClassIDType.Animator);
                            if (isMatchRegex && exportable)
                            {
                                assets.Add(asset);
                            }
                        }
                    }
                    foreach ((var pptr, var asset) in animators)
                    {
                        if (pptr.TryGet<GameObject>(out var gameObject) && (nameFilters.IsNullOrEmpty() || nameFilters.Any(x => x.IsMatch(gameObject.m_Name))))
                        {
                            asset.Name = gameObject.m_Name;
                        }
                    }
                    foreach((var pptr, var name) in mihoyoBinDataNames)
                    {
                        if (pptr.TryGet<MiHoYoBinData>(out var miHoYoBinData))
                        {
                            var asset = objectAssetItemDic[miHoYoBinData];
                            if (int.TryParse(name, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hash))
                            {
                                asset.Name = name;
                                asset.Container = hash.ToString();
                            }
                            else asset.Name = $"BinFile #{asset.PathID}";
                        }
                    }
                    foreach ((var pptr, var container) in containers)
                    {
                        if (pptr.TryGet(out var obj))
                        {
                            var item = objectAssetItemDic[obj];
                            if (containerFilters.IsNullOrEmpty() || containerFilters.Any(x => x.IsMatch(container)))
                            {
                                item.Container = container;
                            }
                            else
                            {
                                assets.Remove(item);
                            }
                        }
                    }
                    Logger.Info($"[{i + 1}/{files.Length}] Processed {Path.GetFileName(file)}");
                    Progress.Report(i + 1, files.Length);
                }
                assetsManager.Clear();
            }
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
                    Logger.Info("Updated !!");
                }
            }
            return assets.ToArray();
        }

        public static void ExportAssetsMap(AssetEntry[] toExportAssets, string name, string savePath, ExportListType exportListType, ManualResetEvent resetEvent = null)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

                Progress.Reset();

                string filename;
                switch (exportListType)
                {
                    case ExportListType.XML:
                        filename = Path.Combine(savePath, $"{name}.xml");
                        var settings = new XmlWriterSettings() { Indent = true };
                        using (XmlWriter writer = XmlWriter.Create(filename, settings))
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
                        filename = Path.Combine(savePath, $"{name}.json");
                        using (StreamWriter file = File.CreateText(filename))
                        {
                            var serializer = new JsonSerializer() { Formatting = Newtonsoft.Json.Formatting.Indented };
                            serializer.Converters.Add(new StringEnumConverter());
                            serializer.Serialize(file, toExportAssets);
                        }
                        break;
                }

                Logger.Info($"Finished exporting asset list with {toExportAssets.Length} items.");
                Logger.Info($"AssetMap build successfully !!");

                resetEvent?.Set();
            });
        }
    }
}
