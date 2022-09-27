// Common/CRC.cs

using System.Text;

namespace SevenZip
{
	public class CRC
	{
		public static readonly uint[] Table;

		static CRC()
		{
			Table = new uint[256];
			const uint kPoly = 0xEDB88320;
			for (uint i = 0; i < 256; i++)
			{
				uint r = i;
				for (int j = 0; j < 8; j++)
					if ((r & 1) != 0)
						r = (r >> 1) ^ kPoly;
					else
						r >>= 1;
				Table[i] = r;
			}
		}

		uint _value = 0xFFFFFFFF;

		public void Init() { _value = 0xFFFFFFFF; }

		public void UpdateByte(byte b)
		{
			_value = Table[(((byte)(_value)) ^ b)] ^ (_value >> 8);
		}

		public void Update(byte[] data, uint offset, uint size)
		{
			for (uint i = 0; i < size; i++)
				_value = Table[(((byte)(_value)) ^ data[offset + i])] ^ (_value >> 8);
		}

		public uint GetDigest() { return _value ^ 0xFFFFFFFF; }

		static uint CalculateDigest(byte[] data, uint offset, uint size)
		{
			CRC crc = new CRC();
			// crc.Init();
			crc.Update(data, offset, size);
			return crc.GetDigest();
		}

		static bool VerifyDigest(uint digest, byte[] data, uint offset, uint size)
		{
			return (CalculateDigest(data, offset, size) == digest);
		}

        public static uint CalculateDigestAscii(string data)
        {
            var bytes = Encoding.ASCII.GetBytes(data);
            return CalculateDigest(bytes, 0, (uint)bytes.Length);
        }

        public static uint CalculateDigestUTF8(string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            return CalculateDigest(bytes, 0, (uint)bytes.Length);
        }

        public static bool VerifyDigestUTF8(string data, uint digest)
        {
            return CalculateDigestUTF8(data) == digest;
        }

        public static bool Verify28DigestUTF8(string data, uint digest)
        {
            return (CalculateDigestUTF8(data) & 0xFFFFFFF) == digest;
        }
    }
}
