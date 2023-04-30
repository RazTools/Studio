using System;
using System.IO;
using System.Linq;

namespace AssetStudio
{
    public static class OPFPUtils
    {
        private static readonly string BaseFolder = "BundleResources";
        private static readonly string[] V0_Prefixes = { "UI/", "Atlas/", "UITexture/" };
        private static readonly string[] V1_Prefixes = { "DynamicAtlas/", "Atlas/Skill", "Atlas/PlayerTitle", "UITexture/HeroCardEP12", "UITexture/HeroCardEP13" };

        public static void Decrypt(Span<byte> data, string path)
        {
            if (IsEncryptionBundle(path, out var key, out var version))
            {
                switch (version)
                {
                    case 0:
                        data[0] ^= key;
                        for (int i = 1; i < data.Length; i++)
                        {
                            data[i] ^= data[i - 1];
                        }
                        break;
                    case 1:
                        for (int i = 1; i < data.Length; i++)
                        {
                            var idx = (i + data.Length + key * key) % (i + 1);
                            (data[i], data[idx]) = (data[idx], data[i]);
                            data[i] ^= key;
                            data[idx] ^= key;
                        }
                        break;
                }
            }
        }
        private static bool IsEncryptionBundle(string path, out byte key, out int version) 
        {
            if (IsFixedPath(path, out var relativePath))
            {
                if (V1_Prefixes.Any(prefix => relativePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                {
                    key = (byte)Path.GetFileName(relativePath).Length;
                    version = 1;
                    return true;
                }
                else if (V0_Prefixes.Any(prefix => relativePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                {
                    key = (byte)relativePath.Length;
                    version = 0;
                    return true;
                }
            }
            key = 0x00;
            version = 0;
            return false;
        }
        private static bool IsFixedPath(string path, out string fixedPath)
        {
            var dirs = path.Split(Path.DirectorySeparatorChar);
            if (dirs.Contains(BaseFolder))
            {
                var idx = Array.IndexOf(dirs, BaseFolder);
                fixedPath = string.Join(Path.DirectorySeparatorChar, dirs[(idx+1)..]).Replace("\\", "/");
                return true;
            }
            fixedPath = string.Empty;
            return false;
        }
    }
}
