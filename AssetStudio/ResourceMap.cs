using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;

namespace AssetStudio
{
    public static class ResourceMap
    {
        private static AssetMap Instance = new() { GameType = GameType.Normal, AssetEntries = new List<AssetEntry>() };
        public static List<AssetEntry> GetEntries() => Instance.AssetEntries;
        public static void FromFile(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                Logger.Info(string.Format("Parsing...."));
                try
                {
                    using var stream = File.OpenRead(path);
                    Instance = MessagePackSerializer.Deserialize<AssetMap>(stream, MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray));
                }
                catch (Exception e)
                {
                    Logger.Error("AssetMap was not loaded");
                    Console.WriteLine(e.ToString());
                    return;
                }
                Logger.Info("Loaded !!");
            }
        }

        public static void Clear()
        {
            Instance.GameType = GameType.Normal;
            Instance.AssetEntries = new List<AssetEntry>();
        }
    }
}
