using K4os.Compression.LZ4;
using System;
using System.IO;
using System.Linq;

namespace AssetStudio
{
    public partial class Mhy0File
    {
        public class Header
        {
            public int Size;
            public int BundleCount;
            public int BlockCount;
        }

        public class StorageBlock
        {
            public int CompressedSize;
            public int UncompressedSize;
        }

        public class Node
        {
            public long Offset;
            public long Size;
            public string Path;
            public bool IsAssetFile;
        }

        private Header m_Header;
        private StorageBlock[] m_BlocksInfo;
        private Node[] m_DirectoryInfo;

        public StreamFile[] FileList;

        private byte[] DecompressHeader(byte[] header)
        {
            using (var ms = new MemoryStream(header))
            using (var reader = new EndianBinaryReader(ms))
            {
                reader.Position += 0x20;
                var decompressedSize = reader.ReadMhy0Int1();
                var decompressed = new byte[decompressedSize];

                var compressed = reader.ReadBytes((int)(reader.BaseStream.Length - reader.Position));

                var numWrite = LZ4Codec.Decode(compressed, decompressed);
                if (numWrite != decompressedSize)
                    throw new IOException($"Lz4 decompression error, write {numWrite} bytes but expected {decompressedSize} bytes");

                return decompressed;
            }
        }

        private void ReadBlocksInfoAndDirectory(byte[] header)
        {
            using (var ms = new MemoryStream(header))
            using (var reader = new EndianBinaryReader(ms))
            {
                m_Header.BundleCount = reader.ReadMhy0Int2();
                m_DirectoryInfo = new Node[m_Header.BundleCount];
                for (int i = 0; i < m_Header.BundleCount; i++)
                {
                    m_DirectoryInfo[i] = new Node
                    {
                        Path = reader.ReadMhy0String(),
                        IsAssetFile = reader.ReadMhy0Bool(),
                        Offset = reader.ReadMhy0Int2(),
                        Size = reader.ReadMhy0Int1(),

                    };
                }

                m_Header.BlockCount = reader.ReadMhy0Int2();
                m_BlocksInfo = new StorageBlock[m_Header.BlockCount];
                for (int i = 0; i < m_Header.BlockCount; i++)
                {
                    m_BlocksInfo[i] = new StorageBlock
                    {
                        CompressedSize = reader.ReadMhy0Int2(),
                        UncompressedSize = reader.ReadMhy0Int1()
                    };
                }
            }
        }

        private Stream CreateBlocksStream(string path)
        {
            Stream blocksStream;
            var uncompressedSizeSum = m_BlocksInfo.Sum(x => x.UncompressedSize);
            if (uncompressedSizeSum >= int.MaxValue)
                blocksStream = new FileStream(path + ".temp", FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
            else
                blocksStream = new MemoryStream(uncompressedSizeSum);
            return blocksStream;
        }

        private void ReadBlocks(EndianBinaryReader reader, Stream blocksStream)
        {
            var compressedBytes = BigArrayPool<byte>.Shared.Rent(m_BlocksInfo.Max(x => x.CompressedSize));
            var uncompressedBytes = BigArrayPool<byte>.Shared.Rent(m_BlocksInfo.Max(x => x.UncompressedSize));
            foreach (var blockInfo in m_BlocksInfo)
            {
                var compressedSize = blockInfo.CompressedSize;
                reader.Read(compressedBytes, 0, compressedSize);
                if (compressedSize < 0x10)
                    throw new Exception($"Wrong compressed length: {compressedSize}");
                compressedBytes = Crypto.DescrambleEntry(compressedBytes);
                var uncompressedSize = blockInfo.UncompressedSize;
                var numWrite = LZ4Codec.Decode(compressedBytes.AsSpan(0xC, compressedSize - 0xC), uncompressedBytes.AsSpan(0, uncompressedSize));
                if (numWrite != uncompressedSize)
                    throw new IOException($"Lz4 decompression error, write {numWrite} bytes but expected {uncompressedSize} bytes");
                blocksStream.Write(uncompressedBytes, 0, uncompressedSize);
            }
            BigArrayPool<byte>.Shared.Return(compressedBytes);
            BigArrayPool<byte>.Shared.Return(uncompressedBytes);
        }

        private void ReadFiles(Stream blocksStream, string path)
        {
            FileList = new StreamFile[m_DirectoryInfo.Length];
            for (int i = 0; i < m_DirectoryInfo.Length; i++)
            {
                var node = m_DirectoryInfo[i];
                var file = new StreamFile();
                FileList[i] = file;
                file.path = node.Path;
                file.fileName = Path.GetFileName(node.Path);
                if (node.Size >= int.MaxValue)
                {
                    var extractPath = path + "_unpacked" + Path.DirectorySeparatorChar;
                    Directory.CreateDirectory(extractPath);
                    file.stream = new FileStream(extractPath + file.fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                }
                else
                    file.stream = new MemoryStream((int)node.Size);
                blocksStream.Position = node.Offset;
                blocksStream.CopyTo(file.stream, node.Size);
                file.stream.Position = 0;
            }
        }

        public Mhy0File(EndianBinaryReader reader, string path)
        {
            var magic = reader.ReadStringToNull(4);
            if (magic != "mhy0")
                throw new Exception("not a mhy0");

            m_Header = new Header();
            m_Header.Size = reader.ReadInt32();
            var header = reader.ReadBytes(m_Header.Size);

            header = Crypto.DescrambleHeader(header);
            header = DecompressHeader(header);
            ReadBlocksInfoAndDirectory(header);
            using (var blocksStream = CreateBlocksStream(path))
            {
                ReadBlocks(reader, blocksStream);
                ReadFiles(blocksStream, path);
            }
        }
    }
}