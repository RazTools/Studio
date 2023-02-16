using System.IO;

namespace AssetStudio
{
    public class XORStream : BlockStream
    {
        private readonly byte[] _xorpad;

        public XORStream(Stream stream, long pos, byte[] xorpad) : base(stream, pos)
        {
            _xorpad = xorpad;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var pos = RelativePosition;
            var read = base.Read(buffer, offset, count);
            for (int i = 0; i < count; i++)
            {
                buffer[i] ^= _xorpad[pos++ % _xorpad.Length];
            }
            return read;
        }
    }
}
