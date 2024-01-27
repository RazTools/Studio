using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using AssetStudio.CLI.Properties;
using Newtonsoft.Json;
using static AssetStudio.CLI.Studio;

namespace AssetStudio.CLI 
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
                Logger.Default = new ConsoleLogger() { Flags = o.LoggerFlags.Aggregate((e, x) => e |= x) };
                Logger.FileLogging = Settings.Default.enableFileLogging;
                AssetsHelper.Minimal = Settings.Default.minimalAssetMap;
                AssetsHelper.SetUnityVersion(o.UnityVersion);

                if (o.TypeFilter == null)
                {
                    TypeFlags.SetTypes(JsonConvert.DeserializeObject<Dictionary<ClassIDType, (bool, bool)>>(Settings.Default.types));
                }
                else
                {
                    foreach (var type in o.TypeFilter)
                    {
                        TypeFlags.SetType(type, true, true);
                    }

                    if (ClassIDType.GameObject.CanExport() || ClassIDType.Animator.CanExport())
                    {
                        TypeFlags.SetType(ClassIDType.Texture2D, true, false);
                        if (ClassIDType.GameObject.CanExport())
                        {
                            TypeFlags.SetType(ClassIDType.Animator, true, false);
                        }
                        else if(ClassIDType.Animator.CanExport())
                        {
                            TypeFlags.SetType(ClassIDType.GameObject, true, false);
                        }
                    }
                }

                assetsManager.Silent = o.Silent;
                assetsManager.Game = game;
                assetsManager.SpecifyUnityVersion = o.UnityVersion;
                o.Output.Create();

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
                    AssetsHelper.LoadCABMapInternal(o.MapName);
                    assetsManager.ResolveDependencies = true;
                }
                if (o.MapOp.HasFlag(MapOpType.AssetMap))
                {
                    if (files.Length == 1)
                    {
                        throw new Exception("Unable to build AssetMap with input_path as a file !!");
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

                    var path = Path.GetDirectoryName(Path.GetFullPath(files[0]));
                    ImportHelper.MergeSplitAssets(path);
                    var toReadFile = ImportHelper.ProcessingSplitFiles(files.ToList());

                    var fileList = new List<string>(toReadFile);
                    foreach (var file in fileList)
                    {
                        assetsManager.LoadFiles(file);
                        if (assetsManager.assetsFileList.Count > 0)
                        {
                            BuildAssetData(o.TypeFilter, o.NameFilter, o.ContainerFilter, ref i);
                            ExportAssets(o.Output.FullName, exportableAssets, o.GroupAssetsType, o.AssetExportType);
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