using System;
using System.IO;
using System.Linq;
using K4os.Compression.LZ4;

namespace AssetStudio
{
    public class BlbFile
    {
        private const int BlockSize = 0x20000;

        private BundleFile.StorageBlock[] m_BlocksInfo;
        private BundleFile.Node[] m_DirectoryInfo;

        public BundleFile.Header m_Header;
        public StreamFile[] fileList;
        public long Offset;

        public BlbFile(FileReader reader, string path)
        {
            Offset = reader.Position;
            reader.Endian = EndianType.LittleEndian;

            var signature = reader.ReadStringToNull(4);
            m_Header = new BundleFile.Header
            {
                version = 6,
                unityVersion = "5.x.x",
                unityRevision = "2017.4.30f1",
            };
            m_Header.uncompressedBlocksInfoSize = reader.ReadUInt32();
            m_Header.compressedBlocksInfoSize = m_Header.uncompressedBlocksInfoSize;

            ReadHeader(reader);
            using var blocksStream = CreateBlocksStream(path);
            ReadBlocks(reader, blocksStream);
            ReadFiles(blocksStream, path);
        }

        private void ReadHeader(FileReader reader)
        {
            var header = reader.ReadBytes((int)m_Header.uncompressedBlocksInfoSize);

            using var ms = new MemoryStream(header);
            using var subReader = new EndianBinaryReader(ms, EndianType.LittleEndian);

            m_Header.size = subReader.ReadUInt32();
            var lastBlockSize = subReader.ReadUInt32();

            subReader.Position += 0x10;

            var blocksCount = subReader.ReadUInt32();
            var directoryInfoCount = subReader.ReadUInt32();

            var blocksInfoOffset = subReader.Position + subReader.ReadInt64();
            var directoryInfoOffset = subReader.Position + subReader.ReadInt64();
            var directoryInfoCountOffset = subReader.Position + subReader.ReadInt64();

            subReader.Position = blocksInfoOffset;

            m_BlocksInfo = new BundleFile.StorageBlock[blocksCount];
            for (int i = 0; i < blocksCount; i++)
            {
                var blocksInfo = new BundleFile.StorageBlock();

                blocksInfo.compressedSize = subReader.ReadUInt32(); 
                blocksInfo.uncompressedSize = i == blocksCount - 1 ? lastBlockSize : BlockSize;

                m_BlocksInfo[i] = blocksInfo;
            }

            subReader.Position = directoryInfoOffset;

            m_DirectoryInfo = new BundleFile.Node[directoryInfoCount];
            for (var i = 0; i < directoryInfoCount; i++)
            {
                m_DirectoryInfo[i] = new BundleFile.Node()
                {
                    offset = subReader.ReadInt32(),
                    size = subReader.ReadInt32(),
                };

                var nameOffset = subReader.Position + subReader.ReadInt64();

                var pos = subReader.Position;

                subReader.Position = nameOffset;
                m_DirectoryInfo[i].path = subReader.ReadStringToNull();

                subReader.Position = pos;
            }
        }

        private Stream CreateBlocksStream(string path)
        {
            Stream blocksStream;
            var uncompressedSizeSum = (int)m_BlocksInfo.Sum(x => x.uncompressedSize);
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

                var compressedBytes = BigArrayPool<byte>.Shared.Rent(compressedSize);
                var uncompressedBytes = BigArrayPool<byte>.Shared.Rent(uncompressedSize);
                reader.Read(compressedBytes, 0, compressedSize);

                var compressedBytesSpan = compressedBytes.AsSpan(0, compressedSize);
                var uncompressedBytesSpan = uncompressedBytes.AsSpan(0, uncompressedSize);

                var numWrite = LZ4Codec.Decode(compressedBytesSpan, uncompressedBytesSpan);
                if (numWrite != uncompressedSize)
                {
                    throw new IOException($"Lz4 decompression error, write {numWrite} bytes but expected {uncompressedSize} bytes");
                }

                blocksStream.Write(uncompressedBytes, 0, uncompressedSize);
                BigArrayPool<byte>.Shared.Return(compressedBytes);
                BigArrayPool<byte>.Shared.Return(uncompressedBytes);
            }
        }

        private void ReadFiles(Stream blocksStream, string path)
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