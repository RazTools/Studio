using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetStudio
{
    public class OffsetStream : Stream
    {
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
                using var reader = new FileReader(path, this, true);

                var readSignature = reader.FileType;
                var signature = "";

                if (readSignature == FileType.BundleFile ||
                    readSignature == FileType.MhyFile ||
                    readSignature == FileType.Blb3File)
                {
                    switch (reader.FileType)
                    {
                        case FileType.BundleFile:
                            signature = "UnityFS\x00";
                            break;
                        case FileType.MhyFile:
                            signature = "mhy";
                            break;
                        case FileType.Blb3File:
                            signature = "Blb\x03";
                            break;
                    }

                    Logger.Verbose($"Parsed signature: {signature}");

                    var signatureBytes = Encoding.UTF8.GetBytes(signature);
                    var buffer = ArrayPool<byte>.Shared.Rent((int)reader.Length);
                    while (Remaining > 0)
                    {
                        var index = 0;
                        var absOffset = AbsolutePosition;
                        var read = Read(buffer);
                        while (index < read)
                        {
                            index = buffer.AsSpan(0, read).Search(signatureBytes, index);
                            if (index == -1) break;
                            var offset = absOffset + index;
                            Offset = offset;
                            yield return offset;
                            index++;
                        }
                    }
                    ArrayPool<byte>.Shared.Return(buffer);
                } else
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
}
