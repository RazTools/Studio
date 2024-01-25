using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssetStudio
{
    public class BlbFile
    {
        private const uint DefaultUncompressedSize = 0x20000;

        private List<BundleFile.StorageBlock> m_BlocksInfo;
        private List<BundleFile.Node> m_DirectoryInfo;

        public BundleFile.Header m_Header;
        public List<StreamFile> fileList;
        public long Offset;


        public BlbFile(FileReader reader, string path)
        {
            Offset = reader.Position;
            reader.Endian = EndianType.LittleEndian;

            var signature = reader.ReadStringToNull(4);
            Logger.Verbose($"Parsed signature {signature}");
            if (signature != "Blb\x02")
                throw new Exception("not a Blb file");

            var size = reader.ReadUInt32();
            m_Header = new BundleFile.Header
            {
                version = 6,
                unityVersion = "5.x.x",
                unityRevision = "2017.4.30f1",
                flags = (ArchiveFlags)0x43
            };
            m_Header.compressedBlocksInfoSize = size;
            m_Header.uncompressedBlocksInfoSize = size;

            Logger.Verbose($"Header: {m_Header}");

            var header = reader.ReadBytes((int)m_Header.compressedBlocksInfoSize);
            ReadBlocksInfoAndDirectory(header);
            using var blocksStream = CreateBlocksStream(path);
            ReadBlocks(reader, blocksStream);
            ReadFiles(blocksStream, path);
        }

        private void ReadBlocksInfoAndDirectory(byte[] header)
        {
            using var stream = new MemoryStream(header);
            using var reader = new EndianBinaryReader(stream, EndianType.LittleEndian);

            m_Header.size = reader.ReadUInt32();
            var lastUncompressedSize = reader.ReadUInt32();

            reader.Position += 4;
            var offset = reader.ReadInt64();
            var compressionType = (CompressionType)reader.ReadByte();
            var serializedFileVersion = (SerializedFileFormatVersion)reader.ReadByte();
            reader.AlignStream();

            var blocksInfoCount = reader.ReadInt32();
            var nodesCount = reader.ReadInt32();

            var blocksInfoOffset = reader.Position + reader.ReadInt64();
            var nodesInfoOffset = reader.Position + reader.ReadInt64();
            var bundleInfoOffset = reader.Position + reader.ReadInt64();

            reader.Position = blocksInfoOffset;
            m_BlocksInfo = new List<BundleFile.StorageBlock>();
            Logger.Verbose($"Blocks count: {blocksInfoCount}");
            for (int i = 0; i < blocksInfoCount; i++)
            {
                m_BlocksInfo.Add(new BundleFile.StorageBlock
                {
                    compressedSize = reader.ReadUInt32(),
                    uncompressedSize = i == blocksInfoCount - 1 ? lastUncompressedSize : DefaultUncompressedSize,
                    flags = (StorageBlockFlags)0x43
                });

                Logger.Verbose($"Block {i} Info: {m_BlocksInfo[i]}");
            }

            reader.Position = nodesInfoOffset;
            m_DirectoryInfo = new List<BundleFile.Node>();
            Logger.Verbose($"Directory count: {nodesCount}");
            for (int i = 0; i < nodesCount; i++)
            {
                m_DirectoryInfo.Add(new BundleFile.Node
                {
                    offset = reader.ReadInt32(),
                    size = reader.ReadInt32()
                });

                var pathOffset = reader.Position + reader.ReadInt64();

                var pos = reader.Position;
                reader.Position = pathOffset;
                m_DirectoryInfo[i].path = reader.ReadStringToNull();
                reader.Position = pos;

                Logger.Verbose($"Directory {i} Info: {m_DirectoryInfo[i]}");
            }
        }

        private Stream CreateBlocksStream(string path)
        {
            Stream blocksStream;
            var uncompressedSizeSum = (int)m_BlocksInfo.Sum(x => x.uncompressedSize);
            Logger.Verbose($"Total size of decompressed blocks: 0x{uncompressedSizeSum:X8}");
            if (uncompressedSizeSum >= int.MaxValue)
                blocksStream = new FileStream(path + ".temp", FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
            else
                blocksStream = new MemoryStream(uncompressedSizeSum);
            return blocksStream;
        }

        private void ReadBlocks(EndianBinaryReader reader, Stream blocksStream)
        {
            foreach (var blockInfo in m_BlocksInfo)
            {
                var compressedSize = (int)blockInfo.compressedSize;
                var uncompressedSize = (int)blockInfo.uncompressedSize;

                var compressedBytes = ArrayPool<byte>.Shared.Rent(compressedSize);
                var uncompressedBytes = ArrayPool<byte>.Shared.Rent(uncompressedSize);
                try
                {
                    reader.Read(compressedBytes, 0, compressedSize);

                    var compressedBytesSpan = compressedBytes.AsSpan(0, compressedSize);
                    var uncompressedBytesSpan = uncompressedBytes.AsSpan(0, uncompressedSize);

                    var numWrite = LZ4.Decompress(compressedBytesSpan, uncompressedBytesSpan);
                    if (numWrite != uncompressedSize)
                    {
                        throw new IOException($"Lz4 decompression error, write {numWrite} bytes but expected {uncompressedSize} bytes");
                    }

                    blocksStream.Write(uncompressedBytes, 0, uncompressedSize);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(compressedBytes, true);
                    ArrayPool<byte>.Shared.Return(uncompressedBytes, true);
                }
            }
        }

        private void ReadFiles(Stream blocksStream, string path)
        {
            Logger.Verbose($"Writing files from blocks stream...");

            fileList = new List<StreamFile>();
            for (int i = 0; i < m_DirectoryInfo.Count; i++)
            {
                var node = m_DirectoryInfo[i];
                var file = new StreamFile();
                fileList.Add(file);
                file.path = node.path;
                file.fileName = Path.GetFileName(node.path);
                if (node.size >= int.MaxValue)
                {
                    var extractPath = path + "_unpacked" + Path.DirectorySeparatorChar;
                    Directory.CreateDirectory(extractPath);
                    file.stream = new FileStream(extractPath + file.fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                }
                else
                    file.stream = new MemoryStream((int)node.size);
                blocksStream.Position = node.offset;
                blocksStream.CopyTo(file.stream, node.size);
                file.stream.Position = 0;
            }
        }
    }
}