using System.IO;

namespace AssetStudio
{
    public class CryptoStream : BlockStream
    {
        private const long _dataPosition = 0x2A;

        private readonly byte[] _xorpad;

        public CryptoStream(Stream stream, byte[] xorpad) : base(stream, _dataPosition)
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
