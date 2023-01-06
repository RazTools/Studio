using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace AssetStudio
{
    public enum MiHoYoBinDataType
    {
        None,
        Bytes,
        JSON
    }
    public sealed partial class MiHoYoBinData : Object
    {
        public static bool Exportable;
        public static bool Encrypted;
        public static byte Key;

        public byte[] RawData;

        public MiHoYoBinData(ObjectReader reader) : base(reader)
        {
            var length = reader.ReadInt32();
            RawData = reader.ReadBytes(length);
        }

        public string AsString => Type switch
        {
            MiHoYoBinDataType.JSON => JToken.Parse(DataStr).ToString(Formatting.Indented),
            MiHoYoBinDataType.Bytes => Chars().Replace(DataStr, string.Empty),
            _ => "",
        };
        public new object Dump() => Type switch
        {
            MiHoYoBinDataType.JSON => AsString,
            MiHoYoBinDataType.Bytes => Data,
            _ => null,
        };
        private string DataStr => Encoding.UTF8.GetString(Data);

        public MiHoYoBinDataType Type
        {
            get
            {
                try
                {
                    var asToken = JToken.Parse(DataStr);
                    if (asToken.Type == JTokenType.Object || asToken.Type == JTokenType.Array)
                        return MiHoYoBinDataType.JSON;
                }
                catch (Exception)
                {
                    return MiHoYoBinDataType.Bytes;
                }
                return MiHoYoBinDataType.None;
            }
        }

        private byte[] Data
        {
            get
            {
                if (Encrypted)
                {
                    byte[] bytes = new byte[RawData.Length];
                    for (int i = 0; i < RawData.Length; i++)
                    {
                        bytes[i] = (byte)(RawData[i] ^ Key);
                    }
                    return bytes;
                }
                else return RawData;
            }
        }

        [GeneratedRegex("[^\\u0020-\\u007E]")]
        private static partial Regex Chars();
    }
}
