using System;
using System.IO;

namespace AssetStudio
{
    public class SubStream : Stream
    {
        private readonly Stream _baseStream;
        private long _offset;
        private long _size;

        public override bool CanRead => _baseStream.CanRead;
        public override bool CanSeek => _baseStream.CanSeek;
        public override bool CanWrite => false;

        public long Size
        {
            get => _size;
            set
            {
                if (value < 0 || value > _baseStream.Length || value + _offset > _baseStream.Length)
                {
                    throw new IOException($"{nameof(Size)} is out of stream bound");
                }
                _size = value;
            }
        }

        public long Offset
        {
            get => _offset;
            set
            {
                if (value < 0 || value > _baseStream.Length)
                {
                    throw new IOException($"{nameof(Offset)} is out of stream bound");
                }
                if (value + _size > _baseStream.Length)
                {
                    _size = _baseStream.Length - value;
                }
                _offset = value;
            }
        }
        public long AbsolutePosition => _baseStream.Position;
        public long Remaining => Length - Position;

        public override long Length => Size;
        public override long Position
        {
            get => _baseStream.Position - _offset;
            set => Seek(value, SeekOrigin.Begin);
        }

        public SubStream(Stream stream, long offset)
        {
            _baseStream = stream;

            Offset = offset;
            Size = _baseStream.Length - _offset;
            Seek(0, SeekOrigin.Begin);
        }

        public SubStream(Stream stream, long offset, long size)
        {
            _baseStream = stream;

            Offset = offset;
            Size = size;
            Seek(0, SeekOrigin.Begin);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (offset > _size)
            {
                throw new IOException("Unable to seek beyond stream bound");
            }

            var target = origin switch
            {
                SeekOrigin.Begin => offset + _offset,
                SeekOrigin.Current => offset + Position,
                SeekOrigin.End => offset + _size,
                _ => throw new NotSupportedException()
            };

            _baseStream.Seek(target, SeekOrigin.Begin);
            return Position;
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (offset > _size || Position + count > _size)
            {
                throw new IOException("Unable to read beyond stream bound");
            }
            return _baseStream.Read(buffer, offset, count);
        }
        public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();
        public override void SetLength(long value) => throw new NotImplementedException();
        public override void Flush() => throw new NotImplementedException();
    }
}
