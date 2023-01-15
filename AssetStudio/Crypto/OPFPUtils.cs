using System;

namespace AssetStudio
{
    public static class OPFPUtils
    {
        public static readonly string[] EncrytpedFolders = { "UI/", "Atlas/", "UITexture/", "DynamicAtlas/" };

        public static void Decrypt(Span<byte> data, string path)
        {
            if (IsEncryptionBundle(path, out var key))
            {
                data[0] ^= key;
                for (int i = 1; i < data.Length; i++)
                {
                    data[i] ^= data[i - 1];
                }
            }
        }
        private static bool IsEncryptionBundle(string path, out byte key) 
        {
            path = path.Replace("\\", "/");
            foreach(var encryptedFolder in EncrytpedFolders)
            {
                var index = path.IndexOf(encryptedFolder, 0, path.Length, StringComparison.OrdinalIgnoreCase);
                if (index != -1)
                {
                    var assetPath = path[index..];
                    if (assetPath.StartsWith(encryptedFolder, StringComparison.OrdinalIgnoreCase))
                    {
                        key = (byte)assetPath.Length;
                        return true;
                    }
                }
            }
            key = 0x00;
            return false;
        }
    }
}
