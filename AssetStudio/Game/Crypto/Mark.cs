using System;
using System.IO;

namespace AssetStudio
{
    public static class Mark
    {
        private const string Signature = "mark";
        private const int BlockSize = 0xA00;
        private const int ChunkSize = 0x264;
        private const int ChunkPadding = 4;

        private static readonly int BlockPadding = ((BlockSize / ChunkSize) + 1) * ChunkPadding;
        private static readonly int ChunkSizeWithPadding = ChunkSize + ChunkPadding;
        private static readonly int BlockSizeWithPadding = BlockSize + BlockPadding;

        public static byte[] Decrypt(FileReader reader)
        {
            var signature = reader.ReadStringToNull(4);
            if (signature != Signature)
            {
                throw new InvalidOperationException($"Expected signature mark, got {signature} instead !!");
            }

            byte[] buffer;
            var block = new byte[BlockSizeWithPadding];
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                while (reader.Length != reader.Position)
                {
                    var blockSize = (int)Math.Min(reader.Length - reader.Position, BlockSizeWithPadding);
                    var readBytes = reader.Read(block, 0, blockSize);
                    if (readBytes != blockSize)
                    {
                        throw new InvalidOperationException($"Expected {blockSize} but got {readBytes} !!");
                    }

                    var offset = 0;
                    while (offset != blockSize)
                    {
                        var chunkSize = Math.Min(readBytes, ChunkSizeWithPadding);
                        if (!(blockSize == BlockSizeWithPadding || chunkSize == ChunkSizeWithPadding))
                        {
                            writer.Write(block, offset, chunkSize);
                        }
                        else
                        {
                            writer.Write(block, offset, chunkSize - ChunkPadding);
                        }
                        readBytes -= chunkSize;
                        offset += chunkSize;
                    }
                }
                buffer = ms.ToArray();
            }

            for (int j = 0; j < buffer.Length; j++)
            {
                buffer[j] ^= Crypto.MarkKey[j % Crypto.MarkKey.Length];
            }

            return buffer;
        }
    }
}
