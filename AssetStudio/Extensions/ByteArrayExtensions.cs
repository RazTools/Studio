using System.Text;

namespace AssetStudio
{
    public static class ByteArrayExtensions
    {
        public static bool IsNullOrEmpty<T>(this T[] array) => array == null || array.Length == 0;
        public static byte[] ToUInt4Array(this byte[] source) => ToUInt4Array(source, 0, source.Length);
        public static byte[] ToUInt4Array(this byte[] source, int offset, int size)
        {
            var buffer = new byte[size * 2];
            for (var i = 0; i < size; i++)
            {
                var idx = i * 2;
                buffer[idx] = (byte)(source[offset + i] >> 4);
                buffer[idx + 1] = (byte)(source[offset + i] & 0xF);
            }
            return buffer;
        }
        public static int Search(this byte[] src, string value, int offset = 0) => Search(src, Encoding.UTF8.GetBytes(value), offset);
        public static int Search(this byte[] src, byte[] pattern, int offset)
        {
            int maxFirstCharSlot = src.Length - pattern.Length + 1;
            for (int i = offset; i < maxFirstCharSlot; i++)
            {
                if (src[i] != pattern[0])
                    continue;

                for (int j = pattern.Length - 1; j >= 1; j--)
                {
                    if (src[i + j] != pattern[j]) break;
                    if (j == 1) return i;
                }
            }
            return -1;
        }
    }
}
