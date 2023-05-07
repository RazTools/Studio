using System;
using System.IO;
using System.Linq;
using System.Threading;
using AssetStudio;
using AssetStudioCLI.Properties;
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
                AssetsHelper.Minimal = Settings.Default.minimalAssetMap;
                Shader.Parsable = !Settings.Default.disableShader;
                Renderer.Parsable = !Settings.Default.disableRenderer;
                assetsManager.Silent = o.Silent;
                assetsManager.Game = game;
                ModelOnly = o.Model;

                if (o.Key != default)
                {
                    MiHoYoBinData.Encrypted = true;
                    MiHoYoBinData.Key = o.Key;
                }

                if (o.AIFile != null && game.Type.IsGISubGroup())
                {
                    ResourceIndex.FromFile(o.AIFile.FullName);
                }

                if (o.DummyDllFolder != null)
                {
                    assemblyLoader.Load(o.DummyDllFolder.FullName);
                }

                Logger.Info("Scanning for files...");
                var files = o.Input.Attributes.HasFlag(FileAttributes.Directory) ? Directory.GetFiles(o.Input.FullName, "*.*", SearchOption.AllDirectories).OrderBy(x => x.Length).ToArray() : new string[] { o.Input.FullName };
                files = files.Where(x => FileReader.IsReadable(x, game)).ToArray();
                Logger.Info($"Found {files.Length} files");

                if (o.MapOp.HasFlag(MapOpType.CABMap))
                {
                    AssetsHelper.BuildCABMap(files, o.MapName, o.Input.FullName, game);
                }
                if (o.MapOp.HasFlag(MapOpType.Load))
                {
                    AssetsHelper.LoadCABMap(o.MapName);
                    assetsManager.ResolveDependencies = true;
                }
                if (o.MapOp.HasFlag(MapOpType.AssetMap))
                {
                    if (files.Length == 1)
                    {
                        throw new Exception("Unable to build AssetMap with input_path as a file !!");
                    }
                    if (!o.Output.Exists)
                    {
                        o.Output.Create();
                    }
                    var resetEvent = new ManualResetEvent(false);
                    AssetsHelper.BuildAssetMap(files, o.MapName, game, o.Output.FullName, o.MapType, resetEvent, o.TypeFilter, o.NameFilter, o.ContainerFilter);
                    resetEvent.WaitOne();
                }
                if (o.MapOp.HasFlag(MapOpType.Both))
                {
                    var resetEvent = new ManualResetEvent(false);
                    AssetsHelper.BuildBoth(files, o.MapName, o.Input.FullName, game, o.Output.FullName, o.MapType, resetEvent, o.TypeFilter, o.NameFilter, o.ContainerFilter);
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
                            BuildAssetData(o.TypeFilter, o.NameFilter, o.ContainerFilter, ref i);
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