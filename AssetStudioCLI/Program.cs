using System;
using System.IO;
using System.Linq;
using System.CommandLine;
using System.CommandLine.Binding;
using System.Text.RegularExpressions;
using static AssetStudioCLI.Studio;
using AssetStudio;
using System.CommandLine.Parsing;

namespace AssetStudioCLI 
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var rootCommand = RegisterOptions();
            rootCommand.Invoke(args);
        }

        public static RootCommand RegisterOptions()
        {
            var optionsBinder = new OptionsBinder();
            var rootCommand = new RootCommand()
            {
                optionsBinder.Silent,
                optionsBinder.TypeFilter,
                optionsBinder.NameFilter,
                optionsBinder.ContainerFilter,
                optionsBinder.GameName,
                optionsBinder.MapOp,
                optionsBinder.MapType,
                optionsBinder.MapName,
                optionsBinder.GroupAssetsType,
                optionsBinder.NoAssetBundle,
                optionsBinder.NoIndexObject,
                optionsBinder.XorByte,
                optionsBinder.AIFile,
                optionsBinder.Input,
                optionsBinder.Output
            };

            rootCommand.SetHandler((Options o) =>
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
                    assetsManager.Game = game;
                    AssetBundle.Exportable = !o.NoAssetBundle;
                    IndexObject.Exportable = !o.NoIndexObject;

                    if (!o.Silent)
                    {
                        Logger.Default = new ConsoleLogger();
                    }

                    if (o.XorKey != default)
                    {
                        if (o.NoIndexObject)
                        {
                            Logger.Warning("XOR key is set but IndexObject/MiHoYoBinData is excluded, ignoring key...");
                        }
                        else
                        {
                            MiHoYoBinData.doXOR = true;
                            MiHoYoBinData.Key = o.XorKey;
                        }
                    }

                    if (o.AIFile != null && game.Name == "GI" || game.Name == "GI_CB2" || game.Name == "GI_CB3")
                    {
                        ResourceIndex.FromFile(o.AIFile.FullName);
                    }

                    Logger.Info("Scanning for files");
                    var files = o.Input.Attributes.HasFlag(FileAttributes.Directory) ? Directory.GetFiles(o.Input.FullName, $"*{game.Extension}", SearchOption.AllDirectories).OrderBy(x => x.Length).ToArray() : new string[] { o.Input.FullName };
                    Logger.Info(string.Format("Found {0} file(s)", files.Count()));

                    if (o.MapOp.Equals(MapOpType.None))
                    {
                        var i = 0;
                        foreach (var file in files)
                        {
                            assetsManager.LoadFiles(file);
                            BuildAssetData(o.TypeFilter, o.NameFilter, o.ContainerFilter, ref i);
                            ExportAssets(o.Output.FullName, exportableAssets, o.GroupAssetsType);
                            exportableAssets.Clear();
                            assetsManager.Clear();
                        }
                    }
                    if (o.MapOp.HasFlag(MapOpType.CABMap))
                    {
                        CABManager.BuildMap(files.ToList(), game);
                    }
                    if (o.MapOp.HasFlag(MapOpType.AssetMap))
                    {
                        if (files.Length == 1)
                        {
                            throw new Exception("Unable to build AssetMap with input_path as a file !!");
                        }
                        var assets = BuildAssetMap(files.ToList(), o.TypeFilter, o.NameFilter, o.ContainerFilter);
                        if (!o.Output.Exists)
                        {
                            o.Output.Create();
                        }
                        if (string.IsNullOrEmpty(o.MapName))
                        {
                            o.MapName = $"assets_map_{game.Name}";
                        }
                        ExportAssetsMap(o.Output.FullName, assets, o.MapName, o.MapType);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }, optionsBinder);

            return rootCommand;
        }
    }

    public class Options
    {
        public bool Silent { get; set; }
        public ClassIDType[] TypeFilter { get; set; }
        public Regex[] NameFilter { get; set; }
        public Regex[] ContainerFilter { get; set; }
        public string GameName { get; set; }
        public MapOpType MapOp { get; set; }
        public ExportListType MapType { get; set; }
        public string MapName { get; set; }
        public AssetGroupOption GroupAssetsType { get; set; }
        public bool NoAssetBundle { get; set; }
        public bool NoIndexObject { get; set; }
        public byte XorKey { get; set; }
        public FileInfo AIFile { get; set; }
        public FileInfo Input { get; set; }
        public DirectoryInfo Output { get; set; }
    }

    public class OptionsBinder : BinderBase<Options>
    {
        public readonly Option<bool> Silent;
        public readonly Option<ClassIDType[]> TypeFilter;
        public readonly Option<Regex[]> NameFilter;
        public readonly Option<Regex[]> ContainerFilter;
        public readonly Option<string> GameName;
        public readonly Option<MapOpType> MapOp;
        public readonly Option<ExportListType> MapType;
        public readonly Option<string> MapName;
        public readonly Option<AssetGroupOption> GroupAssetsType;
        public readonly Option<bool> NoAssetBundle;
        public readonly Option<bool> NoIndexObject;
        public readonly Option<byte> XorByte;
        public readonly Option<FileInfo> AIFile;
        public readonly Argument<FileInfo> Input;
        public readonly Argument<DirectoryInfo> Output;

        public OptionsBinder()
        {
            Silent = new Option<bool>("--silent", "Hide log messages.");
            TypeFilter = new Option<ClassIDType[]>("--types", "Specify unity class type(s)") { AllowMultipleArgumentsPerToken = true, ArgumentHelpName = "Texture2D|Sprite|etc.." };
            NameFilter = new Option<Regex[]>("--names", result => result.Tokens.Select(x => new Regex(x.Value, RegexOptions.IgnoreCase)).ToArray(), false, "Specify name regex filter(s).") { AllowMultipleArgumentsPerToken = true };
            ContainerFilter = new Option<Regex[]>("--containers", result => result.Tokens.Select(x => new Regex(x.Value, RegexOptions.IgnoreCase)).ToArray(), false, "Specify container regex filter(s).") { AllowMultipleArgumentsPerToken = true };
            GameName = new Option<string>("--game", $"Specify Game.") { IsRequired = true };
            MapOp = new Option<MapOpType>("--map_op", "Specify which map to build.");
            MapType = new Option<ExportListType>("--map_type", "AssetMap output type.");
            MapName = new Option<string>("--map_name", "Specify AssetMap file name.");
            GroupAssetsType = new Option<AssetGroupOption>("--group_assets_type", "Specify how exported assets should be grouped.");
            NoAssetBundle = new Option<bool>("--no_asset_bundle", "Exclude AssetBundle from AssetMap/Export.");
            NoIndexObject = new Option<bool>("--no_index_object", "Exclude IndexObject/MiHoYoBinData from AssetMap/Export.");
            AIFile = new Option<FileInfo>("--ai_file", "Specify asset_index json file path (to recover GI containers).").LegalFilePathsOnly();
            Input = new Argument<FileInfo>("input_path", "Input file/folder.").LegalFilePathsOnly();
            Output = new Argument<DirectoryInfo>("output_path", "Output folder.").LegalFilePathsOnly();

            XorByte = new Option<byte>("--xor_key", result =>
            {
                var value = result.Tokens.Single().Value;
                if (value.StartsWith("0x"))
                {
                    value = value.Substring(2);
                    return Convert.ToByte(value, 0x10);
                }
                else
                {
                    return byte.Parse(value);
                }
            }, false, "XOR key to decrypt MiHoYoBinData.");

            TypeFilter.AddValidator(FilterValidator);
            NameFilter.AddValidator(FilterValidator);
            ContainerFilter.AddValidator(FilterValidator);
            XorByte.AddValidator(result =>
            {
                var value = result.Tokens.Single().Value;
                try
                {
                    if (value.StartsWith("0x"))
                    {
                        value = value.Substring(2);
                        Convert.ToByte(value, 0x10);
                    }
                    else
                    {
                        byte.Parse(value);
                    }
                }
                catch(Exception e)
                {
                    result.ErrorMessage = "Invalid byte value.\n" + e.Message;
                } 
            });

            GameName.FromAmong(GameManager.GetGameNames());

            GroupAssetsType.SetDefaultValue(0);
            MapOp.SetDefaultValue(MapOpType.None);
            MapType.SetDefaultValue(ExportListType.XML);
        }

        public void FilterValidator(OptionResult result)
        {
            {
                var values = result.Tokens.Select(x => x.Value).ToArray();
                foreach (var val in values)
                {
                    if (string.IsNullOrWhiteSpace(val))
                    {
                        result.ErrorMessage = "Empty string.";
                        return;
                    }

                    try
                    {
                        Regex.Match("", val, RegexOptions.IgnoreCase);
                    }
                    catch (ArgumentException e)
                    {
                        result.ErrorMessage = "Invalid Regex.\n" + e.Message;
                        return;
                    }
                }
            }
        }

        protected override Options GetBoundValue(BindingContext bindingContext) =>
        new Options
        {
            Silent = bindingContext.ParseResult.GetValueForOption(Silent),
            TypeFilter = bindingContext.ParseResult.GetValueForOption(TypeFilter),
            NameFilter = bindingContext.ParseResult.GetValueForOption(NameFilter),
            ContainerFilter = bindingContext.ParseResult.GetValueForOption(ContainerFilter),
            GameName = bindingContext.ParseResult.GetValueForOption(GameName),
            MapOp = bindingContext.ParseResult.GetValueForOption(MapOp),
            MapType = bindingContext.ParseResult.GetValueForOption(MapType),
            MapName = bindingContext.ParseResult.GetValueForOption(MapName),
            GroupAssetsType = bindingContext.ParseResult.GetValueForOption(GroupAssetsType),
            NoAssetBundle = bindingContext.ParseResult.GetValueForOption(NoAssetBundle),
            NoIndexObject = bindingContext.ParseResult.GetValueForOption(NoIndexObject),
            XorKey = bindingContext.ParseResult.GetValueForOption(XorByte),
            AIFile = bindingContext.ParseResult.GetValueForOption(AIFile),
            Input = bindingContext.ParseResult.GetValueForArgument(Input),
            Output = bindingContext.ParseResult.GetValueForArgument(Output)
        };
    }
}