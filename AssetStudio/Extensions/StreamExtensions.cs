using System.IO;

namespace AssetStudio
{
    public static class StreamExtensions
    {
        private const int BufferSize = 81920;

        public static void CopyTo(this Stream source, Stream destination, long size)
        {
            var buffer = new byte[BufferSize];
            for (var left = size; left > 0; left -= BufferSize)
            {
                int toRead = BufferSize < left ? BufferSize : (int)left;
                int read = source.Read(buffer, 0, toRead);
                destination.Write(buffer, 0, read);
                if (read != toRead)
                {
                    return;
                }
            }
        }

        public static void AlignStream(this Stream stream)
        {
            stream.AlignStream(4);
        }

        public static void AlignStream(this Stream stream, int alignment)
        {
            var pos = stream.Position;
            var mod = pos % alignment;
            if (mod != 0)
            {
                var rem = alignment - mod;
                for (int _ = 0; _ < rem; _++)
                {
                    if (!stream.CanWrite)
                    {
                        throw new IOException("End of stream");
                    }

                    stream.WriteByte(0);
                }
            }
        }
    }
}
