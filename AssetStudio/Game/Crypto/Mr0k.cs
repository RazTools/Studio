using System;
using System.IO;

namespace AssetStudio
{
    public static class Mr0k
    {
		public static byte[] ExpansionKey;
		public static byte[] Key;
		public static byte[] ConstKey;
		public static byte[] SBox;
		public static byte[] BlockKey;
		public static void Decrypt(ref byte[] bytes, ref int size)
        {
			var key1 = new byte[0x10];
			var key2 = new byte[0x10];
			var key3 = new byte[0x10];

			Buffer.BlockCopy(bytes, 4, key1, 0, key1.Length);
			Buffer.BlockCopy(bytes, 0x74, key2, 0, key2.Length);
			Buffer.BlockCopy(bytes, 0x84, key3, 0, key3.Length);

			var encryptedBlockSize = Math.Min(0x10 * ((size - 0x94) >> 7), 0x400);
			var encryptedBlock = new byte[encryptedBlockSize];

			Buffer.BlockCopy(bytes, 0x94, encryptedBlock, 0, encryptedBlockSize);

			if (ConstKey != null)
			{
                for (int i = 0; i < ConstKey.Length; i++)
                    key2[i] ^= ConstKey[i];
            }

			if (SBox != null)
			{
                for (int i = 0; i < 0x10; i++)
                    key1[i] = SBox[(i % 4 * 0x100) | key1[i]];
            }

			AES.Decrypt(key1, ExpansionKey);
			AES.Decrypt(key3, ExpansionKey);

			for (int i = 0; i < key1.Length; i++)
            {
				key1[i] ^= key3[i];
			}

			Buffer.BlockCopy(key1, 0, bytes, 0x84, key1.Length);

			var seed1 = BitConverter.ToUInt64(key2, 0);
			var seed2 = BitConverter.ToUInt64(key3, 0);
			var seed = seed2 ^ seed1 ^ (seed1 + (uint)size - 20);
			var seedBytes = BitConverter.GetBytes(seed);

			for (var i = 0; i < encryptedBlockSize; i++)
            {
				encryptedBlock[i] ^= (byte)(seedBytes[i % 8] ^ Key[i]);
			}

			Buffer.BlockCopy(encryptedBlock, 0, bytes, 0x94, encryptedBlockSize);

			size -= 0x14;
			bytes = bytes.AsSpan(0x14, size).ToArray();

			if (BlockKey != null)
			{
                for (int i = 0; i < 0xC00; i++)
                {
                    bytes[i] ^= BlockKey[i % BlockKey.Length];
                }

            }
        }

		public static bool IsMr0k(byte[] bytes)
		{
			using (var ms = new MemoryStream(bytes))
			using (var reader = new EndianBinaryReader(ms))
			{
				var header = reader.ReadStringToNull(4);
				return header == "mr0k";
			}
		}
    }
}
