using System.IO;

namespace AssetStudio
{
    public class XORStream : OffsetStream
    {
        private readonly byte[] _xorpad;
        private readonly long _offset;

        private long Index => AbsolutePosition - _offset;

        public XORStream(Stream stream, long offset, byte[] xorpad) : base(stream, offset)
        {
            _xorpad = xorpad;
            _offset = offset;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var pos = Index;
            var read = base.Read(buffer, offset, count);
            if (pos >= 0)
            {
                for (int i = offset; i < count; i++)
                {
                    buffer[i] ^= _xorpad[pos++ % _xorpad.Length];
                }
            }
            return read;
        }
    }
}
