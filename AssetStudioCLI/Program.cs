using System;
using System.IO;
using System.Linq;
using System.Threading;
using AssetStudio;
using static AssetStudioCLI.Studio;

namespace AssetStudioCLI 
{
    public class Program
    {
        public static void Main(string[] args) => CommandLine.Init(args);

        public static void Run(Options o)
        {
            try
            {
                var game = GameManager.GetGame(o.GameName);

                if (game == null)
                {
                    Console.WriteLine("Invalid Game !!");
                    Console.WriteLine(GameManager.SupportedGames());
                    return;
                }

                Studio.Game = game;
                Logger.Default = new ConsoleLogger();
                assetsManager.Silent = o.Silent;
                assetsManager.Game = game;


                if (!o.TypeFilter.IsNullOrEmpty())
                {
                    foreach (var kv in assetsManager.ExportableTypes)
                    {
                        assetsManager.ExportableTypes[kv.Key] = o.TypeFilter.Contains(kv.Key);
                    }
                    
                }

                if (o.Model)
                {
                    foreach (var kv in assetsManager.ExportableTypes)
                    {
                        assetsManager.ExportableTypes[kv.Key] = false;
                    }

                    assetsManager.ExportableTypes[ClassIDType.Animator] = true;
                    assetsManager.ExportableTypes[ClassIDType.GameObject] = true;
                    assetsManager.ExportableTypes[ClassIDType.Texture2D] = true;
                    assetsManager.ExportableTypes[ClassIDType.Material] = true;
                    assetsManager.ExportableTypes[ClassIDType.Renderer] = true;
                    assetsManager.ExportableTypes[ClassIDType.Mesh] = true;

                    ModelOnly = true;
                }

                if (o.Key != default)
                {
                    if (!assetsManager.ExportableTypes[ClassIDType.MiHoYoBinData])
                    {
                        Logger.Warning("Key is set but MiHoYoBinData is skipped, ignoring key...");
                    }
                    else
                    {
                        MiHoYoBinData.Encrypted = true;
                        MiHoYoBinData.Key = o.Key;
                    }
                }

                if (o.AIFile != null && game.Type.IsGISubGroup())
                {
                    ResourceIndex.FromFile(o.AIFile.FullName);
                }

                if (o.DummyDllFolder != null)
                {
                    assemblyLoader.Load(o.DummyDllFolder.FullName);
                }

                Logger.Info("Scanning for files");
                var files = o.Input.Attributes.HasFlag(FileAttributes.Directory) ? Directory.GetFiles(o.Input.FullName, "*.*", SearchOption.AllDirectories).OrderBy(x => x.Length).ToArray() : new string[] { o.Input.FullName };
                Logger.Info(string.Format("Found {0} file(s)", files.Length));

                if (o.MapOp.HasFlag(MapOpType.Build))
                {
                    AssetsHelper.BuildMap(files, o.MapName, o.Input.FullName, game);
                }
                if (o.MapOp.HasFlag(MapOpType.Load))
                {
                    AssetsHelper.LoadMap(o.MapName);
                    assetsManager.ResolveDependencies = true;
                }
                if (o.MapOp.HasFlag(MapOpType.List))
                {
                    if (files.Length == 1)
                    {
                        throw new Exception("Unable to build AssetMap with input_path as a file !!");
                    }
                    var assets = AssetsHelper.BuildAssetMap(files, game, o.NameFilter, o.ContainerFilter);
                    if (!o.Output.Exists)
                    {
                        o.Output.Create();
                    }
                    var resetEvent = new ManualResetEvent(false);
                    AssetsHelper.ExportAssetsMap(assets, o.MapName, o.Output.FullName, o.MapType, resetEvent);
                    resetEvent.WaitOne();
                }
                if (o.MapOp.Equals(MapOpType.None) || o.MapOp.HasFlag(MapOpType.Load))
                {
                    var i = 0;
                    foreach (var file in files)
                    {
                        assetsManager.LoadFiles(file);
                        if (assetsManager.assetsFileList.Count > 0)
                        {
                            BuildAssetData(o.NameFilter, o.ContainerFilter, ref i);
                            ExportAssets(o.Output.FullName, exportableAssets, o.GroupAssetsType);
                        }
                        exportableAssets.Clear();
                        assetsManager.Clear();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}