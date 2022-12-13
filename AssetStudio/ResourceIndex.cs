using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public static class ResourceIndex
    {
        public static Dictionary<int, List<int>> BundleDependencyMap;
        public static Dictionary<int, Block> BlockInfoMap;
        public static Dictionary<int, byte> BlockMap;
        public static Dictionary<ulong, ulong> AssetMap;
        public static List<Dictionary<uint, BundleInfo>> AssetLocationMap;
        public static List<int> BlockSortList;

        static ResourceIndex()
        {
            BlockSortList = new List<int>();
            AssetMap = new Dictionary<ulong, ulong>();
            AssetLocationMap = new List<Dictionary<uint, BundleInfo>>(0x100);
            for (int i = 0; i < AssetLocationMap.Capacity; i++)
            {
                AssetLocationMap.Add(new Dictionary<uint, BundleInfo>(0x1FF));
            }
            BundleDependencyMap = new Dictionary<int, List<int>>();
            BlockInfoMap = new Dictionary<int, Block>();
            BlockMap = new Dictionary<int, byte>();
        }
        public static void FromFile(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                Logger.Info(string.Format("Parsing...."));
                try
                {
                    Clear();

                    using (var stream = File.OpenRead(path))
                    {
                        var bytes = new byte[stream.Length];
                        var count = stream.Read(bytes, 0, bytes.Length);

                        if (count != bytes.Length)
                            throw new Exception("Error While Reading AssetIndex");

                        var json = Encoding.UTF8.GetString(bytes);

                        var obj = JsonConvert.DeserializeObject<AssetIndex>(json);
                        if (obj != null)
                        {
                            MapToResourceIndex(obj);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("AssetIndex was not loaded");
                    Console.WriteLine(e.ToString());
                    return;
                }
                Logger.Info("Loaded !!");
            }
        }
        public static void Clear()
        {
            BundleDependencyMap.Clear();
            BlockInfoMap.Clear();
            BlockMap.Clear();
            AssetMap.Clear();
            AssetLocationMap.ForEach(x => x.Clear());
            BlockSortList.Clear();
        }
        public static void MapToResourceIndex(AssetIndex assetIndex)
        {
            BundleDependencyMap = assetIndex.Dependencies;
            BlockSortList = assetIndex.SortList.ConvertAll(x => (int)x);
            foreach (var asset in assetIndex.SubAssets)
            {
                foreach (var subAsset in asset.Value)
                {
                    var bundleInfo = new BundleInfo() { Bundle = asset.Key, Path = subAsset.Name };
                    AssetLocationMap[subAsset.PathHashPre].Add(subAsset.PathHashLast, bundleInfo);
                    AssetMap[subAsset.PathHashLast] = ((ulong)subAsset.PathHashLast) << 8 | subAsset.PathHashPre;
                }
            }
            foreach (var asset in assetIndex.Assets)
            {
                var block = new Block() { Id = (int)asset.Value.Id, Offset = (int)asset.Value.Offset };
                BlockInfoMap.Add(asset.Key, block);

                if (!BlockMap.ContainsKey((int)asset.Value.Id))
                    BlockMap.Add((int)asset.Value.Id, asset.Value.Language);
            }
        }
        public static List<BundleInfo> GetAllAssets() => AssetLocationMap.SelectMany(x => x.Values).ToList();
        public static List<BundleInfo> GetAssets(int bundle) => AssetLocationMap.SelectMany(x => x.Values).Where(x => x.Bundle == bundle).ToList();
        public static ulong GetAssetIndex(ulong blkHash) => AssetMap.TryGetValue(blkHash, out var value) ? value : 0;
        public static Block GetBlockInfo(int bundle) => BlockInfoMap.TryGetValue(bundle, out var blk) ? blk : null;
        public static BlockFile GetBlockFile(int id) => BlockMap.TryGetValue(id, out var languageCode) ? new BlockFile() { LanguageCode = languageCode, Id = id } : null;
        public static int GetBlockID(int bundle) => BlockInfoMap.TryGetValue(bundle, out var block) ? block.Id : 0;
        public static List<int> GetBundleDep(int bundle) => BundleDependencyMap.TryGetValue(bundle, out var dep) ? dep : new List<int>();
        public static BundleInfo GetBundleInfo(ulong hash)
        {
            var asset = new Asset() { Hash = hash };
            if (AssetLocationMap.ElementAtOrDefault(asset.Pre) != null)
                if (AssetLocationMap[asset.Pre].TryGetValue(asset.Last, out var bundleInfo)) 
                    return bundleInfo;
            return null;
        }
        public static string GetBundlePath(uint last)
        {
            foreach (var location in AssetLocationMap)
                if (location.TryGetValue(last, out var bundleInfo)) 
                    return bundleInfo.Path;
            return null;
        }
        public static List<uint> GetAllAssetIndices(int bundle)
        {
            var hashes = new List<uint>();
            foreach (var location in AssetLocationMap)
                foreach (var pair in location)
                    if (pair.Value.Bundle == bundle)
                        hashes.Add(pair.Key);
            return hashes;
        }
        public static List<int> GetBundles(int id)
        {
            var bundles = new List<int>();
            foreach (var block in BlockInfoMap)
                if (block.Value.Id == id)
                    bundles.Add(block.Key);
            return bundles;
        }
        public static void GetDepBundles(ref List<int> bundles)
        {
            for (int i = 0; i < bundles.Count; i++)
            {
                var bundle = bundles[i];
                bundles.AddRange(GetBundleDep(bundle));
            }
            bundles = bundles.Distinct().ToList();
        }
        public static bool CheckIsLegitAssetPath(ulong hash)
        {
            var asset = new Asset() { Hash = hash };
            return AssetLocationMap.ElementAtOrDefault(asset.Pre).ContainsKey(asset.Last);
        }
        public static string GetContainerFromBinName(string binName)
        {
            var lastHex = Convert.ToUInt32(binName, 16);
            var index = GetAssetIndex(lastHex);
            var bundleInfo = GetBundleInfo(index);
            return bundleInfo != null ? bundleInfo.Path : "";
        }
    }
    public class BundleInfo
    {
        public int Bundle;
        public string Path;
    }
    public class Asset
    {
        public ulong Hash;
        public uint Last => (uint)(Hash >> 8);
        public byte Pre => (byte)(Hash & 0xFF);
    }
    public class Block
    {
        public int Id;
        public int Offset;
    }
    public class BlockFile
    {
        public int LanguageCode;
        public int Id;
    }

    public class AssetIndex
    {
        public Dictionary<string, string> Types { get; set; }
        public class SubAssetInfo
        {
            public string Name { get; set; }
            public byte PathHashPre { get; set; }
            public uint PathHashLast { get; set; }
        }
        public Dictionary<int, List<SubAssetInfo>> SubAssets { get; set; }
        public Dictionary<int, List<int>> Dependencies { get; set; }
        public List<uint> PreloadBlocks { get; set; }
        public List<uint> PreloadShaderBlocks { get; set; }
        public class BlockInfo
        {
            public byte Language { get; set; }
            public uint Id { get; set; }
            public uint Offset { get; set; }
        }
        public Dictionary<int, BlockInfo> Assets { get; set; }
        public List<uint> SortList { get; set; }
    }
}
