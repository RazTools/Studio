using System;
using System.Collections.Generic;
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

                if (game.Type.IsUnityCN())
                {
                    if (!UnityCNManager.TryGetEntry(o.KeyIndex, out var unityCN))
                    {
                        Console.WriteLine("Invalid key index !!");
                        Console.WriteLine($"Available Options: \n{UnityCNManager.ToString()}");
                        return;
                    }

                    UnityCN.SetKey(unityCN);
                    Logger.Info($"[UnityCN] Selected Key is {unityCN}");
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
                    int idx = 0;
                    var msg = string.Empty;

                    var path = Path.GetDirectoryName(Path.GetFullPath(files[0]));
                    ImportHelper.MergeSplitAssets(path);
                    var toReadFile = ImportHelper.ProcessingSplitFiles(files.ToList());

                    var fileList = new List<string>(toReadFile);
                    for (int i = 0; i < fileList.Count; i++)
                    {
                        var file = fileList[i];
                        assetsManager.LoadFiles(file);
                        if (assetsManager.assetsFileList.Count > 0)
                        {
                            BuildAssetData(o.TypeFilter, o.NameFilter, o.ContainerFilter, ref idx);
                            ExportAssets(o.Output.FullName, exportableAssets, o.GroupAssetsType);
                            msg = $"Processed {Path.GetFileName(file)}";
                        }
                        else
                        {
                            msg = $"Removed {Path.GetFileName(file)}, no assets found";
                            fileList.Remove(file);
                        }
                        Logger.Info($"[{i + 1}/{fileList.Count}] {msg}");
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