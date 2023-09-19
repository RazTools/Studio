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
    }
}