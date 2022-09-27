using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AssetStudio
{
    public class VersionIndex
    {
        public string MappedPath;
        public string RawPath;
        public string Version;
        public double Coverage;
    }

    public static class AIVersionManager
    {
        private const string BaseUrl = "https://raw.githubusercontent.com/radioegor146/gi-asset-indexes/master/";
        private const string CommitsUrl = "https://api.github.com/repos/radioegor146/gi-asset-indexes/commits?path=";
        
        private static readonly string BaseAIFolder = Path.Combine(Environment.CurrentDirectory, "AI");
        private static readonly string VersionsPath = Path.Combine(BaseAIFolder, "versions.json");
        private static readonly string VersionIndexUrl = Path.Combine(BaseUrl, "version-index.json");
        private static readonly HttpClient Client;

        private static List<VersionIndex> Versions;
        
        public static bool Loaded;
        
        static AIVersionManager()
        {
            Client = new HttpClient();
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 6.0; Windows 98; Trident/5.1)");
            Client.Timeout = TimeSpan.FromMinutes(1);
            Versions = new List<VersionIndex>();
            Loaded = false;
        }

        public static Uri CreateUri(string source, out Uri result) => Uri.TryCreate(source, UriKind.Absolute, out result) && result.Scheme == Uri.UriSchemeHttps ? result : null;

        public static void FetchVersions()
        {
            var versions = Task.Run(() => DownloadString(VersionIndexUrl)).Result;
            if (string.IsNullOrEmpty(versions))
            {
                Logger.Warning("Could not load AI versions !!");
                return;
            }
            Versions = JsonConvert.DeserializeObject<List<VersionIndex>>(versions);
            Loaded = Versions.Count > 0;
        }

        public static async Task<string> DownloadString(string url)
        {
            string json = "";
            if (CreateUri(url, out var uri) != null)
            {
                try
                {
                    json = await Client.GetStringAsync(uri);
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Failed to fetch {Path.GetFileName(url)}, {ex.Message}");
                }
            }
            return json;
        }

        public static async Task<string> DownloadAI(string version)
        {
            var versionIndex = Versions.FirstOrDefault(x => x.Version == version);

            Logger.Info("Downloading....");
            string json = await DownloadString(BaseUrl + versionIndex.MappedPath);
            if (!await StoreCommit(version))
            {
                throw new Exception("Failed to store AIVersion");
            }
            return json;
        }

        public static async Task<bool> NeedDownload(string version)
        {
            var path = GetAIPath(version);
            if (!File.Exists(path)) return true;
            var latestCommit = await GetLatestCommit(version);
            if (string.IsNullOrEmpty(latestCommit)) return true;
            var dict = LoadVersions();
            if (dict.TryGetValue(version, out var commit))
            {
                if (commit == latestCommit) return false;
            }
            return true;
        }

        public static async Task<bool> StoreCommit(string version)
        {
            var latestCommit = await GetLatestCommit(version);
            if (string.IsNullOrEmpty(latestCommit)) return false;
            var dict = LoadVersions();
            if (dict.TryGetValue(version, out var commit))
            {
                if (commit != latestCommit)
                    dict[version] = latestCommit;
            }
            else dict.Add(version, latestCommit);
            StoreVersions(dict);
            return true;
        }

        public static Dictionary<string, string> CreateVersions()
        {
            var dict = new Dictionary<string, string>();
            var dir = Path.GetDirectoryName(VersionsPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            using (var stream = File.Create(VersionsPath))
            {
                var serializer = new JsonSerializer();

                using (StreamWriter writer = new StreamWriter(stream))
                using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
                {
                    JsonSerializer ser = new JsonSerializer();
                    ser.Serialize(jsonWriter, dict);
                    jsonWriter.Flush();
                }
            }
            return dict;
        }

        public static Dictionary<string, string> LoadVersions()
        {
            if (!File.Exists(VersionsPath))
            {
                return CreateVersions();
            }

            var file = File.ReadAllText(VersionsPath);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(file);
        }

        public static void StoreVersions(Dictionary<string, string> dict)
        {
            var json = JsonConvert.SerializeObject(dict, Formatting.Indented);
            File.WriteAllText(VersionsPath, json);
        }

        public static string GetAIPath(string version)
        {
            var versionIndex = Versions.FirstOrDefault(x => x.Version == version);
            return Path.Combine(BaseAIFolder, Path.GetFileName(versionIndex.MappedPath));
        }

        public static async Task<string> GetLatestCommit(string version)
        {
            var versionIndex = Versions.FirstOrDefault(x => x.Version == version);
            string commit = "";
            try
            {
                string json = await DownloadString(CommitsUrl + versionIndex.MappedPath);
                JArray data = JArray.Parse(json);
                commit = data[0]["sha"].ToString();
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to fetch latest commit", ex);
            }
            return commit;
        }

        public static string[] GetVersions() => Versions.Select(x => x.Version).ToArray();
    }
}
