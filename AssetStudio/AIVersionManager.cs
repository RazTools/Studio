using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace AssetStudio
{
    public static class AIVersionManager
    {
        private const string BaseUrl = "https://raw.githubusercontent.com/14eyes/gi-asset-indexes/master/";
        private const string CommitsUrl = "https://api.github.com/repos/14eyes/gi-asset-indexes/commits?path=";
        private const string VersionIndexName = "version-index.json";
        private const string VersionIndexKey = "index";

        private static readonly string BaseAIFolder = Path.Combine(Environment.CurrentDirectory, "AI");
        private static readonly string VersionsPath = Path.Combine(BaseAIFolder, "versions.json");
        
        private static readonly HttpClient Client;

        private static Dictionary<string, VersionIndex> Versions;
        
        static AIVersionManager()
        {
            Client = new HttpClient();
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 6.0; Windows 98; Trident/5.1)");
            Versions = new Dictionary<string, VersionIndex>();
        }

        public static List<(string, bool)> GetVersions()
        {
            var versions = new List<(string, bool)>();
            var cachedVersions = LoadVersions();
            foreach (var version in Versions)
            {
                versions.Add((version.Key, cachedVersions.ContainsKey(version.Key)));
            }
            return versions;
        }
        public static async Task<bool> FetchVersions()
        {
            var versions = "";
            var url = Path.Combine(BaseUrl, VersionIndexName);
            var path = GetPath(VersionIndexKey);
            if (await NeedDownload(VersionIndexKey, VersionIndexName))
            {
                versions = await DownloadString(url, TimeSpan.FromSeconds(2));
                if (string.IsNullOrEmpty(versions))
                {
                    Logger.Warning("Could not load AI versions !!");
                    return false;
                }
                if (!await StoreCommit(VersionIndexKey, VersionIndexName))
                {
                    throw new Exception("Failed to store version list !!");
                }
                File.WriteAllText(path, versions);
            }
            else
            {
                versions = File.ReadAllText(path);
            }
            Versions = JsonConvert.DeserializeObject<List<VersionIndex>>(versions).ToDictionary(x => x.Version, x => x);
            return Versions.Count > 0;
        }

        public static async Task<string> FetchAI(string version)
        {
            var path = "";
            if (Versions.TryGetValue(version, out var versionIndex))
            {
                var url = Path.Combine(BaseUrl, versionIndex.MappedPath);
                path = GetPath(version);
                if (await NeedDownload(version, versionIndex.MappedPath))
                {
                    Logger.Info("Downloading...");
                    var json = await DownloadString(url, TimeSpan.FromMinutes(2));
                    if (string.IsNullOrEmpty(json))
                    {
                        Logger.Warning("Could not load AI !!");
                        return "";
                    }
                    if (!await StoreCommit(version, versionIndex.MappedPath))
                    {
                        throw new Exception("Failed to store AI !!");
                    }
                    File.WriteAllText(path, json);
                }
            }
            return path;
        }
        private static bool CreateUri(string source, out Uri result) => Uri.TryCreate(source, UriKind.Absolute, out result) && result.Scheme == Uri.UriSchemeHttps;

        private static async Task<string> DownloadString(string url, TimeSpan timeout)
        {
            var content = "";
            if (CreateUri(url, out var uri))
            {
                try
                {
                    using (var cts = new CancellationTokenSource())
                    {
                        cts.CancelAfter(timeout);
                        var response = await Client.GetAsync(uri, cts.Token);
                        content = await response.Content.ReadAsStringAsync();
                    }
                }
                catch (TaskCanceledException ex)
                {
                    Logger.Warning($"Timeout occured while trying to download {Path.GetFileName(url)}, {ex.Message}");
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Failed to download {Path.GetFileName(url)}, {ex.Message}");
                }
            }
            return content;
        }

        private static async Task<bool> NeedDownload(string key, string path)
        {
            if (!File.Exists(GetPath(key)))
            {
                return true;
            }
            var latestCommit = await GetLatestCommit(path);
            if (string.IsNullOrEmpty(latestCommit))
            {
                return !File.Exists(GetPath(key));
            }
            var dict = LoadVersions();
            if (dict.TryGetValue(key, out var commit))
            {
                if (commit == latestCommit)
                {
                    return false;
                }
            }
            return true;
        }

        private static async Task<bool> StoreCommit(string key, string path)
        {
            var latestCommit = await GetLatestCommit(path);
            if (string.IsNullOrEmpty(latestCommit))
            {
                return false;
            }
            var dict = LoadVersions();
            if (dict.TryGetValue(key, out var commit))
            {
                if (commit != latestCommit)
                {
                    dict[key] = latestCommit;
                }
            }
            else dict.Add(key, latestCommit);
            StoreVersions(dict);
            return true;
        }

        private static Dictionary<string, string> CreateVersions()
        {
            var dict = new Dictionary<string, string>();
            var dir = Path.GetDirectoryName(VersionsPath);
            Directory.CreateDirectory(dir);
            var json = JsonConvert.SerializeObject(dict);
            File.WriteAllText(VersionsPath, json);
            return dict;
        }

        private static Dictionary<string, string> LoadVersions()
        {
            if (!File.Exists(VersionsPath))
            {
                return CreateVersions();
            }

            var file = File.ReadAllText(VersionsPath);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(file);
        }

        private static void StoreVersions(Dictionary<string, string> dict)
        {
            var json = JsonConvert.SerializeObject(dict, Formatting.Indented);
            File.WriteAllText(VersionsPath, json);
        }

        private static string GetPath(string version)
        {
            string path = "";
            if (Versions.TryGetValue(version, out var versionIndex))
            {
                path = Path.Combine(BaseAIFolder, Path.GetFileName(versionIndex.MappedPath));
            }
            else if (version == VersionIndexKey)
            {
                path = Path.Combine(BaseAIFolder, VersionIndexName);
            }
            return path;
        }

        private static async Task<string> GetLatestCommit(string path)
        {
            string commit = "";
            var json = await DownloadString($"{CommitsUrl}{path}", TimeSpan.FromSeconds(2));
            try
            {
                JArray data = JArray.Parse(json);
                commit = data[0]["sha"].ToString();
            }
            catch (Exception)
            {
                Logger.Warning($"Failed to parse latest commit {Path.GetFileName(path)}");
            }
            return commit;
        }

        internal record VersionIndex
        {
            public string MappedPath = "";
            public string RawPath = "";
            public string Version = "";
            public double Coverage = 0;
        }
    }
}
