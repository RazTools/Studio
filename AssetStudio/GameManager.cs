using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace AssetStudio
{
    public static class GameManager
    {
        private static Dictionary<int, Game> Games = new Dictionary<int, Game>();
        static GameManager()
        {
            int count = 0;
            foreach (Type type in
                Assembly.GetAssembly(typeof(Game)).GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(Game))))
            {
                var format = (Game)Activator.CreateInstance(type);
                Games.Add(count++, format);
            }
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
        public static string SupportedGames() => $"Supported Games:\n{string.Join("\n", Games.Values.Select(x => $"{x.Name} ({x.DisplayName})"))}";
    }

    public abstract class Game
    {
        public string Name;
        public string DisplayName;
        public string Extension;
        public string MapName;
        public string Path;
        public override string ToString() => DisplayName;
    }

    public class GI : Game
    {
        public GI()
        {
            Name = "GI";
            DisplayName = "GI";
            MapName = "BLKMap";
            Extension = ".blk";
            Path = "GI_Data|YS_Data";
        }
    }
    public class CB1 : Game
    {
        public CB1()
        {
            Name = "CB1";
            DisplayName = "GI_CB1";
            MapName = "CB1Map";
            Extension = ".asb";
            Path = "GS_Data";
        }
    }
    public class CB2 : Game
    {
        public CB2()
        {
            Name = "CB2";
            DisplayName = "GI_CB2";
            MapName = "CB2Map";
            Extension = ".blk";
            Path = "G_Data";
        }
    }
    public class CB3 : Game
    {
        public CB3()
        {
            Name = "CB3";
            DisplayName = "GI_CB3";
            MapName = "CB3Map";
            Extension = ".blk";
            Path = "YS_Data";
        }
    }
    public class BH3 : Game
    {
        public BH3()
        {
            Name = "BH3";
            DisplayName = "HI3";
            MapName = "WMVMap";
            Extension = ".wmv";
            Path = "BH3_Data";
        }
    }
    public class ZZZ : Game
    {
        public ZZZ()
        {
            Name = "ZZZ";
            DisplayName = "ZZZ";
            MapName = "ZZZMap";
            Extension = ".bundle";
            Path = "Win_Data/StreamingAssets/Bundles";
        }
    }
    public class SR : Game
    {
        public SR()
        {
            Name = "SR";
            DisplayName = "SR";
            MapName = "ENCRMap";
            Extension = ".unity3d";
            Path = "SR_Data";
        }
    }

    public class TOT : Game
    {
        public TOT()
        {
            Name = "TOT";
            DisplayName = "ToT";
            MapName = "TOTMap";
            Extension = ".blk";
            Path = "AssetbundlesCache";
        }
    }
}
