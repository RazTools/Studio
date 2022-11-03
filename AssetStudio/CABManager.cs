using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace AssetStudio
{
    public static class CABManager
    {
        public static Dictionary<string, Entry> CABMap = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
        public static Dictionary<string, HashSet<long>> offsets = new Dictionary<string, HashSet<long>>();

        public static void BuildMap(List<string> files, Game game)
        {
            Logger.Info($"Building {game.Name}Map");
            try
            {
                int collisions = 0;
                CABMap.Clear();
                Progress.Reset();
                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    var reader = new FileReader(file, game);
                    var gameFile = new GameFile(reader);
                    reader.Dispose();
                    foreach (var bundle in gameFile.Bundles)
                    {
                        foreach (var cab in bundle.Value)
                        {
                            var dummyPath = Path.Combine(Path.GetDirectoryName(reader.FullPath), cab.fileName);
                            using (var cabReader = new FileReader(dummyPath, cab.stream))
                            {
                                if (cabReader.FileType == FileType.AssetsFile)
                                {
                                    if (CABMap.ContainsKey(cab.path))
                                    {
                                        collisions++;
                                        continue;
                                    }
                                    var assetsFile = new SerializedFile(cabReader, null, reader.FullPath);
                                    var dependencies = assetsFile.m_Externals.Select(x => x.fileName).ToList();
                                    CABMap.Add(cab.path, new Entry(file, bundle.Key, dependencies));
                                }
                            }
                        }
                    }
                    Logger.Info($"[{i + 1}/{files.Count}] Processed {Path.GetFileName(file)}");
                    Progress.Report(i + 1, files.Count);
                }

                CABMap = CABMap.OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
                var outputFile = new FileInfo($"Maps/{game.Name}Map.bin");

                if (!outputFile.Directory.Exists)
                    outputFile.Directory.Create();

                using (var binaryFile = outputFile.Create())
                using (var writer = new BinaryWriter(binaryFile))
                {
                    writer.Write(CABMap.Count);
                    foreach (var cab in CABMap)
                    {
                        writer.Write(cab.Key);
                        writer.Write(cab.Value.Path);
                        writer.Write(cab.Value.Offset);
                        writer.Write(cab.Value.Dependencies.Count);
                        foreach (var dependancy in cab.Value.Dependencies)
                        {
                            writer.Write(dependancy);
                        }
                    }
                }
                Logger.Info($"{game.Name}Map build successfully, {collisions} collisions found !!");
            }
            catch (Exception e)
            {
                Logger.Warning($"{game.Name}Map was not build, {e.Message}");
            }
        }
        public static void LoadMap(Game game)
        {
            Logger.Info($"Loading {game.Name}Map");
            try
            {
                CABMap.Clear();
                using (var binaryFile = File.OpenRead($"Maps/{game.Name}Map.bin"))
                using (var reader = new BinaryReader(binaryFile))
                {
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
                        CABMap.Add(cab, new Entry(path, offset, dependencies));
                    }
                }
                Logger.Info($"Loaded {game.Name}Map !!");
            }
            catch (Exception e)
            {
                Logger.Warning($"{game.Name}Map was not loaded, {e.Message}");
            }
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
                        if (!offsets.ContainsKey(entry.Path))
                        {
                            offsets.Add(entry.Path, new HashSet<long>());
                        }
                        offsets[entry.Path].Add(entry.Offset);
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
            cabs = CABMap.Where(x => x.Value.Path.Contains(path)).Select(x => x.Key).ToList();
            return cabs.Count != 0;
        }

        public static string[] ProcessFiles(string[] files)
        {
            foreach (var file in files)
            {
                if (!offsets.ContainsKey(file))
                {
                    offsets.Add(file, new HashSet<long>());
                }
                if (FindCAB(file, out var cabs))
                {
                    AddCABOffsets(files, cabs);
                }
            }
            return offsets.Keys.ToArray();
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
                var file = files.FirstOrDefault();
                var supportedExtensions = GameManager.GetGames().Select(x => x.Extension).ToList();
                if (supportedExtensions.Contains(Path.GetExtension(file)))
                {
                    files = ProcessFiles(files);
                }
            }
            return files;
        }
    }

    public class Entry : IComparable<Entry>
    {
         public string Path;
         public long Offset;
         public List<string> Dependencies;
         public Entry(string path, long offset, List<string> dependencies)
         {
             Path = path;
             Offset = offset;
             Dependencies = dependencies;
         }
         public int CompareTo(Entry other)
         {
             if (other == null) return 1;

             int result;
             if (other == null)
                 throw new ArgumentException("Object is not an Entry");

             result = Path.CompareTo(other.Path);

             if (result == 0)
                 result = Offset.CompareTo(other.Offset);

             return result;
         }
    }
}
