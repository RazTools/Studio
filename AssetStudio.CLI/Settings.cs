using System;
using System.ComponentModel;
using System.Configuration;

namespace AssetStudio.CLI.Properties {
    public static class AppSettings
    {
        public static string Get(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public static TValue Get<TValue>(string key, TValue defaultValue)
        {
            try
            {
                var value = Get(key);

                if (string.IsNullOrEmpty(value)) 
                    return defaultValue;

                return (TValue)TypeDescriptor.GetConverter(typeof(TValue)).ConvertFromInvariantString(value);
            }
            catch (Exception)
            {
                Console.WriteLine($"Invalid value at \"{key}\", switching to default value [{defaultValue}] !!");
                return defaultValue;
            }
            
        }
    }

    public class Settings
    {
        private static Settings defaultInstance = new Settings();

        public static Settings Default => defaultInstance;

        public bool displayAll => AppSettings.Get("displayAll", false);
        public bool enablePreview => AppSettings.Get("enablePreview", true);
        public bool displayInfo => AppSettings.Get("displayInfo", true);
        public bool openAfterExport => AppSettings.Get("openAfterExport", true);
        public int assetGroupOption => AppSettings.Get("assetGroupOption", 0);
        public bool convertTexture => AppSettings.Get("convertTexture", true);
        public bool convertAudio => AppSettings.Get("convertAudio", true);
        public ImageFormat convertType => AppSettings.Get("convertType", ImageFormat.Png);
        public bool eulerFilter => AppSettings.Get("eulerFilter", true);
        public decimal filterPrecision => AppSettings.Get("filterPrecision", (decimal)0.25);
        public bool exportAllNodes => AppSettings.Get("exportAllNodes", true);
        public bool exportSkins => AppSettings.Get("exportSkins", true);
        public bool collectAnimations => AppSettings.Get("collectAnimations", true);
        public bool exportAnimations => AppSettings.Get("exportAnimations", true);
        public decimal boneSize => AppSettings.Get("boneSize", (decimal)10);
        public int fbxVersion => AppSettings.Get("fbxVersion", 3);
        public int fbxFormat => AppSettings.Get("fbxFormat", 0);
        public decimal scaleFactor => AppSettings.Get("scaleFactor", (decimal)1);
        public bool exportBlendShape => AppSettings.Get("exportBlendShape", true);
        public bool castToBone => AppSettings.Get("castToBone", false);
        public bool restoreExtensionName => AppSettings.Get("restoreExtensionName", true);
        public bool exportAllUvsAsDiffuseMaps => AppSettings.Get("exportAllUvsAsDiffuseMaps", false);
        public bool exportUV0UV1 => AppSettings.Get("exportUV0UV1", false);
        public bool encrypted => AppSettings.Get("encrypted", true);
        public byte key => AppSettings.Get("key", (byte)0x93);
        public int selectedGame => AppSettings.Get("selectedGame", 0);
        public bool enableResolveDependencies => AppSettings.Get("enableResolveDependencies", true);
        public int selectedCNUnityKey => AppSettings.Get("selectedCNUnityKey", 0);
        public int selectedAssetMapType => AppSettings.Get("selectedAssetMapType", 0);
        public bool exportMiHoYoBinData => AppSettings.Get("exportMiHoYoBinData", true);
        public bool disableShader => AppSettings.Get("disableShader", false);
        public bool disableRenderer => AppSettings.Get("disableRenderer", false);
        public bool disableAnimationClip => AppSettings.Get("disableAnimationClip", false);
        public bool enableFileLogging => AppSettings.Get("enableFileLogging", false);
        public bool minimalAssetMap => AppSettings.Get("minimalAssetMap", true);

    }
}
