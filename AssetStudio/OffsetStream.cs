using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetStudio
{
    public class OffsetStream : Stream
    {
        private const int BufferSize = 0x10000;

        private readonly Stream _baseStream;
        private long _offset;

        public override bool CanRead => _baseStream.CanRead;
        public override bool CanSeek => _baseStream.CanSeek;
        public override bool CanWrite => false;

        public long Offset
        {
            get => _offset;
            set
            {
                if (value < 0 || value > _baseStream.Length)
                {
                    throw new IOException($"{nameof(Offset)} is out of stream bound");
                }
                _offset = value;
                Seek(0, SeekOrigin.Begin);
            }
        }
        public long AbsolutePosition => _baseStream.Position;
        public long Remaining => Length - Position;

        public override long Length => _baseStream.Length - _offset;
        public override long Position
        {
            get => _baseStream.Position - _offset;
            set => Seek(value, SeekOrigin.Begin);
        }

        public OffsetStream(Stream stream, long offset)
        {
            _baseStream = stream;

            Offset = offset;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (offset > _baseStream.Length)
            {
                throw new IOException("Unable to seek beyond stream bound");
            }

            var target = origin switch
            {
                SeekOrigin.Begin => offset + _offset,
                SeekOrigin.Current => offset + Position,
                SeekOrigin.End => offset + _baseStream.Length,
                _ => throw new NotSupportedException()
            };

            _baseStream.Seek(target, SeekOrigin.Begin);
            return Position;
        }
        public override int Read(byte[] buffer, int offset, int count) => _baseStream.Read(buffer, offset, count);
        public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();
        public override void SetLength(long value) => throw new NotImplementedException();
        public override void Flush() => throw new NotImplementedException();
        public IEnumerable<long> GetOffsets(string path)
        {
            if (AssetsHelper.TryGet(path, out var offsets))
            {
                foreach (var offset in offsets)
                {
                    Offset = offset;
                    yield return offset;
                }
            }
            else
            {
                while (Remaining > 0)
                {
                    Offset = AbsolutePosition;
                    yield return AbsolutePosition;
                    if (Offset == AbsolutePosition)
                    {
                        break;
                    }
                }
            }
        }
    }
}
