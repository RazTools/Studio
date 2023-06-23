using K4os.Compression.LZ4;
using ZstdSharp;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AssetStudio
{
    [Flags]
    public enum ArchiveFlags
    {
        CompressionTypeMask = 0x3f,
        BlocksAndDirectoryInfoCombined = 0x40,
        BlocksInfoAtTheEnd = 0x80,
        OldWebPluginCompatibility = 0x100,
        BlockInfoNeedPaddingAtStart = 0x200,
    }

    [Flags]
    public enum StorageBlockFlags
    {
        CompressionTypeMask = 0x3f,
        Streamed = 0x40,
    }

    public enum CompressionType
    {
        None,
        Lzma,
        Lz4,
        Lz4HC,
        Lzham,
        Lz4Mr0k,
        Zstd = 5
    }

    public class BundleFile
    {
        public class Header
        {
            public string signature;
            public uint version;
            public string unityVersion;
            public string unityRevision;
            public long size;
            public uint compressedBlocksInfoSize;
            public uint uncompressedBlocksInfoSize;
            public ArchiveFlags flags;
        }

        public class StorageBlock
        {
            public uint compressedSize;
            public uint uncompressedSize;
            public StorageBlockFlags flags;
        }

        public class Node
        {
            public long offset;
            public long size;
            public uint flags;
            public string path;
        }

        private Game Game;

        public Header m_Header;
        private Node[] m_DirectoryInfo;
        private StorageBlock[] m_BlocksInfo;

        public StreamFile[] fileList;

        public BundleFile(FileReader reader, Game game)
        {
            Game = game;
            m_Header = ReadBundleHeader(reader);
            switch (m_Header.signature)
            {
                case "UnityArchive":
                    break; //TODO
                case "UnityWeb":
                case "UnityRaw":
                    if (m_Header.version == 6)
                    {
                        goto case "UnityFS";
                    }
                    ReadHeaderAndBlocksInfo(reader);
                    using (var blocksStream = CreateBlocksStream(reader.FullPath))
                    {
                        ReadBlocksAndDirectory(reader, blocksStream);
                        ReadFiles(blocksStream, reader.FullPath);
                    }
                    break;
                case "UnityFS":
                case "ENCR":
                    ReadHeader(reader);
                    ReadBlocksInfoAndDirectory(reader);
                    using (var blocksStream = CreateBlocksStream(reader.FullPath))
                    {
                        ReadBlocks(reader, blocksStream);
                        ReadFiles(blocksStream, reader.FullPath);
                    }
                    break;
            }
        }

        private Header ReadBundleHeader(FileReader reader)
        {
            Header header = new Header();
            header.signature = reader.ReadStringToNull(20);
            switch (header.signature)
            {
                case "UnityFS":
                    if (Game.Type.IsBH3Group())
                    {
                        var version = reader.ReadUInt32();
                        if (version > 11)
                        {
                            XORShift128.InitSeed(version);
                            header.version = 6;
                            header.unityVersion = "5.x.x";
                            header.unityRevision = "2017.4.18f1";
                        }
                        else
                        {
                            reader.Position -= 4;
                            goto default;
                        }
                    }
                    else
                    {
                        header.version = reader.ReadUInt32();
                        header.unityVersion = reader.ReadStringToNull();
                        header.unityRevision = reader.ReadStringToNull();
                    }
                    break;
                case "ENCR":
                    header.version = 6; // is 7 but does not have uncompressedDataHash
                    header.unityVersion = "5.x.x";
                    header.unityRevision = "2019.4.32f1";
                    break;
                default:
                    if (Game.Type.IsNaraka())
                    {
                        header.signature = "UnityFS";
                        goto case "UnityFS";
                    }
                    break;

            }

            return header;
        }

        private void ReadHeaderAndBlocksInfo(FileReader reader)
        {
            if (m_Header.version >= 4)
            {
                var hash = reader.ReadBytes(16);
                var crc = reader.ReadUInt32();
            }
            var minimumStreamedBytes = reader.ReadUInt32();
            m_Header.size = reader.ReadUInt32();
            var numberOfLevelsToDownloadBeforeStreaming = reader.ReadUInt32();
            var levelCount = reader.ReadInt32();
            m_BlocksInfo = new StorageBlock[1];
            for (int i = 0; i < levelCount; i++)
            {
                var storageBlock = new StorageBlock()
                {
                    compressedSize = reader.ReadUInt32(),
                    uncompressedSize = reader.ReadUInt32(),
                };
                if (i == levelCount - 1)
                {
                    m_BlocksInfo[0] = storageBlock;
                }
            }
            if (m_Header.version >= 2)
            {
                var completeFileSize = reader.ReadUInt32();
            }
            if (m_Header.version >= 3)
            {
                var fileInfoHeaderSize = reader.ReadUInt32();
            }
            reader.Position = m_Header.size;
        }

        private Stream CreateBlocksStream(string path)
        {
            Stream blocksStream;
            var uncompressedSizeSum = m_BlocksInfo.Sum(x => x.uncompressedSize);
            if (uncompressedSizeSum >= int.MaxValue)
            {
                /*var memoryMappedFile = MemoryMappedFile.CreateNew(null, uncompressedSizeSum);
                assetsDataStream = memoryMappedFile.CreateViewStream();*/
                blocksStream = new FileStream(path + ".temp", FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
            }
            else
            {
                blocksStream = new MemoryStream((int)uncompressedSizeSum);
            }
            return blocksStream;
        }

        private void ReadBlocksAndDirectory(FileReader reader, Stream blocksStream)
        {
            var isCompressed = m_Header.signature == "UnityWeb";
            foreach (var blockInfo in m_BlocksInfo)
            {
                var uncompressedBytes = reader.ReadBytes((int)blockInfo.compressedSize);
                if (isCompressed)
                {
                    using (var memoryStream = new MemoryStream(uncompressedBytes))
                    {
                        using (var decompressStream = SevenZipHelper.StreamDecompress(memoryStream))
                        {
                            uncompressedBytes = decompressStream.ToArray();
                        }
                    }
                }
                blocksStream.Write(uncompressedBytes, 0, uncompressedBytes.Length);
            }
            blocksStream.Position = 0;
            var blocksReader = new EndianBinaryReader(blocksStream);
            var nodesCount = blocksReader.ReadInt32();
            m_DirectoryInfo = new Node[nodesCount];
            for (int i = 0; i < nodesCount; i++)
            {
                m_DirectoryInfo[i] = new Node
                {
                    path = blocksReader.ReadStringToNull(),
                    offset = blocksReader.ReadUInt32(),
                    size = blocksReader.ReadUInt32()
                };
            }
        }

        public void ReadFiles(Stream blocksStream, string path)
        {
            fileList = new StreamFile[m_DirectoryInfo.Length];
            for (int i = 0; i < m_DirectoryInfo.Length; i++)
            {
                var node = m_DirectoryInfo[i];
                var file = new StreamFile();
                fileList[i] = file;
                file.path = node.path;
                file.fileName = Path.GetFileName(node.path);
                if (node.size >= int.MaxValue)
                {
                    /*var memoryMappedFile = MemoryMappedFile.CreateNew(null, entryinfo_size);
                    file.stream = memoryMappedFile.CreateViewStream();*/
                    var extractPath = path + "_unpacked" + Path.DirectorySeparatorChar;
                    Directory.CreateDirectory(extractPath);
                    file.stream = new FileStream(extractPath + file.fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                }
                else
                {
                    file.stream = new MemoryStream((int)node.size);
                }
                blocksStream.Position = node.offset;
                blocksStream.CopyTo(file.stream, node.size);
                file.stream.Position = 0;
            }
        }

        private void DecryptHeader()
        {
            m_Header.flags ^= (ArchiveFlags)XORShift128.NextDecryptInt();
            m_Header.size ^= XORShift128.NextDecryptLong();
            m_Header.uncompressedBlocksInfoSize ^= XORShift128.NextDecryptUInt();
            m_Header.compressedBlocksInfoSize ^= XORShift128.NextDecryptUInt();
            XORShift128.Init = false;
        }

        private void ReadHeader(FileReader reader)
        {
            if ((Game.Type.IsBH3Group()) && XORShift128.Init)
            {
                m_Header.flags = (ArchiveFlags)reader.ReadUInt32();
                m_Header.size = reader.ReadInt64();
                m_Header.uncompressedBlocksInfoSize = reader.ReadUInt32();
                m_Header.compressedBlocksInfoSize = reader.ReadUInt32();
                DecryptHeader();

                var encUnityVersion = reader.ReadStringToNull();
                var encUnityRevision = reader.ReadStringToNull();
                return;
            }

            m_Header.size = reader.ReadInt64();
            m_Header.compressedBlocksInfoSize = reader.ReadUInt32();
            m_Header.uncompressedBlocksInfoSize = reader.ReadUInt32();
            m_Header.flags = (ArchiveFlags)reader.ReadUInt32();
            if (m_Header.signature != "UnityFS" && !Game.Type.IsSRGroup())
            {
                reader.ReadByte();
            }

            if (Game.Type.IsNaraka())
            {
                m_Header.compressedBlocksInfoSize -= 0xCA;
                m_Header.uncompressedBlocksInfoSize -= 0xCA;
            }
        }

        private void ReadBlocksInfoAndDirectory(FileReader reader)
        {
            byte[] blocksInfoBytes;
            if (m_Header.version >= 7 && !Game.Type.IsSRGroup())
            {
                reader.AlignStream(16);
            }
            if ((m_Header.flags & ArchiveFlags.BlocksInfoAtTheEnd) != 0) //kArchiveBlocksInfoAtTheEnd
            {
                var position = reader.Position;
                reader.Position = reader.BaseStream.Length - m_Header.compressedBlocksInfoSize;
                blocksInfoBytes = reader.ReadBytes((int)m_Header.compressedBlocksInfoSize);
                reader.Position = position;
            }
            else //0x40 BlocksAndDirectoryInfoCombined
            {
                blocksInfoBytes = reader.ReadBytes((int)m_Header.compressedBlocksInfoSize);
            }
            MemoryStream blocksInfoUncompresseddStream;
            var blocksInfoBytesSpan = blocksInfoBytes.AsSpan();
            var uncompressedSize = m_Header.uncompressedBlocksInfoSize;
            var compressionType = (CompressionType)(m_Header.flags & ArchiveFlags.CompressionTypeMask);
            switch (compressionType) //kArchiveCompressionTypeMask
            {
                case CompressionType.None: //None
                    {
                        blocksInfoUncompresseddStream = new MemoryStream(blocksInfoBytes);
                        break;
                    }
                case CompressionType.Lzma: //LZMA
                    {
                        blocksInfoUncompresseddStream = new MemoryStream((int)(uncompressedSize));
                        using (var blocksInfoCompressedStream = new MemoryStream(blocksInfoBytes))
                        {
                            SevenZipHelper.StreamDecompress(blocksInfoCompressedStream, blocksInfoUncompresseddStream, m_Header.compressedBlocksInfoSize, m_Header.uncompressedBlocksInfoSize);
                        }
                        blocksInfoUncompresseddStream.Position = 0;
                        break;
                    }
                case CompressionType.Lz4: //LZ4
                case CompressionType.Lz4HC: //LZ4HC
                    {
                        var uncompressedBytes = new byte[uncompressedSize];
                        var numWrite = LZ4Codec.Decode(blocksInfoBytesSpan, uncompressedBytes);
                        if (numWrite != uncompressedSize)
                        {
                            throw new IOException($"Lz4 decompression error, write {numWrite} bytes but expected {uncompressedSize} bytes");
                        }
                        blocksInfoUncompresseddStream = new MemoryStream(uncompressedBytes);
                        break;
                    }
                case CompressionType.Lz4Mr0k: //Lz4Mr0k
                    if (Mr0kUtils.IsMr0k(blocksInfoBytesSpan))
                    {
                        blocksInfoBytesSpan = Mr0kUtils.Decrypt(blocksInfoBytesSpan, (Mr0k)Game).ToArray();
                    }
                    goto case CompressionType.Lz4HC;
                default:
                    throw new IOException($"Unsupported compression type {compressionType}");
            }
            using (var blocksInfoReader = new EndianBinaryReader(blocksInfoUncompresseddStream))
            {
                if (m_Header.version >= 7 || !Game.Type.IsSRGroup())
                {
                    var uncompressedDataHash = blocksInfoReader.ReadBytes(16);
                }
                var blocksInfoCount = blocksInfoReader.ReadInt32();
                m_BlocksInfo = new StorageBlock[blocksInfoCount];
                for (int i = 0; i < blocksInfoCount; i++)
                {
                    m_BlocksInfo[i] = new StorageBlock
                    {
                        uncompressedSize = blocksInfoReader.ReadUInt32(),
                        compressedSize = blocksInfoReader.ReadUInt32(),
                        flags = (StorageBlockFlags)blocksInfoReader.ReadUInt16()
                    };
                }

                var nodesCount = blocksInfoReader.ReadInt32();
                m_DirectoryInfo = new Node[nodesCount];
                for (int i = 0; i < nodesCount; i++)
                {
                    m_DirectoryInfo[i] = new Node
                    {
                        offset = blocksInfoReader.ReadInt64(),
                        size = blocksInfoReader.ReadInt64(),
                        flags = blocksInfoReader.ReadUInt32(),
                        path = blocksInfoReader.ReadStringToNull(),
                    };
                }
            }
            if ((m_Header.flags & ArchiveFlags.BlockInfoNeedPaddingAtStart) != 0)
            {
                reader.AlignStream(16);
            }
        }

        private void ReadBlocks(FileReader reader, Stream blocksStream)
        {
            for (int i = 0; i < m_BlocksInfo.Length; i++)
            {
                var blockInfo = m_BlocksInfo[i];
                var compressionType = (CompressionType)(blockInfo.flags & StorageBlockFlags.CompressionTypeMask);
                switch (compressionType) //kStorageBlockCompressionTypeMask
                {
                    case CompressionType.None: //None
                        {
                            reader.BaseStream.CopyTo(blocksStream, blockInfo.compressedSize);
                            break;
                        }
                    case CompressionType.Lzma: //LZMA
                        {
                            SevenZipHelper.StreamDecompress(reader.BaseStream, blocksStream, blockInfo.compressedSize, blockInfo.uncompressedSize);
                            break;
                        }
                    case CompressionType.Lz4: //LZ4
                    case CompressionType.Lz4HC: //LZ4HC
                    case CompressionType.Lz4Mr0k when Game.Type.IsMhyGroup(): //Lz4Mr0k
                        {
                            var compressedSize = (int)blockInfo.compressedSize;
                            var compressedBytes = BigArrayPool<byte>.Shared.Rent(compressedSize);
                            reader.Read(compressedBytes, 0, compressedSize);
                            var compressedBytesSpan = compressedBytes.AsSpan(0, compressedSize);
                            if (compressionType == CompressionType.Lz4Mr0k && Mr0kUtils.IsMr0k(compressedBytes))
                            {
                                compressedBytesSpan = Mr0kUtils.Decrypt(compressedBytesSpan, (Mr0k)Game);
                            }
                            if (Game.Type.IsOPFP())
                            {
                                OPFPUtils.Decrypt(compressedBytesSpan, reader.FullPath);
                            }
                            if (Game.Type.IsNetEase() && i == 0)
                            {
                                NetEaseUtils.Decrypt(compressedBytesSpan);
                            }
                            var uncompressedSize = (int)blockInfo.uncompressedSize;
                            var uncompressedBytes = BigArrayPool<byte>.Shared.Rent(uncompressedSize);
                            var uncompressedBytesSpan = uncompressedBytes.AsSpan(0, uncompressedSize);
                            var numWrite = LZ4Codec.Decode(compressedBytesSpan, uncompressedBytesSpan);
                            if (numWrite != uncompressedSize)
                            {
                                throw new IOException($"Lz4 decompression error, write {numWrite} bytes but expected {uncompressedSize} bytes");
                            }
                            blocksStream.Write(uncompressedBytes, 0, uncompressedSize);
                            BigArrayPool<byte>.Shared.Return(compressedBytes);
                            BigArrayPool<byte>.Shared.Return(uncompressedBytes);
                            break;
                        }
                    case CompressionType.Zstd when !Game.Type.IsMhyGroup(): //Zstd
                        {
                            var compressedSize = (int)blockInfo.compressedSize;
                            var compressedBytes = BigArrayPool<byte>.Shared.Rent(compressedSize);
                            reader.Read(compressedBytes, 0, compressedSize);

                            var uncompressedSize = (int)blockInfo.uncompressedSize;
                            var uncompressedBytes = BigArrayPool<byte>.Shared.Rent(uncompressedSize);

                            try
                            {
                                using var decompressor = new Decompressor();
                                var numWrite = decompressor.Unwrap(compressedBytes, 0, compressedSize, uncompressedBytes, 0, uncompressedSize);
                                if (numWrite != uncompressedSize)
                                {
                                    throw new IOException($"Zstd decompression error, write {numWrite} bytes but expected {uncompressedSize} bytes");
                                }
                            }
                            catch(Exception ex)
                            {
                                Console.WriteLine($"Zstd decompression error:\n{ex}");
                            }
                            
                            blocksStream.Write(uncompressedBytes.ToArray(), 0, uncompressedSize);
                            BigArrayPool<byte>.Shared.Return(compressedBytes);
                            BigArrayPool<byte>.Shared.Return(uncompressedBytes);
                            break;
                        }
                    default:
                        throw new IOException($"Unsupported compression type {compressionType}");
                }
            }
            blocksStream.Position = 0;
        }

        public int[] ParseVersion()
        {
            var versionSplit = Regex.Replace(m_Header.unityRevision, @"\D", ".").Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            return versionSplit.Select(int.Parse).ToArray();
        }
    }
}
