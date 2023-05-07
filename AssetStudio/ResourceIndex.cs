using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetStudio
{
    public static class ResourceIndex
    {
        private static AssetIndex Instance = new();
        private static Dictionary<uint, Dictionary<uint, string>> BundleMap = new Dictionary<uint, Dictionary<uint, string>>();
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

                        var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                        JsonConvert.PopulateObject(json, Instance, settings);

                        BuildBundleMap();
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
        private static void BuildBundleMap()
        {
            foreach(var asset in Instance.Assets)
            {
                if (!BundleMap.ContainsKey(asset.Value.Id))
                {
                    BundleMap[asset.Value.Id] = new Dictionary<uint, string>();
                }
                if (Instance.SubAssets.TryGetValue(asset.Key, out var subAssets))
                {
                    foreach(var subAsset in subAssets)
                    {
                        BundleMap[asset.Value.Id].Add(subAsset.PathHashLast, subAsset.Name);
                    }
                }
            }
        }
        public static void Clear()
        {
            Instance.Types.Clear();
            Instance.SubAssets.Clear();
            Instance.Dependencies.Clear();
            Instance.PreloadBlocks.Clear();
            Instance.PreloadShaderBlocks.Clear();
            Instance.Assets.Clear();
            Instance.SortList.Clear();
            BundleMap.Clear();
        }
        public static string GetContainer(uint id, uint last)
        {
            if (BundleMap.TryGetValue(id, out var bundles))
            {
                if (bundles.TryGetValue(last, out var container))
                {
                    return container;
                }
            }

            return string.Empty;
        }
    }
}
