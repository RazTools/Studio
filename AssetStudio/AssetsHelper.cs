using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Globalization;
using System.Xml.Linq;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace AssetStudio
{
    public static class AssetsHelper
    {
        public const string CABMapName = "Maps";

        public static CancellationTokenSource tokenSource = new CancellationTokenSource();
        public static AssetsManager assetsManager = new AssetsManager() { Silent = true, SkipProcess = true, ResolveDependencies = false };

        public static string BaseFolder = "";
        public static Dictionary<string, Entry> CABMap = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
        public static Dictionary<string, HashSet<long>> Offsets = new Dictionary<string, HashSet<long>>();

        public static void Clear()
        {
            CABMap.Clear();
            Offsets.Clear();
            BaseFolder = "";

            tokenSource.Dispose();
            tokenSource = new CancellationTokenSource();

            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        public static string[] GetMaps()
        {
            Directory.CreateDirectory(CABMapName);
            var files = Directory.GetFiles(CABMapName, "*.bin", SearchOption.TopDirectoryOnly);
            return files.Select(x => Path.GetFileNameWithoutExtension(x)).ToArray();
        }

        public static void BuildCABMap(string[] files, string mapName, string baseFolder, Game game)
        {
            Logger.Info($"Processing...");
            try
            {
                CABMap.Clear();
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
                                Logger.Info("Building CABMap has been aborted !!");
                                return;
                            }
                            var dependencies = assetsFile.m_Externals.Select(x => x.fileName).ToArray();
                            var entry = new Entry()
                            {
                                Path = relativePath,
                                Offset = assetsFile.offset,
                                Dependencies = dependencies
                            };

                            CABMap.TryAdd(assetsFile.fileName, new());
                            CABMap[assetsFile.fileName] = entry;
                        }
                        Logger.Info($"Processed {Path.GetFileName(file)}");
                    }
                    assetsManager.Clear();
                }

                CABMap = CABMap.OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
                var outputFile = new FileInfo(Path.Combine(CABMapName, $"{mapName}.bin"));

                outputFile.Directory.Create();

                using (var binaryFile = outputFile.Create())
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

                Logger.Info($"CABMap build successfully !!");
            }
            catch (Exception e)
            {
                Logger.Warning($"CABMap was not build, {e.Message}{e.StackTrace}");
            }
        }

        public static void LoadMap(string mapName)
        {
            Logger.Info($"Loading {mapName}");
            try
            {
                CABMap.Clear();
                using (var fs = File.OpenRead(Path.Combine(CABMapName, $"{mapName}.bin")))
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
                        CABMap.Add(cab, entry);
                    }
                }
                Logger.Info($"Loaded {mapName} !!");
            }
            catch (Exception e)
            {
                Logger.Warning($"{mapName} was not loaded, {e.Message}");
            }
        }

        public static AssetEntry[] BuildAssetMap(string[] files, Game game, ClassIDType[] typeFilters = null, Regex[] nameFilters = null, Regex[] containerFilters = null)
        {
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
                        assetsFile.m_Objects = ObjectInfo.Filter(assetsFile.m_Objects);

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
                                    exportable = MiHoYoBinData.Exportable;
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
                                default:
                                    asset.Name = objectReader.ReadAlignedString();
                                    break;
                            }
                            if (obj != null)
                            {
                                objectAssetItemDic.Add(obj, asset);
                                assetsFile.AddObject(obj);
                            }
                            var isMatchRegex = nameFilters.IsNullOrEmpty() || nameFilters.Any(x => x.IsMatch(asset.Name) || asset.Type == ClassIDType.Animator);
                            var isFilteredType = typeFilters.IsNullOrEmpty() || typeFilters.Contains(asset.Type) || asset.Type == ClassIDType.Animator;
                            if (isMatchRegex && isFilteredType && exportable)
                            {
                                assets.Add(asset);
                            }
                        }
                    }
                    foreach ((var pptr, var asset) in animators)
                    {
                        if (pptr.TryGet<GameObject>(out var gameObject) && (nameFilters.IsNullOrEmpty() || nameFilters.Any(x => x.IsMatch(gameObject.m_Name))) && (typeFilters.IsNullOrEmpty() || typeFilters.Contains(asset.Type)))
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
                    Logger.Info($"Processed {Path.GetFileName(file)}");
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
                        var path = ResourceIndex.GetAssetPath(last);
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
            return assets.ToArray();
        }

        public static void ExportAssetsMap(AssetEntry[] toExportAssets, string name, string savePath, ExportListType exportListType)
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
                        filename = Path.Combine(savePath, $"{name}.json");
                        using (StreamWriter file = File.CreateText(filename))
                        {
                            var serializer = new JsonSerializer() { Formatting = Formatting.Indented };
                            serializer.Converters.Add(new StringEnumConverter());
                            serializer.Serialize(file, toExportAssets);
                        }
                        break;
                }

                Logger.Info($"Finished exporting asset list with {toExportAssets.Length} items.");
                Logger.Info($"AssetMap build successfully !!");
            });
        }

        public static void AddCABOffsets(string[] path, List<string> cabs)
        {
            for (int i = 0; i < cabs.Count; i++)
            {
                var cab = cabs[i];
                if (CABMap.TryGetValue(cab, out var entry))
                {
                    if (!path.Contains(entry.Path))
                    {
                        var fullPath = Path.Combine(BaseFolder, entry.Path);
                        if (!Offsets.ContainsKey(fullPath))
                        {
                            Offsets.Add(fullPath, new HashSet<long>());
                        }
                        Offsets[fullPath].Add(entry.Offset);
                    }
                    foreach (var dep in entry.Dependencies)
                    {
                        if (!cabs.Contains(dep))
                            cabs.Add(dep);
                    }
                }
            }
        }
        
        public static bool FindCAB(string path, out List<string> cabs)
        {
            cabs = new List<string>();
            var relativePath = Path.GetRelativePath(BaseFolder, path);
            foreach (var kv in CABMap)
            {
                if (kv.Value.Path.Equals(relativePath))
                {
                    cabs.Add(kv.Key);
                }
            }
            return cabs.Count != 0;
        }
        
        public static string[] ProcessFiles(string[] files)
        {
            foreach (var file in files)
            {
                if (!Offsets.ContainsKey(file))
                {
                    Offsets.Add(file, new HashSet<long>());
                }
                if (FindCAB(file, out var cabs))
                {
                    AddCABOffsets(files, cabs);
                }
            }
            return Offsets.Keys.ToArray();
        }

        public static string[] ProcessDependencies(string[] files)
        {
            if (CABMap.Count == 0)
            {
                Logger.Warning("CABMap is not build, skip resolving dependencies...");
            }
            else
            {
                Logger.Info("Resolving Dependencies...");
                files = ProcessFiles(files);
            }
            return files;
        }
        public record Entry
        {
            public string Path { get; set; }
            public long Offset { get; set; }
            public string[] Dependencies { get; set; }
        }
    }
}
