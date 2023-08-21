using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;

namespace AssetStudio
{
    public static class BlkUtils
    {
        private const int DataOffset = 0x2A;
        private const int KeySize = 0x1000;
        private const int SeedBlockSize = 0x800;
        private const int BufferSize = 0x10000;

        public static XORStream Decrypt(FileReader reader, Blk blk)
        {
            reader.Endian = EndianType.LittleEndian;

            var signature = reader.ReadStringToNull();
            Logger.Verbose($"Signature: {signature}");
            var count = reader.ReadInt32();
            Logger.Verbose($"Key size: {count}");
            var key = reader.ReadBytes(count);
            reader.Position += count;
            var seedSize = Math.Min(reader.ReadInt16(), blk.SBox.IsNullOrEmpty() ? SeedBlockSize : SeedBlockSize * 2);
            Logger.Verbose($"Seed size: 0x{seedSize:X8}");

            if (!blk.SBox.IsNullOrEmpty() && blk.Type.IsGI())
            {
                for (int i = 0; i < 0x10; i++)
                {
                    key[i] = blk.SBox[(i % 4 * 0x100) | key[i]];
                }
            }

            AES.Decrypt(key, blk.ExpansionKey);

            for (int i = 0; i < 0x10; i++)
            {
                key[i] ^= blk.InitVector[i];
            }

            ulong keySeed = ulong.MaxValue;

            var dataPos = reader.Position;
            for (int i = 0; i < seedSize; i += 8)
            {
                keySeed ^= reader.ReadUInt64();
            }
            reader.Position = dataPos;

            var keyLow = BinaryPrimitives.ReadUInt64LittleEndian(key.AsSpan(0, 8));
            var keyHigh = BinaryPrimitives.ReadUInt64LittleEndian(key.AsSpan(8, 8));
            var seed = keyLow ^ keyHigh ^ keySeed ^ blk.InitSeed;

            Logger.Verbose($"Seed: 0x{seed:X8}");

            var mt64 = new MT19937_64(seed);
            var xorpad = new byte[KeySize];
            for (int i = 0; i < KeySize; i += 8)
            {
                BinaryPrimitives.WriteUInt64LittleEndian(xorpad.AsSpan(i, 8), mt64.Int64());
            }

            return new XORStream(reader.BaseStream, DataOffset, xorpad);
        }

        public static IEnumerable<long> GetOffsets(this XORStream stream, string path)
        {
            if (AssetsHelper.TryGet(path, out var offsets))
            {
                foreach(var offset in offsets)
                {
                    stream.Offset = offset;
                    yield return offset;
                }
            }
            else
            {
                using var reader = new FileReader(path, stream, true);
                var signature = reader.FileType switch
                {
                    FileType.BundleFile => "UnityFS\x00",
                    FileType.Mhy0File => "mhy0",
                    _ => throw new InvalidOperationException()
                };

                Logger.Verbose($"Prased signature: {signature}");

                var signatureBytes = Encoding.UTF8.GetBytes(signature);
                var buffer = BigArrayPool<byte>.Shared.Rent(BufferSize);
                while (stream.Remaining > 0)
                {
                    var index = 0;
                    var absOffset = stream.AbsolutePosition;
                    var read = stream.Read(buffer);
                    while (index < read)
                    {
                        index = buffer.AsSpan(0, read).Search(signatureBytes, index);
                        if (index == -1) break;
                        var offset = absOffset + index;
                        stream.Offset = offset;
                        yield return offset;
                        index++;
                    }
                }
                BigArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}