using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace AssetStudio
{
    public static class GameManager
    {
        private static Dictionary<int, Game> Games = new Dictionary<int, Game>();
        static GameManager()
        {
            int index = 0;
            Games.Add(index++, new("GI", ".blk", "GI_Data|YS_Data"));
            Games.Add(index++, new("GI_CB1", ".asb", "GS_Data"));
            Games.Add(index++, new("GI_CB2", ".blk", "G_Data"));
            Games.Add(index++, new("GI_CB3", ".blk", "YS_Data"));
            Games.Add(index++, new("BH3", ".wmv", "BH3_Data"));
            Games.Add(index++, new("ZZZ_CB1", ".bundle", "Win_Data/StreamingAssets/Bundles"));
            Games.Add(index++, new("SR_CB2", ".unity3d", "SR_Data"));
            Games.Add(index++, new("SR_CB3", ".block", "SR_Data"));
            Games.Add(index++, new("TOT", ".blk", "AssetbundlesCache"));
        }
        public static Game GetGame(int index)
        {
            if (!Games.TryGetValue(index, out var format))
            {
                throw new ArgumentException("Invalid format !!");
            }

            return format;
        }

        public static Game GetGame(string name)
        {
            foreach(var game in Games)
            {
                if (game.Value.Name == name)
                    return game.Value;
            }

            return null;
        }
        public static Game[] GetGames() => Games.Values.ToArray();
        public static string[] GetGameNames() => Games.Values.Select(x => x.Name).ToArray();
        public static string SupportedGames() => $"Supported Games:\n{string.Join("\n", Games.Values.Select(x => x.Name))}";
    }

    public record Game
    {
        public string Name;
        public string Extension;
        public string Path;
        public Game(string name, string extension, string path)
        {
            Name = name;
            Extension = extension;
            Path = path;
        }
        public override string ToString() => Name;
    }
}
