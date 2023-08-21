using System;
using System.Buffers;
using System.Buffers.Binary;

namespace AssetStudio
{
    public static class Mr0kUtils
    {
        private const int BlockSize = 0x400;

        private static readonly byte[] mr0kMagic = { 0x6D, 0x72, 0x30, 0x6B };
        public static Span<byte> Decrypt(Span<byte> data, Mr0k mr0k)
        {
            var key1 = new byte[0x10];
            var key2 = new byte[0x10];
            var key3 = new byte[0x10];

            data.Slice(4, 0x10).CopyTo(key1);
            data.Slice(0x74, 0x10).CopyTo(key2);
            data.Slice(0x84, 0x10).CopyTo(key3);

            var encryptedBlockSize = Math.Min(0x10 * ((data.Length - 0x94) >> 7), BlockSize);

            Logger.Verbose($"Encrypted block size: {encryptedBlockSize}");
            if (!mr0k.InitVector.IsNullOrEmpty())
            {
                for (int i = 0; i < mr0k.InitVector.Length; i++)
                    key2[i] ^= mr0k.InitVector[i];
            }

            if (!mr0k.SBox.IsNullOrEmpty())
            {
                for (int i = 0; i < 0x10; i++)
                    key1[i] = mr0k.SBox[(i % 4 * 0x100) | key1[i]];
            }

            AES.Decrypt(key1, mr0k.ExpansionKey);
            AES.Decrypt(key3, mr0k.ExpansionKey);

            for (int i = 0; i < key1.Length; i++)
            {
                key1[i] ^= key3[i];
            }

            key1.CopyTo(data.Slice(0x84, 0x10));

            var seed1 = BinaryPrimitives.ReadUInt64LittleEndian(key2);
            var seed2 = BinaryPrimitives.ReadUInt64LittleEndian(key3);
            var seed = seed2 ^ seed1 ^ (seed1 + (uint)data.Length - 20);

            Logger.Verbose($"Seed: 0x{seed:X8}");

            var encryptedBlock = data.Slice(0x94, encryptedBlockSize);
            var seedSpan = BitConverter.GetBytes(seed);
            for (var i = 0; i < encryptedBlockSize; i++)
            {
                encryptedBlock[i] ^= (byte)(seedSpan[i % seedSpan.Length] ^ mr0k.BlockKey[i % mr0k.BlockKey.Length]);
            }

            data = data[0x14..];

            if (!mr0k.PostKey.IsNullOrEmpty())
            {
                for (int i = 0; i < 0xC00; i++)
                {
                    data[i] ^= mr0k.PostKey[i % mr0k.PostKey.Length];
                }
            }

            return data;
        }

        public static bool IsMr0k(ReadOnlySpan<byte> data) => data[..4].SequenceEqual(mr0kMagic);
    }
}
