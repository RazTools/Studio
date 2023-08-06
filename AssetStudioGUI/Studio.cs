using AssetStudio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using static AssetStudioGUI.Exporter;
using Object = AssetStudio.Object;

namespace AssetStudioGUI
{
    internal enum ExportType
    {
        Convert,
        Raw,
        Dump,
        JSON
    }

    internal enum ExportFilter
    {
        All,
        Selected,
        Filtered
    }

    internal static class Studio
    {
        public static Game Game;
        public static bool SkipContainer = false;
        public static AssetsManager assetsManager = new AssetsManager() { CacheObjects = true };
        public static AssemblyLoader assemblyLoader = new AssemblyLoader();
        public static List<AssetItem> exportableAssets = new List<AssetItem>();
        public static List<AssetItem> visibleAssets = new List<AssetItem>();
        internal static Action<string> StatusStripUpdate = x => { };

        public static int ExtractFolder(string path, string savePath)
        {
            int extractedCount = 0;
            Progress.Reset();
            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                var fileOriPath = Path.GetDirectoryName(file);
                var fileSavePath = fileOriPath.Replace(path, savePath);
                extractedCount += ExtractFile(file, fileSavePath);
                Progress.Report(i + 1, files.Length);
            }
            return extractedCount;
        }

        public static int ExtractFile(string[] fileNames, string savePath)
        {
            int extractedCount = 0;
            Progress.Reset();
            for (var i = 0; i < fileNames.Length; i++)
            {
                var fileName = fileNames[i];
                extractedCount += ExtractFile(fileName, savePath);
                Progress.Report(i + 1, fileNames.Length);
            }
            return extractedCount;
        }

        public static int ExtractFile(string fileName, string savePath)
        {
            int extractedCount = 0;
            var reader = new FileReader(fileName);
            reader = reader.PreProcessing(Game);
            if (reader.FileType == FileType.BundleFile)
                extractedCount += ExtractBundleFile(reader, savePath);
            else if (reader.FileType == FileType.WebFile)
                extractedCount += ExtractWebDataFile(reader, savePath);
            else if (reader.FileType == FileType.BlkFile)
                extractedCount += ExtractBlkFile(reader, savePath);
            else if (reader.FileType == FileType.BlockFile)
                extractedCount += ExtractBlockFile(reader, savePath);
            else
                reader.Dispose();
            return extractedCount;
        }

        private static int ExtractBundleFile(FileReader reader, string savePath)
        {
            StatusStripUpdate($"Decompressing {reader.FileName} ...");
            try
            {
                var bundleFile = new BundleFile(reader, Game);
                reader.Dispose();
                if (bundleFile.fileList.Length > 0)
                {
                    var extractPath = Path.Combine(savePath, reader.FileName + "_unpacked");
                    return ExtractStreamFile(extractPath, bundleFile.fileList);
                }
            }
            catch (InvalidCastException)
            {
                Logger.Error($"Game type mismatch, Expected {nameof(Mr0k)} but got {Game.Name} ({Game.GetType().Name}) !!");
            }
            return 0;
        }

        private static int ExtractWebDataFile(FileReader reader, string savePath)
        {
            StatusStripUpdate($"Decompressing {reader.FileName} ...");
            var webFile = new WebFile(reader);
            reader.Dispose();
            if (webFile.fileList.Length > 0)
            {
                var extractPath = Path.Combine(savePath, reader.FileName + "_unpacked");
                return ExtractStreamFile(extractPath, webFile.fileList);
            }
            return 0;
        }

        private static int ExtractBlkFile(FileReader reader, string savePath)
        {
            int total = 0;
            StatusStripUpdate($"Decompressing {reader.FileName} ...");
            try
            {
                using var stream = BlkUtils.Decrypt(reader, (Blk)Game);
                do
                {
                    stream.Offset = stream.AbsolutePosition;
                    var dummyPath = Path.Combine(reader.FullPath, stream.AbsolutePosition.ToString("X8"));
                    var subReader = new FileReader(dummyPath, stream, true);
                    var subSavePath = Path.Combine(savePath, reader.FileName + "_unpacked");
                    switch (subReader.FileType)
                    {
                        case FileType.BundleFile:
                            total += ExtractBundleFile(subReader, subSavePath);
                            break;
                        case FileType.Mhy0File:
                            total += ExtractMhy0File(subReader, subSavePath);
                            break;
                    }
                } while (stream.Remaining > 0);
            }
            catch (InvalidCastException)
            {
                Logger.Error($"Game type mismatch, Expected {nameof(Blk)} but got {Game.Name} ({Game.GetType().Name}) !!");
            }
            return total;
        }

        private static int ExtractBlockFile(FileReader reader, string savePath)
        {
            int total = 0;
            StatusStripUpdate($"Decompressing {reader.FileName} ...");
            using var stream = new OffsetStream(reader.BaseStream, 0);
            do
            {
                stream.Offset = stream.AbsolutePosition;
                var subSavePath = Path.Combine(savePath, reader.FileName + "_unpacked");
                var dummyPath = Path.Combine(reader.FullPath, stream.AbsolutePosition.ToString("X8"));
                var subReader = new FileReader(dummyPath, stream, true);
                total += ExtractBundleFile(subReader, subSavePath);
            } while (stream.Remaining > 0);
            return total;
        }

        private static int ExtractMhy0File(FileReader reader, string savePath)
        {
            StatusStripUpdate($"Decompressing {reader.FileName} ...");
            try
            {
                var mhy0File = new Mhy0File(reader, reader.FullPath, (Mhy0)Game);
                reader.Dispose();
                if (mhy0File.fileList.Length > 0)
                {
                    var extractPath = Path.Combine(savePath, reader.FileName + "_unpacked");
                    return ExtractStreamFile(extractPath, mhy0File.fileList);
                }
            }
            catch (InvalidCastException)
            {
                Logger.Error($"Game type mismatch, Expected {nameof(Mhy0)} but got {Game.Name} ({Game.GetType().Name}) !!");
            }
            return 0;
        }

        private static int ExtractStreamFile(string extractPath, StreamFile[] fileList)
        {
            int extractedCount = 0;
            foreach (var file in fileList)
            {
                var filePath = Path.Combine(extractPath, file.path);
                var fileDirectory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(fileDirectory))
                {
                    Directory.CreateDirectory(fileDirectory);
                }
                if (!File.Exists(filePath))
                {
                    using (var fileStream = File.Create(filePath))
                    {
                        file.stream.CopyTo(fileStream);
                    }
                    extractedCount += 1;
                }
                file.stream.Dispose();
            }
            return extractedCount;
        }

        public static void UpdateContainers()
        {
            if (exportableAssets.Count > 0)
            {
                Logger.Info("Updating Containers...");
                foreach (var asset in exportableAssets)
                {
                    if (int.TryParse(asset.Container, out var value))
                    {
                        var last = unchecked((uint)value);
                        var name = Path.GetFileNameWithoutExtension(asset.SourceFile.originalPath);
                        if (uint.TryParse(name, out var id))
                        {
                            var path = ResourceIndex.GetContainer(id, last);
                            if (!string.IsNullOrEmpty(path))
                            {
                                asset.Container = path;
                                if (asset.Type == ClassIDType.MiHoYoBinData)
                                {
                                    asset.Text = Path.GetFileNameWithoutExtension(path);
                                }
                            }
                        }
                    }
                }
                Logger.Info("Updated !!");
            }
        }

        public static (string, List<TreeNode>) BuildAssetData()
        {
            StatusStripUpdate("Building asset list...");

            int i = 0;
            string productName = null;
            var objectCount = assetsManager.assetsFileList.Sum(x => x.m_Objects.Count);
            var objectAssetItemDic = new Dictionary<ObjectInfo, AssetItem>(objectCount);
            var mihoyoBinDataNames = new List<(PPtr<Object>, string)>();
            var containers = new List<(PPtr<Object>, string)>();
            Progress.Reset();
            foreach (var assetsFile in assetsManager.assetsFileList)
            {
                foreach (var objInfo in assetsFile.m_Objects)
                {
                    if (assetsManager.tokenSource.IsCancellationRequested)
                    {
                        Logger.Info("Building asset list has been cancelled !!");
                        return (string.Empty, Array.Empty<TreeNode>().ToList());
                    }
                    var assetItem = new AssetItem(objInfo, assetsFile);
                    objectAssetItemDic.Add(objInfo, assetItem);
                    assetItem.UniqueID = "#" + i;
                    var exportable = false;
                    switch (objInfo.type)
                    {
                        case ClassIDType.Texture2D:
                            var m_Texture2D = assetsFile.ReadObject(objInfo) as Texture2D;
                            if (!string.IsNullOrEmpty(m_Texture2D.m_StreamData?.path))
                                assetItem.FullSize = objInfo.byteSize + m_Texture2D.m_StreamData.size;
                            exportable = true;
                            goto case ClassIDType.NamedObject;
                        case ClassIDType.AudioClip:
                            var m_AudioClip = assetsFile.ReadObject(objInfo) as AudioClip;
                            if (!string.IsNullOrEmpty(m_AudioClip.m_Source))
                                assetItem.FullSize = objInfo.byteSize + m_AudioClip.m_Size;
                            exportable = true;
                            goto case ClassIDType.NamedObject;
                        case ClassIDType.VideoClip:
                            var m_VideoClip = assetsFile.ReadObject(objInfo) as VideoClip;
                            if (!string.IsNullOrEmpty(m_VideoClip.m_OriginalPath))
                                assetItem.FullSize = objInfo.byteSize + (long)m_VideoClip.m_ExternalResources.m_Size;
                            exportable = true;
                            goto case ClassIDType.NamedObject;
                        case ClassIDType.Shader:
                        case ClassIDType.Mesh:
                        case ClassIDType.TextAsset:
                        case ClassIDType.AnimationClip:
                        case ClassIDType.Font:
                        case ClassIDType.MovieTexture:
                        case ClassIDType.Sprite:
                        case ClassIDType.Animator:
                        case ClassIDType.MonoBehaviour:
                        case ClassIDType.MiHoYoBinData:
                            exportable = true;
                            goto case ClassIDType.NamedObject;
                        case ClassIDType.AssetBundle:
                        case ClassIDType.IndexObject:
                        case ClassIDType.GameObject:
                        case ClassIDType.PlayerSettings:
                        case ClassIDType.ResourceManager:
                        case ClassIDType.NamedObject:
                            assetItem.Text = assetsFile.ReadObjectName(objInfo, skipContainer: SkipContainer);
                            break;
                            }
                    if (assetItem.Text == "")
                    {
                        assetItem.Text = assetItem.TypeString + assetItem.UniqueID;
                    }
                    if (Properties.Settings.Default.displayAll || exportable)
                    {
                        exportableAssets.Add(assetItem);
                    }
                    Progress.Report(++i, objectCount);
                }
                assetsManager.RemoveReader(assetsFile.fileName);
                    }
            if (!SkipContainer)
            {
                if (Game.Type.IsGISubGroup())
                {
                    UpdateContainers();
                }
            }
            foreach (var tmp in exportableAssets)
            {
                if (assetsManager.tokenSource.IsCancellationRequested)
                {
                    Logger.Info("Processing subitems been cancelled !!");
                    return (string.Empty, Array.Empty<TreeNode>().ToList());
                }
                tmp.SetSubItems();
            }

            visibleAssets = exportableAssets;

            StatusStripUpdate("Building tree structure...");

            var treeNodeCollection = new List<TreeNode>();
            var treeNodeDictionary = new Dictionary<ObjectInfo, GameObjectTreeNode>();
            var assetsFileCount = assetsManager.assetsFileList.Count;
            int j = 0;
            Progress.Reset();
            foreach (var assetsFile in assetsManager.assetsFileList)
            {
                var fileNode = new TreeNode(assetsFile.fileName); //RootNode

                foreach (var objInfo in assetsFile.m_Objects)
                {
                    if (assetsManager.tokenSource.IsCancellationRequested)
                    {
                        Logger.Info("Building tree structure been cancelled !!");
                        return (string.Empty, Array.Empty<TreeNode>().ToList());
                    }

                    if (objInfo.type == ClassIDType.GameObject)
                    {
                        if (!treeNodeDictionary.TryGetValue(objInfo, out var currentNode))
                        {
                            currentNode = new GameObjectTreeNode(objInfo, assetsFile);
                            treeNodeDictionary.Add(objInfo, currentNode);
                        }

                        var m_GameObject = currentNode.gameObject;
                        foreach (var pptr in m_GameObject.m_Components)
                        {
                            if (pptr.TryGet(out var m_Component))
                            {
                                objectAssetItemDic[m_Component.objInfo].TreeNode = currentNode;
                                if (m_Component is MeshFilter m_MeshFilter)
                                {
                                    if (m_MeshFilter.m_Mesh.TryGetInfo(out var m_Mesh))
                                    {
                                        objectAssetItemDic[m_Mesh].TreeNode = currentNode;
                                    }
                                }
                                else if (m_Component is SkinnedMeshRenderer m_SkinnedMeshRenderer)
                                {
                                    if (m_SkinnedMeshRenderer.m_Mesh.TryGetInfo(out var m_Mesh))
                                    {
                                        objectAssetItemDic[m_Mesh].TreeNode = currentNode;
                                    }
                                }
                            }
                        }

                        var parentNode = fileNode;

                        if (m_GameObject.m_Transform != null)
                        {
                            if (m_GameObject.m_Transform.m_Father.TryGet(out var m_Father))
                            {
                                if (m_Father.m_GameObject.TryGetInfo(out var parentGameObject))
                                {
                                    if (!treeNodeDictionary.TryGetValue(parentGameObject, out var parentGameObjectNode))
                                    {
                                        parentGameObjectNode = new GameObjectTreeNode(parentGameObject, assetsFile);
                                        treeNodeDictionary.Add(parentGameObject, parentGameObjectNode);
                                    }
                                    parentNode = parentGameObjectNode;
                                }
                            }
                        }

                        parentNode.Nodes.Add(currentNode);
                    }
                }

                if (fileNode.Nodes.Count > 0)
                {
                    treeNodeCollection.Add(fileNode);
                }

                assetsManager.RemoveReader(assetsFile.fileName);
                Progress.Report(++j, assetsFileCount);
            }
            treeNodeDictionary.Clear();

            objectAssetItemDic.Clear();

            return (productName, treeNodeCollection);
        }

        public static Dictionary<string, SortedDictionary<int, TypeTreeItem>> BuildClassStructure()
        {
            var typeMap = new Dictionary<string, SortedDictionary<int, TypeTreeItem>>();
            foreach (var assetsFile in assetsManager.assetsFileList)
            {
                if (assetsManager.tokenSource.IsCancellationRequested)
                {
                    Logger.Info("Processing class structure been cancelled !!");
                    return new Dictionary<string, SortedDictionary<int, TypeTreeItem>>();
                }
                if (typeMap.TryGetValue(assetsFile.unityVersion, out var curVer))
                {
                    foreach (var type in assetsFile.m_Types.Where(x => x.m_Type != null))
                    {
                        var key = type.classID;
                        if (type.m_ScriptTypeIndex >= 0)
                        {
                            key = -1 - type.m_ScriptTypeIndex;
                        }
                        curVer[key] = new TypeTreeItem(key, type.m_Type);
                    }
                }
                else
                {
                    var items = new SortedDictionary<int, TypeTreeItem>();
                    foreach (var type in assetsFile.m_Types.Where(x => x.m_Type != null))
                    {
                        var key = type.classID;
                        if (type.m_ScriptTypeIndex >= 0)
                        {
                            key = -1 - type.m_ScriptTypeIndex;
                        }
                        items[key] = new TypeTreeItem(key, type.m_Type);
                    }
                    typeMap.Add(assetsFile.unityVersion, items);
                }
            }

            return typeMap;
        }

        public static void ExportAssets(string savePath, List<AssetItem> toExportAssets, ExportType exportType)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

                int toExportCount = toExportAssets.Count;
                int exportedCount = 0;
                int i = 0;
                Progress.Reset();
                foreach (var asset in toExportAssets)
                {
                    string exportPath;
                    switch ((AssetGroupOption)Properties.Settings.Default.assetGroupOption)
                    {
                        case AssetGroupOption.ByType: //type name
                            exportPath = Path.Combine(savePath, asset.TypeString);
                            break;
                        case AssetGroupOption.ByContainer: //container path
                            if (!string.IsNullOrEmpty(asset.Container))
                            {
                                exportPath = Path.Combine(savePath, Path.GetDirectoryName(asset.Container));
                            }
                            else
                            {
                                exportPath = savePath;
                            }
                            break;
                        case AssetGroupOption.BySource: //source file
                            if (string.IsNullOrEmpty(asset.SourceFile.originalPath))
                            {
                                exportPath = Path.Combine(savePath, asset.SourceFile.fileName + "_export");
                            }
                            else
                            {
                                exportPath = Path.Combine(savePath, Path.GetFileName(asset.SourceFile.originalPath) + "_export", asset.SourceFile.fileName);
                            }
                            break;
                        default:
                            exportPath = savePath;
                            break;
                    }
                    exportPath += Path.DirectorySeparatorChar;
                    StatusStripUpdate($"[{exportedCount}/{toExportCount}] Exporting {asset.TypeString}: {asset.Text}");
                    try
                    {
                        switch (exportType)
                        {
                            case ExportType.Raw:
                                if (ExportRawFile(asset, exportPath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ExportType.Dump:
                                if (ExportDumpFile(asset, exportPath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ExportType.Convert:
                                if (ExportConvertFile(asset, exportPath))
                                {
                                    exportedCount++;
                                }
                                break;
                            case ExportType.JSON:
                                if (ExportJSONFile(asset, exportPath))
                                {
                                    exportedCount++;
                                }
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Export {asset.Type}:{asset.Text} error\r\n{ex.Message}\r\n{ex.StackTrace}");
                    }

                    Progress.Report(++i, toExportCount);
                }

                var statusText = exportedCount == 0 ? "Nothing exported." : $"Finished exporting {exportedCount} assets.";

                if (toExportCount > exportedCount)
                {
                    statusText += $" {toExportCount - exportedCount} assets skipped (not extractable or files already exist)";
                }

                StatusStripUpdate(statusText);

                if (Properties.Settings.Default.openAfterExport && exportedCount > 0)
                {
                    OpenFolderInExplorer(savePath);
                }
            });
        }

        public static void ExportAssetsList(string savePath, List<AssetItem> toExportAssets, ExportListType exportListType)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

                Progress.Reset();

                switch (exportListType)
                {
                    case ExportListType.XML:
                        var filename = Path.Combine(savePath, "assets.xml");
                        var settings = new XmlWriterSettings() { Indent = true };
                        using (XmlWriter writer = XmlWriter.Create(filename, settings))
                        {
                            writer.WriteStartDocument();
                            writer.WriteStartElement("Assets");
                            writer.WriteAttributeString("filename", filename);
                            writer.WriteAttributeString("createdAt", DateTime.UtcNow.ToString("s"));
                            foreach (var asset in toExportAssets)
                            {
                                writer.WriteStartElement("Asset");
                                writer.WriteElementString("Name", asset.Name);
                                writer.WriteElementString("Container", asset.Container);
                                writer.WriteStartElement("Type");
                                writer.WriteAttributeString("id", ((int)asset.Type).ToString());
                                writer.WriteValue(asset.TypeString);
                                writer.WriteEndElement();
                                writer.WriteElementString("PathID", asset.m_PathID.ToString());
                                writer.WriteElementString("Source", asset.SourceFile.fullName);
                                writer.WriteElementString("Size", asset.FullSize.ToString());
                                writer.WriteEndElement();
                            }
                            writer.WriteEndElement();
                            writer.WriteEndDocument();
                        }
                        break;
                }

                var statusText = $"Finished exporting asset list with {toExportAssets.Count()} items.";

                StatusStripUpdate(statusText);

                if (Properties.Settings.Default.openAfterExport && toExportAssets.Count() > 0)
                {
                    OpenFolderInExplorer(savePath);
                }
            });
        }

        public static void ExportSplitObjects(string savePath, TreeNodeCollection nodes)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                var count = nodes.Cast<TreeNode>().Sum(x => x.Nodes.Count);
                int k = 0;
                Progress.Reset();
                foreach (TreeNode node in nodes)
                {
                    //遍历一级子节点
                    foreach (GameObjectTreeNode j in node.Nodes)
                    {
                        //收集所有子节点
                        var gameObjects = new List<GameObject>();
                        CollectNode(j, gameObjects);
                        //跳过一些不需要导出的object
                        if (gameObjects.All(x => x.m_SkinnedMeshRenderer == null && x.m_MeshFilter == null))
                        {
                            Progress.Report(++k, count);
                            continue;
                        }
                        //处理非法文件名
                        var filename = FixFileName(j.Text);
                        //每个文件存放在单独的文件夹
                        var targetPath = $"{savePath}{filename}{Path.DirectorySeparatorChar}";
                        //重名文件处理
                        for (int i = 1; ; i++)
                        {
                            if (Directory.Exists(targetPath))
                            {
                                targetPath = $"{savePath}{filename} ({i}){Path.DirectorySeparatorChar}";
                            }
                            else
                            {
                                break;
                            }
                        }
                        Directory.CreateDirectory(targetPath);
                        //导出FBX
                        StatusStripUpdate($"Exporting {filename}.fbx");
                        try
                        {
                            ExportGameObject(j.gameObject, targetPath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Export GameObject:{j.Text} error\r\n{ex.Message}\r\n{ex.StackTrace}");
                        }

                        Progress.Report(++k, count);
                        StatusStripUpdate($"Finished exporting {filename}.fbx");
                    }
                }
                if (Properties.Settings.Default.openAfterExport)
                {
                    OpenFolderInExplorer(savePath);
                }
                StatusStripUpdate("Finished");
            });
        }

        private static void CollectNode(GameObjectTreeNode node, List<GameObject> gameObjects)
        {
            gameObjects.Add(node.gameObject);
            foreach (GameObjectTreeNode i in node.Nodes)
            {
                CollectNode(i, gameObjects);
            }
        }

        public static void ExportAnimatorWithAnimationClip(AssetItem animator, List<AssetItem> animationList, string exportPath)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                Progress.Reset();
                StatusStripUpdate($"Exporting {animator.Text}");
                try
                {
                    ExportAnimator(animator, exportPath, animationList);
                    if (Properties.Settings.Default.openAfterExport)
                    {
                        OpenFolderInExplorer(exportPath);
                    }
                    Progress.Report(1, 1);
                    StatusStripUpdate($"Finished exporting {animator.Text}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Export Animator:{animator.Text} error\r\n{ex.Message}\r\n{ex.StackTrace}");
                    StatusStripUpdate("Error in export");
                }
            });
        }

        public static void ExportObjectsWithAnimationClip(string exportPath, TreeNodeCollection nodes, List<AssetItem> animationList = null)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                var gameObjects = new List<GameObject>();
                GetSelectedParentNode(nodes, gameObjects);
                if (gameObjects.Count > 0)
                {
                    var count = gameObjects.Count;
                    int i = 0;
                    Progress.Reset();
                    foreach (var gameObject in gameObjects)
                    {
                        StatusStripUpdate($"Exporting {gameObject.m_Name}");
                        try
                        {
                            var subExportPath = Path.Combine(exportPath, gameObject.m_Name) + Path.DirectorySeparatorChar;
                            ExportGameObject(gameObject, subExportPath, animationList);
                            StatusStripUpdate($"Finished exporting {gameObject.m_Name}");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Export GameObject:{gameObject.m_Name} error\r\n{ex.Message}\r\n{ex.StackTrace}");
                            StatusStripUpdate("Error in export");
                        }

                        Progress.Report(++i, count);
                    }
                    if (Properties.Settings.Default.openAfterExport)
                    {
                        OpenFolderInExplorer(exportPath);
                    }
                }
                else
                {
                    StatusStripUpdate("No Object selected for export.");
                }
            });
        }

        public static void ExportObjectsMergeWithAnimationClip(string exportPath, List<GameObject> gameObjects, List<AssetItem> animationList = null)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                var name = Path.GetFileName(exportPath);
                Progress.Reset();
                StatusStripUpdate($"Exporting {name}");
                try
                {
                    ExportGameObjectMerge(gameObjects, exportPath, animationList);
                    Progress.Report(1, 1);
                    StatusStripUpdate($"Finished exporting {name}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Export Model:{name} error\r\n{ex.Message}\r\n{ex.StackTrace}");
                    StatusStripUpdate("Error in export");
                }
                if (Properties.Settings.Default.openAfterExport)
                {
                    OpenFolderInExplorer(Path.GetDirectoryName(exportPath));
                }
            });
        }

        public static void ExportNodesWithAnimationClip(string exportPath, List<TreeNode> nodes, List<AssetItem> animationList = null)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                int i = 0;
                Progress.Reset();
                foreach (var node in nodes)
                {
                    var name = node.Text;
                    StatusStripUpdate($"Exporting {name}");
                    var gameObjects = new List<GameObject>();
                    GetSelectedParentNode(node.Nodes, gameObjects);
                    if (gameObjects.Count > 0)
                    {
                        var subExportPath = exportPath + Path.Combine(node.Text, FixFileName(node.Text) + ".fbx");
                        try
                        {
                            ExportGameObjectMerge(gameObjects, subExportPath, animationList);
                            Progress.Report(++i, nodes.Count);
                            StatusStripUpdate($"Finished exporting {name}");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Export Model:{name} error\r\n{ex.Message}\r\n{ex.StackTrace}");
                            StatusStripUpdate("Error in export");
                        }
                    }
                    else
                    {
                        StatusStripUpdate("Empty node selected for export.");
                    }
                }
                if (Properties.Settings.Default.openAfterExport)
                {
                    OpenFolderInExplorer(exportPath);
                }
            });
        }

        public static void GetSelectedParentNode(TreeNodeCollection nodes, List<GameObject> gameObjects)
        {
            foreach (TreeNode i in nodes)
            {
                if (i is GameObjectTreeNode gameObjectTreeNode && i.Checked)
                {
                    gameObjects.Add(gameObjectTreeNode.gameObject);
                }
                else
                {
                    GetSelectedParentNode(i.Nodes, gameObjects);
                }
            }
        }

        public static TypeTree MonoBehaviourToTypeTree(MonoBehaviour m_MonoBehaviour)
        {
            if (!assemblyLoader.Loaded)
            {
                var openFolderDialog = new OpenFolderDialog();
                openFolderDialog.Title = "Select Assembly Folder";
                if (openFolderDialog.ShowDialog() == DialogResult.OK)
                {
                    assemblyLoader.Load(openFolderDialog.Folder);
                }
                else
                {
                    assemblyLoader.Loaded = true;
                }
            }
            return m_MonoBehaviour.ConvertToTypeTree(assemblyLoader);
        }

        public static string DumpAsset(Object obj)
        {
            var str = obj.Dump();
            if (str == null && obj is MonoBehaviour m_MonoBehaviour)
            {
                var type = MonoBehaviourToTypeTree(m_MonoBehaviour);
                str = m_MonoBehaviour.Dump(type);
            }
            return str;
        }

        public static void OpenFolderInExplorer(string path)
        {
            var info = new ProcessStartInfo(path);
            info.UseShellExecute = true;
            Process.Start(info);
        }
    }
}
