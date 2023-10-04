using System;
using System.IO;
using System.Linq;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace AssetStudio.CLI
{
    public static class CommandLine
    {
        public static void Init(string[] args)
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
                optionsBinder.Verbose,
                optionsBinder.Filter,
                optionsBinder.GameName,
                optionsBinder.KeyIndex,
                optionsBinder.MapOp,
                optionsBinder.MapType,
                optionsBinder.MapName,
                optionsBinder.UnityVersion,
                optionsBinder.GroupAssetsType,
                optionsBinder.ExportOptions,
                optionsBinder.AssetBrowser,
                optionsBinder.Model,
                optionsBinder.AIFile,
                optionsBinder.DummyDllFolder,
                optionsBinder.Input,
                optionsBinder.Output
            };

            rootCommand.SetHandler(Program.Run, optionsBinder);

            return rootCommand;
        }
    }
    public class Options
    {
        public bool Silent { get; set; }
        public bool Verbose { get; set; }
        public Regex[] Filter { get; set; }
        public string GameName { get; set; }
        public int KeyIndex { get; set; }
        public MapOpType MapOp { get; set; }
        public ExportListType MapType { get; set; }
        public string MapName { get; set; }
        public string UnityVersion { get; set; }
        public AssetGroupOption GroupAssetsType { get; set; }
        public bool ExportOptions { get; set; }
        public bool AssetBrowser { get; set; }
        public bool Model { get; set; }
        public FileInfo AIFile { get; set; }
        public DirectoryInfo DummyDllFolder { get; set; }
        public FileInfo Input { get; set; }
        public DirectoryInfo Output { get; set; }
    }

    public class OptionsBinder : BinderBase<Options>
    {
        public readonly Option<bool> Silent;
        public readonly Option<bool> Verbose;
        public readonly Option<Regex[]> Filter;
        public readonly Option<string> GameName;
        public readonly Option<int> KeyIndex;
        public readonly Option<MapOpType> MapOp;
        public readonly Option<ExportListType> MapType;
        public readonly Option<string> MapName;
        public readonly Option<string> UnityVersion;
        public readonly Option<AssetGroupOption> GroupAssetsType;
        public readonly Option<bool> ExportOptions;
        public readonly Option<bool> AssetBrowser;
        public readonly Option<bool> Model;
        public readonly Option<FileInfo> AIFile;
        public readonly Option<DirectoryInfo> DummyDllFolder;
        public readonly Argument<FileInfo> Input;
        public readonly Argument<DirectoryInfo> Output;


        public OptionsBinder()
        {
            Silent = new Option<bool>("--silent", "Hide log messages.");
            Verbose = new Option<bool>("--verbose", "Enable verbose logging.");
            Filter = new Option<Regex[]>("--filters", ParseFilter, false, "Specify regex filter(s). (or .txt file with filters)") { AllowMultipleArgumentsPerToken = true };
            GameName = new Option<string>("--game", $"Specify Game.");
            KeyIndex = new Option<int>("--key_index", "Specify key index.");
            MapOp = new Option<MapOpType>("--map_op", "Specify which map to build.");
            MapType = new Option<ExportListType>("--map_type", "AssetMap output type.");
            MapName = new Option<string>("--map_name", () => "assets_map", "Specify AssetMap file name.");
            UnityVersion = new Option<string>("--unity_version", "Specify Unity version.");
            GroupAssetsType = new Option<AssetGroupOption>("--group_assets", "Specify how exported assets should be grouped.");
            ExportOptions = new Option<bool>("--options", "Edit export options.");
            AssetBrowser = new Option<bool>("--browser", "Open AssetBrowser.");
            Model = new Option<bool>("--models", "Enable to export models only");
            AIFile = new Option<FileInfo>("--ai_file", "Specify asset_index json file path (to recover GI containers).").LegalFilePathsOnly();
            DummyDllFolder = new Option<DirectoryInfo>("--dummy_dlls", "Specify DummyDll path.").LegalFilePathsOnly();
            Input = new Argument<FileInfo>("input_path", "Input file/folder.").LegalFilePathsOnly();
            Output = new Argument<DirectoryInfo>("output_path", "Output folder.").LegalFilePathsOnly();
          
            GameName.FromAmong(GameManager.GetGameNames());

            GameName.SetDefaultValue(GameManager.GetGame(GameType.Normal));
            GroupAssetsType.SetDefaultValue(AssetGroupOption.ByType);
            MapOp.SetDefaultValue(MapOpType.None);
            MapType.SetDefaultValue(ExportListType.XML);
            KeyIndex.SetDefaultValue(-1);
        }

        public Regex[] ParseFilter(ArgumentResult result)
        {
            var regex = new List<Regex>();
            var values = result.Tokens.Select(x => x.Value).ToArray();
            if (values.Length == 1)
            {
                try
                {
                    var file = new FileInfo(values[0]);
                    if (file.Exists && file.Extension == ".txt")
                    {
                        values = File.ReadAllLines(file.FullName);
                    }
                }
                catch (Exception) { };
            }
            foreach (var val in values)
            {
                if (string.IsNullOrWhiteSpace(val))
                {
                    result.ErrorMessage = "Empty string.";
                    return Array.Empty<Regex>();
                }

                try
                {
                    Regex.Match("", val, RegexOptions.IgnoreCase);
                }
                catch (ArgumentException e)
                {
                    result.ErrorMessage = "Invalid Regex.\n" + e.Message;
                    return Array.Empty<Regex>();
                }

                regex.Add(new Regex(val));
            }

            return regex.ToArray();
        }

        protected override Options GetBoundValue(BindingContext bindingContext) =>
        new()
        {
            Silent = bindingContext.ParseResult.GetValueForOption(Silent),
            Verbose = bindingContext.ParseResult.GetValueForOption(Verbose),
            Filter = bindingContext.ParseResult.GetValueForOption(Filter),
            GameName = bindingContext.ParseResult.GetValueForOption(GameName),
            KeyIndex = bindingContext.ParseResult.GetValueForOption(KeyIndex),
            MapOp = bindingContext.ParseResult.GetValueForOption(MapOp),
            MapType = bindingContext.ParseResult.GetValueForOption(MapType),
            MapName = bindingContext.ParseResult.GetValueForOption(MapName),
            UnityVersion = bindingContext.ParseResult.GetValueForOption(UnityVersion),
            GroupAssetsType = bindingContext.ParseResult.GetValueForOption(GroupAssetsType),
            ExportOptions = bindingContext.ParseResult.GetValueForOption(ExportOptions),
            AssetBrowser = bindingContext.ParseResult.GetValueForOption(AssetBrowser),
            Model = bindingContext.ParseResult.GetValueForOption(Model),
            AIFile = bindingContext.ParseResult.GetValueForOption(AIFile),
            DummyDllFolder = bindingContext.ParseResult.GetValueForOption(DummyDllFolder),
            Input = bindingContext.ParseResult.GetValueForArgument(Input),
            Output = bindingContext.ParseResult.GetValueForArgument(Output)
        };
    }
}
