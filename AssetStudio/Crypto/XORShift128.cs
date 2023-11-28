using System;
using System.Buffers.Binary;

namespace AssetStudio
{
    public static class XORShift128
    {
        private const long SEED = 0x61C8864E7A143579;
        private const uint MT19937 = 0x6C078965;
        private static uint x = 0, y = 0, z = 0, w = 0, initseed = 0;
        
        public static bool Init = false;

        public static void InitSeed(uint seed)
        {
            initseed = seed;
            x = seed;
            y = MT19937 * x + 1;
            z = MT19937 * y + 1;
            w = MT19937 * z + 1;
            Init = true;
        }

        public static uint XORShift()
        {
            uint t = x ^ (x << 11);
            x = y; y = z; z = w;
            return w = w ^ (w >> 19) ^ t ^ (t >> 8);
        }

        public static uint NextUInt32()
        {
            return XORShift();
        }

        public static int NextDecryptInt() => BinaryPrimitives.ReadInt32LittleEndian(NextDecrypt(4));
        public static uint NextDecryptUInt() => BinaryPrimitives.ReadUInt32LittleEndian(NextDecrypt(4));

        public static long NextDecryptLong() => BinaryPrimitives.ReadInt64LittleEndian(NextDecrypt(8));

        public static byte[] NextDecrypt(int size)
        {
            var valueBytes = new byte[size];
            var key = size * initseed - SEED;
            var keyBytes = BitConverter.GetBytes(key);
            for (int i = 0; i < size; i++)
            {
                var val = NextUInt32();
                valueBytes[i] = keyBytes[val % 8];
            }
            return valueBytes;
        }
    }
}
