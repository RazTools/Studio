using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace AssetStudio
{
    public static class BinaryReaderExtensions
    {
        public static void AlignStream(this BinaryReader reader)
        {
            reader.AlignStream(4);
        }

        public static void AlignStream(this BinaryReader reader, int alignment)
        {
            var pos = reader.BaseStream.Position;
            var mod = pos % alignment;
            if (mod != 0)
            {
                reader.BaseStream.Position += alignment - mod;
            }
        }

        public static string ReadAlignedString(this BinaryReader reader)
        {
            var length = reader.ReadInt32();
            if (length > 0 && length <= reader.BaseStream.Length - reader.BaseStream.Position)
            {
                var stringData = reader.ReadBytes(length);
                var result = Encoding.UTF8.GetString(stringData);
                reader.AlignStream(4);
                return result;
            }
            return "";
        }

        public static string ReadStringToNull(this BinaryReader reader, int maxLength = 32767)
        {
            var bytes = new List<byte>();
            int count = 0;
            while (reader.BaseStream.Position != reader.BaseStream.Length && count < maxLength)
            {
                var b = reader.ReadByte();
                if (b == 0)
                {
                    break;
                }
                bytes.Add(b);
                count++;
            }
            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        public static Quaternion ReadQuaternion(this BinaryReader reader)
        {
            return new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector4 ReadVector4(this BinaryReader reader)
        {
            return new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static Color ReadColor4(this BinaryReader reader)
        {
            return new Color(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static Matrix4x4 ReadMatrix(this BinaryReader reader)
        {
            return new Matrix4x4(reader.ReadSingleArray(16));
        }

        public static int ReadMhy0Int1(this BinaryReader reader)
        {
            var buffer = reader.ReadBytes(7);
            return buffer[1] | (buffer[6] << 8) | (buffer[3] << 0x10) | (buffer[2] << 0x18);
        }

        public static int ReadMhy0Int2(this BinaryReader reader)
        {
            var buffer = reader.ReadBytes(6);
            return buffer[2] | (buffer[4] << 8) | (buffer[0] << 0x10) | (buffer[5] << 0x18);
        }

        public static string ReadMhy0String(this BinaryReader reader)
        {
            var bytes = reader.ReadBytes(0x100);
            return Encoding.UTF8.GetString(bytes.TakeWhile(b => !b.Equals(0)).ToArray());
        }

        public static bool ReadMhy0Bool(this BinaryReader reader)
        {
            var value = reader.ReadMhy0Int2();
            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return BitConverter.ToBoolean(bytes, 0);
        }

        private static T[] ReadArray<T>(Func<T> del, int length)
        {
            var array = new T[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = del();
            }
            return array;
        }

        public static bool[] ReadBooleanArray(this BinaryReader reader)
        {
            return ReadArray(reader.ReadBoolean, reader.ReadInt32());
        }

        public static byte[] ReadUInt8Array(this BinaryReader reader)
        {
            return reader.ReadBytes(reader.ReadInt32());
        }

        public static ushort[] ReadUInt16Array(this BinaryReader reader)
        {
            return ReadArray(reader.ReadUInt16, reader.ReadInt32());
        }

        public static int[] ReadInt32Array(this BinaryReader reader)
        {
            return ReadArray(reader.ReadInt32, reader.ReadInt32());
        }

        public static int[] ReadInt32Array(this BinaryReader reader, int length)
        {
            return ReadArray(reader.ReadInt32, length);
        }

        public static uint[] ReadUInt32Array(this BinaryReader reader)
        {
            return ReadArray(reader.ReadUInt32, reader.ReadInt32());
        }

        public static uint[][] ReadUInt32ArrayArray(this BinaryReader reader)
        {
            return ReadArray(reader.ReadUInt32Array, reader.ReadInt32());
        }

        public static uint[] ReadUInt32Array(this BinaryReader reader, int length)
        {
            return ReadArray(reader.ReadUInt32, length);
        }

        public static float[] ReadSingleArray(this BinaryReader reader)
        {
            return ReadArray(reader.ReadSingle, reader.ReadInt32());
        }

        public static float[] ReadSingleArray(this BinaryReader reader, int length)
        {
            return ReadArray(reader.ReadSingle, length);
        }

        public static string[] ReadStringArray(this BinaryReader reader)
        {
            return ReadArray(reader.ReadAlignedString, reader.ReadInt32());
        }

        public static Vector2[] ReadVector2Array(this BinaryReader reader)
        {
            return ReadArray(reader.ReadVector2, reader.ReadInt32());
        }

        public static Vector4[] ReadVector4Array(this BinaryReader reader)
        {
            return ReadArray(reader.ReadVector4, reader.ReadInt32());
        }

        public static Matrix4x4[] ReadMatrixArray(this BinaryReader reader)
        {
            return ReadArray(reader.ReadMatrix, reader.ReadInt32());
        }
    }
}
