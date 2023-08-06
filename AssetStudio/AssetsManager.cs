using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using static AssetStudio.ImportHelper;

namespace AssetStudio
{
    public class AssetsManager
    {
        public Game Game;
        public bool Silent = false;
        public bool SkipProcess = false;
        public bool CacheObjects = false;
        public bool ResolveDependencies = false;        
        public string SpecifyUnityVersion;
        public CancellationTokenSource tokenSource = new CancellationTokenSource();
        public List<SerializedFile> assetsFileList = new List<SerializedFile>();

        internal Dictionary<string, int> assetsFileIndexCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        internal Dictionary<string, BinaryReader> resourceFileReaders = new Dictionary<string, BinaryReader>(StringComparer.OrdinalIgnoreCase);
        internal Dictionary<string, FileReader> serializedFileReaders = new Dictionary<string, FileReader>(StringComparer.OrdinalIgnoreCase);

        internal List<string> importFiles = new List<string>();
        internal HashSet<string> importFilesHash = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        internal HashSet<string> noexistFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        internal HashSet<string> assetsFileListHash = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public void LoadFiles(string file)
        {
            if (Silent)
            {
                Logger.Silent = true;
                Progress.Silent = true;
            }

            LoadFile(file);

            if (Silent)
            {
                Logger.Silent = false;
                Progress.Silent = false;
            }
        }

        public void LoadFiles(params string[] files)
        {
            if (Silent)
            {
                Logger.Silent = true;
                Progress.Silent = true;
            }

            var path = Path.GetDirectoryName(Path.GetFullPath(files[0]));
            MergeSplitAssets(path);
            var toReadFile = ProcessingSplitFiles(files.ToList());
            Load(toReadFile);

            if (Silent)
            {
                Logger.Silent = false;
                Progress.Silent = false;
            }
        }

        public void LoadFolder(string path)
        {
            if (Silent)
            {
                Logger.Silent = true;
                Progress.Silent = true;
            }

            MergeSplitAssets(path, true);
            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).ToList();
            var toReadFile = ProcessingSplitFiles(files);
            Load(toReadFile);

            if (Silent)
            {
                Logger.Silent = false;
                Progress.Silent = false;
            }
        }

        private void Load(string[] files)
        {
            foreach (var file in files)
            {
                importFiles.Add(file);
                importFilesHash.Add(Path.GetFileName(file));
            }

            Progress.Reset();
            //use a for loop because list size can change
            for (var i = 0; i < importFiles.Count; i++)
            {
                LoadFile(importFiles[i]);
                Progress.Report(i + 1, importFiles.Count);
                if (tokenSource.IsCancellationRequested)
                {
                    Logger.Info("Loading files has been aborted !!");
                    break;
                }
            }

            importFiles.Clear();
            importFilesHash.Clear();
            noexistFiles.Clear();
            assetsFileListHash.Clear();
            AssetsHelper.ClearOffsets();

            if (!SkipProcess)
            {
                ProcessAssets();
            }
        }

        private void LoadFile(string fullName)
        {
            var reader = new FileReader(fullName);
            reader = reader.PreProcessing(Game);
            LoadFile(reader);
        }

        private void LoadFile(FileReader reader, string originalPath = null)
        {
            switch (reader.FileType)
            {
                case FileType.AssetsFile:
                    LoadAssetsFile(reader, originalPath);
                    break;
                case FileType.BundleFile:
                    LoadBundleFile(reader, originalPath);
                    break;
                case FileType.WebFile:
                    LoadWebFile(reader);
                    break;
                case FileType.ZipFile:
                    LoadZipFile(reader, originalPath);
                    break;
                case FileType.BlockFile:
                    LoadBlockFile(reader);
                    break;
                case FileType.BlkFile:
                    LoadBlkFile(reader);
                    break;
                case FileType.BlbFile:
                    LoadBlbFile(reader);
                    break;
            }
        }

        private void LoadAssetsFile(FileReader reader, string originalPath = null)
        {
            if (!assetsFileListHash.Contains(reader.FileName))
            {
                Logger.Info($"Loading {reader.FullPath}");
                try
                {
                    var assetsFile = new SerializedFile(reader, this);
                    assetsFile.originalPath = originalPath ?? reader.FullPath;
                    CheckStrippedVersion(assetsFile);
                    assetsFileList.Add(assetsFile);
                    assetsFileListHash.Add(assetsFile.fileName);

                    foreach (var sharedFile in assetsFile.m_Externals)
                    {
                        var sharedFileName = sharedFile.fileName;

                        if (!importFilesHash.Contains(sharedFileName))
                        {
                            var sharedFilePath = Path.Combine(Path.GetDirectoryName(reader.FullPath), sharedFileName);
                            if (!noexistFiles.Contains(sharedFilePath))
                            {
                                if (!File.Exists(sharedFilePath))
                                {
                                    var findFiles = Directory.GetFiles(Path.GetDirectoryName(reader.FullPath), sharedFileName, SearchOption.AllDirectories);
                                    if (findFiles.Length > 0)
                                    {
                                        sharedFilePath = findFiles[0];
                                    }
                                }
                                if (File.Exists(sharedFilePath))
                                {
                                    importFiles.Add(sharedFilePath);
                                    importFilesHash.Add(sharedFileName);
                                }
                                else
                                {
                                    noexistFiles.Add(sharedFilePath);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error($"Error while reading assets file {reader.FullPath}", e);
                    reader.Dispose();
                }
            }
            else
            {
                Logger.Info($"Skipping {reader.FullPath}");
                reader.Dispose();
            }
        }

        private void LoadAssetsFromMemory(FileReader reader, string originalPath, string unityVersion = null, long originalOffset = 0)
        {
            if (!assetsFileListHash.Contains(reader.FileName))
            {
                try
                {
                    var assetsFile = new SerializedFile(reader, this);
                    assetsFile.originalPath = originalPath;
                    assetsFile.offset = originalOffset;
                    if (!string.IsNullOrEmpty(unityVersion) && assetsFile.header.m_Version < SerializedFileFormatVersion.Unknown_7)
                    {
                        assetsFile.SetVersion(unityVersion);
                    }
                    CheckStrippedVersion(assetsFile);
                    assetsFileList.Add(assetsFile);
                    assetsFileListHash.Add(assetsFile.fileName);

                    if (ResolveDependencies)
                    {
                        foreach (var sharedFile in assetsFile.m_Externals)
                        {
                            var sharedFileName = sharedFile.fileName;
                    
                            if (!importFilesHash.Contains(sharedFileName))
                            {
                                var sharedFilePath = Path.Combine(Path.GetDirectoryName(originalPath), sharedFileName);
                                if (!noexistFiles.Contains(sharedFilePath))
                                {
                                    if (AssetsHelper.TryAdd(sharedFileName, out var path))
                                    {
                                        sharedFilePath = path;
                                    }
                                    if (File.Exists(sharedFilePath))
                                    {
                                        if (!importFiles.Contains(sharedFilePath))
                                        {
                                            importFiles.Add(sharedFilePath);
                                        }
                                        importFilesHash.Add(sharedFileName);
                                    }
                                    else
                                    {
                                        noexistFiles.Add(sharedFilePath);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error($"Error while reading assets file {reader.FullPath} from {Path.GetFileName(originalPath)}", e);
                }
            }
            else
                Logger.Info($"Skipping {originalPath} ({reader.FileName})");
        }

        private void LoadBundleFile(FileReader reader, string originalPath = null, long originalOffset = 0, bool log = true)
        {
            if (log)
            {
                Logger.Info("Loading " + reader.FullPath);
            }
            try
            {
                var bundleFile = new BundleFile(reader, Game);
                foreach (var file in bundleFile.fileList)
                {
                    var dummyPath = Path.Combine(reader.FullPath, file.fileName);
                    var subReader = new FileReader(dummyPath, file.stream);
                    if (subReader.FileType == FileType.AssetsFile)
                    {
                        LoadAssetsFromMemory(subReader, originalPath ?? reader.FullPath, bundleFile.m_Header.unityRevision, originalOffset);
                    }
                    subReader.Close();
                }
            }
            catch (InvalidCastException)
            {
                Logger.Error($"Game type mismatch, Expected {nameof(Mr0k)} but got {Game.Name} ({Game.GetType().Name}) !!");
            }
            catch (Exception e)
            {
                var str = $"Error while reading bundle file {reader.FullPath}";
                if (originalPath != null)
                {
                    str += $" from {Path.GetFileName(originalPath)}";
                }
                Logger.Error(str, e);
            }
            finally
            {
                reader.Dispose();
            }
        }

        private void LoadWebFile(FileReader reader)
        {
            Logger.Info("Loading " + reader.FullPath);
            try
            {
                var webFile = new WebFile(reader);
                foreach (var file in webFile.fileList)
                {
                    var dummyPath = Path.Combine(reader.FullPath, file.fileName);
                    var subReader = new FileReader(dummyPath, file.stream);
                    switch (subReader.FileType)
                    {
                        case FileType.AssetsFile:
                            LoadAssetsFromMemory(subReader, reader.FullPath);
                            break;
                        case FileType.BundleFile:
                            LoadBundleFile(subReader, reader.FullPath);
                            break;
                        case FileType.WebFile:
                            LoadWebFile(subReader);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Error while reading web file {reader.FullPath}", e);
            }
            finally
            {
                reader.Dispose();
            }
        }

        private void LoadZipFile(FileReader reader, string originalPath = null)
        {
            Logger.Info("Loading " + reader.FileName);
            try
            {
                using (ZipArchive archive = new ZipArchive(reader.BaseStream, ZipArchiveMode.Read))
                {
                    List<string> splitFiles = new List<string>();
                    // register all files before parsing the assets so that the external references can be found
                    // and find split files
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.Name.Contains(".split"))
                        {
                            string baseName = Path.GetFileNameWithoutExtension(entry.Name);
                            string basePath = Path.Combine(Path.GetDirectoryName(entry.FullName), baseName);
                            if (!splitFiles.Contains(basePath))
                            {
                                splitFiles.Add(basePath);
                                importFilesHash.Add(baseName);
                            }
                        }
                        else
                        {
                            importFilesHash.Add(entry.Name);
                        }
                    }

                    // merge split files and load the result
                    foreach (string basePath in splitFiles)
                    {
                        try
                        {
                            Stream splitStream = new MemoryStream();
                            int i = 0;
                            while (true)
                            {
                                string path = $"{basePath}.split{i++}";
                                ZipArchiveEntry entry = archive.GetEntry(path);
                                if (entry == null)
                                    break;
                                using (Stream entryStream = entry.Open())
                                {
                                    entryStream.CopyTo(splitStream);
                                }
                            }
                            splitStream.Seek(0, SeekOrigin.Begin);
                            FileReader entryReader = new FileReader(basePath, splitStream);
                            LoadFile(entryReader);
                        }
                        catch (Exception e)
                        {
                            Logger.Error($"Error while reading zip split file {basePath}", e);
                        }
                    }

                    // load all entries
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        try
                        {
                            string dummyPath = Path.Combine(Path.GetDirectoryName(reader.FullPath), reader.FileName, entry.FullName);
                            // create a new stream
                            // - to store the deflated stream in
                            // - to keep the data for later extraction
                            Stream streamReader = new MemoryStream();
                            using (Stream entryStream = entry.Open())
                            {
                                entryStream.CopyTo(streamReader);
                            }
                            streamReader.Position = 0;

                            FileReader entryReader = new FileReader(dummyPath, streamReader);
                            LoadFile(entryReader, originalPath ?? reader.FullPath);
                            if (entryReader.FileType == FileType.ResourceFile)
                            {
                                entryReader.Position = 0;
                                if (!resourceFileReaders.ContainsKey(entry.Name))
                                {
                                    resourceFileReaders.Add(entry.Name, entryReader);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Error($"Error while reading zip entry {entry.FullName}", e);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Error while reading zip file {reader.FileName}", e);
            }
            finally
            {
                reader.Dispose();
            }
        }
        private void LoadBlockFile(FileReader reader)
        {
            Logger.Info("Loading " + reader.FullPath);
            try
            {
                using var stream = new OffsetStream(reader.BaseStream, 0);
                if (AssetsHelper.TryGet(reader.FullPath, out var offsets))
                {
                    foreach (var offset in offsets)
                    {
                        var name = offset.ToString("X8");
                        Logger.Info($"Loading Block {name}");

                        stream.Offset = offset;
                        var dummyPath = Path.Combine(reader.FullPath, name);
                        var subReader = new FileReader(dummyPath, stream, true);
                        switch(subReader.FileType)
                        {
                            case FileType.BundleFile:
                                LoadBundleFile(subReader, reader.FullPath, offset, false);
                                break;
                            case FileType.BlbFile:
                                LoadBlbFile(subReader, reader.FullPath, offset, false);
                                break;
                        }
                    }
                    AssetsHelper.Remove(reader.FullPath);
                }
                else
                {
                    do
                    {
                        var name = stream.AbsolutePosition.ToString("X8");
                        Logger.Info($"Loading Block {name}");

                        stream.Offset = stream.AbsolutePosition;
                        var dummyPath = Path.Combine(reader.FullPath, name);
                        var subReader = new FileReader(dummyPath, stream, true);
                        switch (subReader.FileType)
                        {
                            case FileType.BundleFile:
                                LoadBundleFile(subReader, reader.FullPath, stream.AbsolutePosition, false);
                                break;
                            case FileType.BlbFile:
                                LoadBlbFile(subReader, reader.FullPath, stream.AbsolutePosition, false);
                                break;
                        }
                    } while (stream.Remaining > 0);
                }    
            }
            catch (Exception e)
            {
                Logger.Error($"Error while reading block file {reader.FileName}", e);
            }
            finally
            {
                reader.Dispose();
            }
        }
        private void LoadBlkFile(FileReader reader)
        {
            Logger.Info("Loading " + reader.FullPath);
            try
            {
                using var stream = BlkUtils.Decrypt(reader, (Blk)Game);
                if (AssetsHelper.TryGet(reader.FullPath, out var offsets))
                {
                    foreach (var offset in offsets)
                    {
                        var name = offset.ToString("X8");
                        Logger.Info($"Loading Block {name}");

                        stream.Offset = offset;
                        var dummyPath = Path.Combine(reader.FullPath, name);
                        var subReader = new FileReader(dummyPath, stream, true);
                        switch (subReader.FileType)
                        {
                            case FileType.BundleFile:
                                LoadBundleFile(subReader, reader.FullPath, offset, false);
                                break;
                            case FileType.Mhy0File:
                                LoadMhy0File(subReader, reader.FullPath, offset, false);
                                break;
                        }
                    }
                    AssetsHelper.Remove(reader.FullPath);
                }
                else
                {
                    do
                    {
                        var name = stream.AbsolutePosition.ToString("X8");
                        Logger.Info($"Loading Block {name}");

                        var dummyPath = Path.Combine(reader.FullPath, name);
                        var subReader = new FileReader(dummyPath, stream, true);
                        switch (subReader.FileType)
                        {
                            case FileType.BundleFile:
                                LoadBundleFile(subReader, reader.FullPath, stream.AbsolutePosition, false);
                                break;
                            case FileType.Mhy0File:
                                LoadMhy0File(subReader, reader.FullPath, stream.AbsolutePosition, false);
                                break;
                        }

                        stream.Offset = stream.AbsolutePosition;
                    } while (stream.Remaining > 0);
                }
            }
            catch (InvalidCastException)
            {
                Logger.Error($"Game type mismatch, Expected {nameof(Blk)} but got {Game.Name} ({Game.GetType().Name}) !!");
            }
            catch (Exception e)
            {
                Logger.Error($"Error while reading blk file {reader.FileName}", e);
            }
            finally
            {
                reader.Dispose();
            }
        } 
        private void LoadMhy0File(FileReader reader, string originalPath = null, long originalOffset = 0, bool log = true)
        {
            if (log)
            {
                Logger.Info("Loading " + reader.FullPath);
            }
            try
            {
                var mhy0File = new Mhy0File(reader, reader.FullPath, (Mhy0)Game);
                foreach (var file in mhy0File.fileList)
                {
                    var dummyPath = Path.Combine(reader.FullPath, file.fileName);
                    var cabReader = new FileReader(dummyPath, file.stream);
                    if (cabReader.FileType == FileType.AssetsFile)
                    {
                        LoadAssetsFromMemory(cabReader, originalPath ?? reader.FullPath, mhy0File.m_Header.unityRevision, originalOffset);
                    }
                }
            }
            catch (InvalidCastException)
            {
                Logger.Error($"Game type mismatch, Expected {nameof(Mhy0)} but got {Game.Name} ({Game.GetType().Name}) !!");
            }
            catch (Exception e)
            {
                var str = $"Error while reading mhy0 file {reader.FullPath}";
                if (originalPath != null)
                {
                    str += $" from {Path.GetFileName(originalPath)}";
                }
                Logger.Error(str, e);
            }
            finally
            {
                reader.Dispose();
            }
        }
        private void LoadBlbFile(FileReader reader, string originalPath = null, long originalOffset = 0, bool log = true)
        {
            if (log)
            {
                Logger.Info("Loading " + reader.FullPath);
            }
            try
            {
                var blbFile = new BlbFile(reader, reader.FullPath);
                foreach (var file in blbFile.fileList)
                {
                    var dummyPath = Path.Combine(Path.GetDirectoryName(reader.FullPath), file.fileName);
                    var cabReader = new FileReader(dummyPath, file.stream);
                    if (cabReader.FileType == FileType.AssetsFile)
                    {
                        LoadAssetsFromMemory(cabReader, originalPath ?? reader.FullPath, blbFile.m_Header.unityRevision, originalOffset);
                    }
                }
            }
            catch (InvalidCastException)
            {
                Logger.Error($"Game type mismatch, Expected {nameof(Mhy0)} but got {Game.Name} ({Game.GetType().Name}) !!");
            }
            catch (Exception e)
            {
                var str = $"Error while reading mhy0 file {reader.FullPath}";
                if (originalPath != null)
                {
                    str += $" from {Path.GetFileName(originalPath)}";
                }
                Logger.Error(str, e);
            }
            finally
            {
                reader.Dispose();
            }
        }

        public void CheckStrippedVersion(SerializedFile assetsFile)
        {
            if (assetsFile.IsVersionStripped && string.IsNullOrEmpty(SpecifyUnityVersion))
            {
                throw new Exception("The Unity version has been stripped, please set the version in the options");
            }
            if (!string.IsNullOrEmpty(SpecifyUnityVersion))
            {
                assetsFile.SetVersion(SpecifyUnityVersion);
            }
        }
        public void RemoveReader(string fileNmae) => serializedFileReaders.Remove(fileNmae);

        public void Clear()
        {
            foreach (var assetsFile in assetsFileList)
            {
                assetsFile.ObjectsDic.Clear();
            }
            assetsFileList.Clear();

            foreach (var resourceFileReader in resourceFileReaders)
            {
                resourceFileReader.Value.Close();
            }
            resourceFileReaders.Clear();
            serializedFileReaders.Clear();

            assetsFileIndexCache.Clear();

            tokenSource.Dispose();
            tokenSource = new CancellationTokenSource();
        }

        private void ProcessAssets()
        {
            Logger.Info("Process Assets...");

            foreach (var assetsFile in assetsFileList)
            {
                var gameObjects = assetsFile.ReadObjects(ClassIDType.GameObject).Cast<GameObject>();
                foreach (var m_GameObject in gameObjects)
                {
                    if (tokenSource.IsCancellationRequested)
                    {
                        Logger.Info("Processing assets has been cancelled !!");
                        return;
                    }
                    foreach (var pptr in m_GameObject.m_Components)
                    {
                        if (pptr.TryGet(out var m_Component))
                        {
                            switch (m_Component)
                            {
                                case Transform m_Transform:
                                    m_GameObject.m_Transform = m_Transform;
                                    break;
                                case MeshRenderer m_MeshRenderer:
                                    m_GameObject.m_MeshRenderer = m_MeshRenderer;
                                    break;
                                case MeshFilter m_MeshFilter:
                                    m_GameObject.m_MeshFilter = m_MeshFilter;
                                    break;
                                case SkinnedMeshRenderer m_SkinnedMeshRenderer:
                                    m_GameObject.m_SkinnedMeshRenderer = m_SkinnedMeshRenderer;
                                    break;
                                case Animator m_Animator:
                                    m_GameObject.m_Animator = m_Animator;
                                    break;
                                case Animation m_Animation:
                                    m_GameObject.m_Animation = m_Animation;
                                    break;
                            }
                        }
                    }
                }
                var spriteAtlases = assetsFile.ReadObjects(ClassIDType.SpriteAtlas).Cast<SpriteAtlas>();
                foreach (var m_SpriteAtlas in spriteAtlases)
                {
                    if (tokenSource.IsCancellationRequested)
                    {
                        Logger.Info("Processing assets has been cancelled !!");
                        return;
                    }
                    if (m_SpriteAtlas.m_RenderDataMap.Count > 0)
                    {
                        foreach (var m_PackedSprite in m_SpriteAtlas.m_PackedSprites)
                        {
                            if (m_PackedSprite.TryGet(out var m_Sprite))
                            {
                                if (m_Sprite.m_SpriteAtlas.IsNull)
                                {
                                    m_Sprite.m_SpriteAtlas.Set(m_SpriteAtlas);
                                }
                                else
                                {
                                    m_Sprite.m_SpriteAtlas.TryGet(out var m_SpriteAtlaOld);
                                    if (m_SpriteAtlaOld.m_IsVariant)
                                    {
                                        m_Sprite.m_SpriteAtlas.Set(m_SpriteAtlas);
                                    }
                                }
                            }
                        }
                    }
                }

                RemoveReader(assetsFile.fileName);
            }
        }

        internal FileReader GetReader(string fullPath, Stack<string> paths, Game game, long offset = 0)
        {
            FileReader reader = null;

            try
            {
                reader = new FileReader(fullPath);
                reader = GetReader(reader, paths, game, offset);
            }
            catch (Exception e)
            {
                Logger.Error($"Unable to get reader for {fullPath}!!, {e}");
            }

            return reader;
        }

        internal FileReader GetReader(FileReader reader, Stack<string> paths, Game game, long offset = 0, bool blockFile = true)
        {
            try
            {
                reader = reader.PreProcessing(game, blockFile);

                StreamFile file;
                string dummyPath = string.Empty;
                string fileName = paths.Count == 0 ? string.Empty : paths.Pop();
                switch (reader.FileType)
                {
                    case FileType.BundleFile:
                        reader.Position = offset;
                        var bundleFile = new BundleFile(reader, game);
                        reader.Dispose();

                        file = bundleFile.fileList.FirstOrDefault(x => x.fileName == fileName);
                        dummyPath = Path.Combine(reader.FullPath, file.fileName);
                        reader = new FileReader(dummyPath, file.stream);
                        break;
                    case FileType.WebFile:
                        reader.Position = offset;
                        var webFile = new WebFile(reader);
                        reader.Dispose();

                        file = webFile.fileList.FirstOrDefault(y => y.fileName == fileName);
                        dummyPath = Path.Combine(reader.FullPath, file.fileName);
                        reader = new FileReader(dummyPath, file.stream);
                        reader = GetReader(reader, paths, game, offset);
                        break;
                    case FileType.ZipFile:
                        using (ZipArchive archive = new ZipArchive(reader.BaseStream, ZipArchiveMode.Read))
                        {
                            var targetPath = fileName;
                            ZipArchiveEntry targetEntry = null;
                            while (paths.Count > -1)
                            {
                                targetEntry = archive.Entries.FirstOrDefault(x => x.FullName.Equals(targetPath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), StringComparison.OrdinalIgnoreCase));
                                if (targetEntry != null) break;
                                targetPath = Path.Combine(targetPath, paths.Pop());

                            }
                            if (targetEntry != null)
                            {
                                dummyPath = Path.Combine(Path.GetDirectoryName(reader.FullPath), reader.FileName, targetEntry.FullName);
                                Stream streamReader = new MemoryStream();
                                using (Stream entryStream = targetEntry.Open())
                                {
                                    entryStream.CopyTo(streamReader);
                                }
                                streamReader.Position = 0;

                                FileReader entryReader = new FileReader(dummyPath, streamReader);
                                reader = GetReader(entryReader, paths, game, offset);
                            }
                        }
                        break;
                    case FileType.BlockFile:
                        using (var stream = new OffsetStream(reader.BaseStream, offset))
                        {
                            dummyPath = Path.Combine(reader.FullPath, fileName);
                            var subReader = new FileReader(dummyPath, stream);
                            reader = GetReader(subReader, paths, game, 0, false);
                        }
                        break;
                    case FileType.BlkFile:
                        using (var stream = BlkUtils.Decrypt(reader, (Blk)Game))
                        {
                            stream.Offset = offset;
                            dummyPath = Path.Combine(reader.FullPath, fileName);
                            var subReader = new FileReader(dummyPath, stream);
                            reader = GetReader(subReader, paths, game, 0, false);
                        }
                        break;
                    case FileType.Mhy0File:
                        reader.Position = offset;
                        var mhy0File = new Mhy0File(reader, reader.FullPath, (Mhy0)game);
                        reader.Dispose();

                        file = mhy0File.fileList.FirstOrDefault(x => x.fileName == fileName);
                        dummyPath = Path.Combine(reader.FullPath, file.fileName);
                        reader = new FileReader(dummyPath, file.stream);
                        break;
                    case FileType.BlbFile:
                        reader.Position = offset;
                        var blbFile = new BlbFile(reader, reader.FullPath);
                        reader.Dispose();

                        file = blbFile.fileList.FirstOrDefault(x => x.fileName == fileName);
                        dummyPath = Path.Combine(reader.FullPath, file.fileName);
                        reader = new FileReader(dummyPath, file.stream);
                        break;
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Unable to get reader for {reader.FullPath}!!, {e}");
            }

            return reader;
        }
    }
}
