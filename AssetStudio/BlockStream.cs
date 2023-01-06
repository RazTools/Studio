using System;
using System.IO;

namespace AssetStudio
{
    public class BlockStream : Stream
    {
        private readonly Stream _baseStream;
        private readonly long _origin;
        private long _startPosition;

        public override bool CanRead => _baseStream.CanRead;
        public override bool CanSeek => _baseStream.CanSeek;
        public override bool CanWrite => false;
        
        public override long Length => _baseStream.Length - _startPosition;

        public override long Position
        {
            get => _baseStream.Position - _startPosition;
            set => Seek(value, SeekOrigin.Begin);
        }

        public long Offset
        {
            get => _startPosition;
            set => _startPosition = value + _origin;
        }

        public long RelativePosition => _baseStream.Position - _origin;
        public long Remaining => Length - Position;

        public BlockStream(Stream stream, long offset)
        {
            _baseStream = stream;
            _origin = offset;
            _startPosition = offset;
            Seek(0, SeekOrigin.Begin);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            var target = origin switch
            {
                SeekOrigin.Begin => offset + _startPosition,
                SeekOrigin.Current => offset + _baseStream.Position,
                SeekOrigin.End => offset + Length,
                _ => throw new NotSupportedException()
            };

            _baseStream.Seek(target, SeekOrigin.Begin);
            return Position;
        }
        public override int Read(byte[] buffer, int offset, int count) => _baseStream.Read(buffer, offset, count);
        public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();
        public override void SetLength(long value) => throw new NotImplementedException();
        public override void Flush() => throw new NotImplementedException();
    }
}
