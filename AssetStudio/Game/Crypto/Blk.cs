using System;

namespace AssetStudio
{
    public static class Blk
    {
        private const int KeySize = 0x1000;
        private const int SeedBlockSize = 0x800;

        public static byte[] ExpansionKey;
        public static byte[] SBox;
        public static byte[] ConstKey;
        public static ulong ConstVal;

        public static byte[] Decrypt(FileReader reader)
        {
            reader.Endian = EndianType.LittleEndian;

            var signature = reader.ReadStringToNull();
            if (signature != "blk")
                throw new Exception("not a blk");

            var count = reader.ReadInt32();
            var key = reader.ReadBytes(count);
            reader.Position += count;
            var seedSize = Math.Min(reader.ReadInt16(), SBox == null ? SeedBlockSize : SeedBlockSize * 2);

            var dataSize = (int)(reader.BaseStream.Length - reader.BaseStream.Position);
            var buffer = reader.ReadBytes(dataSize);

            long keySeed = -1;
            for (int i = 0; i < seedSize / 8; i++)
            {
                keySeed ^= BitConverter.ToInt64(buffer, i * 8);
            }

            DecryptKey(key);

            var keyLow = BitConverter.ToUInt64(key, 0);
            var keyHigh = BitConverter.ToUInt64(key, 8);
            var seed = keyLow ^ keyHigh ^ (ulong)keySeed ^ ConstVal;

            var xorpad = new byte[KeySize];
            var rand = new MT19937_64(seed);
            for (int i = 0; i < KeySize / 8; i++)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(rand.Int63()), 0, xorpad, i * 8, 8);
            }

            for (int i = 0; i < dataSize; i++)
            {
                buffer[i] ^= xorpad[i % KeySize];
            }

            return buffer;
        }

        private static void DecryptKey(byte[] key)
        {
            if (SBox != null)
            {
                for (int i = 0; i < 0x10; i++)
                {
                    key[i] = SBox[(i % 4 * 0x100) | key[i]];
                }
            }

            AES.Decrypt(key, ExpansionKey);

            for (int i = 0; i < 0x10; i++)
            {
                key[i] ^= ConstKey[i];
            }
        }
    }
}
