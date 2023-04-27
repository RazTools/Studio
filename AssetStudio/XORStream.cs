using System.IO;

namespace AssetStudio
{
    public class XORStream : SubStream
    {
        private readonly byte[] _xorpad;
        private readonly long _offset;

        public XORStream(Stream stream, long offset, byte[] xorpad) : base(stream, offset)
        {
            _xorpad = xorpad;
            _offset = offset;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var pos = AbsolutePosition - _offset;
            var read = base.Read(buffer, offset, count);
            for (int i = 0; i < count; i++)
            {
                buffer[i] ^= _xorpad[pos++ % _xorpad.Length];
            }
            return read;
        }
    }
}
