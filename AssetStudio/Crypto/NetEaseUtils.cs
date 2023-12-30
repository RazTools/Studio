using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace AssetStudio
{
    //Special thanks to LukeFZ#4035.
    public static class NetEaseUtils
    {
        private static readonly byte[] Signature = new byte[] { 0xEE, 0xDD };
        public static void DecryptWithHeader(Span<byte> bytes)
        {
            var (encryptedOffset, encryptedSize) = ReadHeader(bytes);
            var encrypted = bytes.Slice(encryptedOffset, encryptedSize);
            Decrypt(encrypted);
        }
        public static void DecryptWithoutHeader(Span<byte> bytes)
        {
            var encrypted = bytes[..Math.Min(bytes.Length, 0x1000)];
            Decrypt(encrypted);
        }
        private static void Decrypt(Span<byte> bytes)
        {
            Logger.Verbose($"Attempting to decrypt block with NetEase encryption...");

            var encryptedInts = MemoryMarshal.Cast<byte, int>(bytes);

            var seedInts = new int[] { encryptedInts[3], encryptedInts[1], encryptedInts[4], bytes.Length, encryptedInts[2] };
            var seedBytes = MemoryMarshal.AsBytes<int>(seedInts).ToArray();
            var seed = (int)CRC.CalculateDigest(seedBytes, 0, (uint)seedBytes.Length);

            var keyPart0 = seed ^ (encryptedInts[7] + 0x1981);
            var keyPart1 = seed ^ (bytes.Length + 0x2013);
            var keyPart2 = seed ^ (encryptedInts[5] + 0x1985);
            var keyPart3 = seed ^ (encryptedInts[6] + 0x2018);

            for (int i = 0; i < 0x20; i++)
            {
                bytes[i] ^= 0xA6;
            }

            var block = bytes[0x20..];
            var keyVector = new int[] { keyPart2, keyPart0, keyPart1, keyPart3 };
            var keysVector = new int[] { 0x571, keyPart3, 0x892, 0x750, keyPart2, keyPart0, 0x746, keyPart1, 0x568 };
            if (block.Length >= 0x80)
            {
                var dataBlock = block[0x80..];
                var keyBlock = block[..0x80].ToArray();
                var keyBlockInts = MemoryMarshal.Cast<byte, int>(keyBlock);

                RC4(block[..0x80], seed);
                RC4(keyBlock, keyPart1);

                var blockCount = dataBlock.Length / 0x80;
                for (int i = 0; i < blockCount; i++)
                {
                    var blockOffset = i * 0x80;
                    var type = (byte)keysVector[i % keysVector.Length] % keyVector.Length;
                    var dataBlockInts = MemoryMarshal.Cast<byte, int>(dataBlock.Slice(blockOffset, 0x80));
                    for (int j = 0; j < 0x20; j++)
                    {
                        dataBlockInts[j] ^= keyBlockInts[j] ^ type switch
                        {
                            0 => keysVector[j % keysVector.Length] ^ (0x20 - j),
                            1 => keyVector[(byte)keyBlockInts[j] % keyVector.Length],
                            2 => keyVector[(byte)keyBlockInts[j] % keyVector.Length] ^ j,
                            3 => keyVector[(byte)keysVector[j % keysVector.Length] % keyVector.Length] ^ j,
                            _ => throw new NotImplementedException()
                        };
                    }
                }

                var remainingCount = dataBlock.Length % 0x80;
                if (remainingCount > 0)
                {
                    var remaining = bytes[^remainingCount..];
                    for (int i = 0; i < remainingCount; i++)
                    {
                        remaining[i] ^= (byte)(keyBlock[i] ^ ((uint)keysVector[(uint)keyVector[i % keyVector.Length] % keysVector.Length] % 0xFF) ^ i);
                    }
                }
            }
            else
            {
                RC4(block, seed);
            }
        }
        private static (int, int) ReadHeader(Span<byte> bytes)
        {
            var index = bytes.Search(Signature);
            if (index == -1 || index >= 0x40)
            {
                throw new Exception("Header not found !!");
            }

            var info = bytes[index..];
            ReadVersion(info);
            ReadEncryptedSize(info, bytes.Length, out var encryptedSize);

            var headerOffset = 0;
            ReadHeaderOffset(info, 8, ref headerOffset);
            ReadHeaderOffset(info, 9, ref headerOffset);

            var headerSize = 0x30;
            var encryptedOffset = 0x30;
            if (headerOffset == index || headerOffset == 0)
            {
                if (index >= 0x20)
                {
                    headerSize = 0x40;
                    encryptedOffset = 0x40;
                }
            }
            else
            {
                if (headerOffset >= 0x20)
                {
                    headerSize = 0x40;
                    encryptedOffset = 0x40;
                }
                if (headerOffset > index)
                {
                    encryptedOffset += index - headerOffset;
                }
            }

            encryptedSize -= headerSize;

            return (encryptedOffset, encryptedSize);
        }

        private static void ReadVersion(Span<byte> bytes)
        {
            var version = BinaryPrimitives.ReadUInt16LittleEndian(bytes[2..]);
            if (version < 0x2017 || version > 0x2025)
            {
                throw new Exception("Unsupported version");
            }
            var versionString = version.ToString("X4");
            Logger.Verbose($"Bundle version: {versionString}");
            Encoding.UTF8.GetBytes(versionString, bytes);
        }

        private static void ReadEncryptedSize(Span<byte> bytes, int size, out int encryptedSize)
        {
            var (vectorCount, bytesCount) = (bytes[4], bytes[6]);
            encryptedSize = size > 0x1000 ? 0x1000 : size;
            if (vectorCount != 0x2E && bytesCount != 0x2E)
            {
                encryptedSize = bytesCount + 0x10 * vectorCount;
                if (vectorCount == 0xAA && bytesCount == 0xBB)
                {
                    encryptedSize = 0x1000;
                }
                bytes[4] = bytes[6] = 0x2E;
            }
        }

        private static void ReadHeaderOffset(Span<byte> bytes, int index, ref int headerOffset)
        {
            if (bytes[index + 1] == 0x31 && bytes[index] != 0x66)
            {
                headerOffset = bytes[index];
                bytes[index] = 0x66;
            }
        }

        public class CRC
        {
            private static readonly uint[] Table;

            static CRC()
            {
                Table = new uint[256];
                const uint kPoly = 0x9823D6E; 
                for (uint i = 0; i < 256; i++)
                {
                    uint r = i;
                    for (int j = 0; j < 8; j++)
                    {
                        if ((r & 1) != 0)
                            r ^= kPoly;
                        r >>= 1;
                    }
                    Table[i] = r;
                }
            }

            uint _value = 0xFFFFFFFF;

            public void Update(byte[] data, uint offset, uint size)
            {
                for (uint i = 0; i < size; i++)
                    _value = (Table[(byte)_value ^ data[offset + i]] ^ (_value >> 8)) + 0x10;
            }

            public uint GetDigest() { return ~_value - 0x7D29C488; }

            public static uint CalculateDigest(byte[] data, uint offset, uint size)
            {
                var crc = new CRC();
                crc.Update(data, offset, size);
                return crc.GetDigest();
            }
        }

        public static void RC4(Span<byte> data, int key) => RC4(data, BitConverter.GetBytes(key));

        public static void RC4(Span<byte> data, byte[] key)
        {
            int[] S = new int[0x100];
            for (int _ = 0; _ < 0x100; _++)
            {
                S[_] = _;
            }

            int[] T = new int[0x100];

            if (key.Length == 0x100)
            {
                Buffer.BlockCopy(key, 0, T, 0, key.Length);
            }
            else
            {
                for (int _ = 0; _ < 0x100; _++)
                {
                    T[_] = key[_ % key.Length];
                }
            }

            int i = 0;
            int j = 0;
            for (i = 0; i < 0x100; i++)
            {
                j = (j + S[i] + T[i]) % 0x100;

                (S[j], S[i]) = (S[i], S[j]);
            }

            i = j = 0;
            for (int iteration = 0; iteration < data.Length; iteration++)
            {
                i = (i + 1) % 0x100;
                j = (j + S[i]) % 0x100;

                (S[j], S[i]) = (S[i], S[j]);
                var K = (uint)S[(S[j] + S[i]) % 0x100];

                var k = (byte)(K << 6) | (K >> 2);
                data[iteration] ^= (byte)(k + 0x3A);
            }
        }
    }
}
