using System;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace AssetStudio
{
    public enum MiHoYoBinDataType
    {
        None,
        Bytes,
        JSON
    }
    public sealed class MiHoYoBinData : Object
    {
        public static bool doXOR;
        public static byte Key;
        public byte[] RawData;

        public byte[] Data
        {
            get
            {
                if (doXOR)
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

        public string Str
        {
            get
            {
                var str = Encoding.UTF8.GetString(Data);
                switch (Type)
                {
                    case MiHoYoBinDataType.JSON:
                        return JToken.Parse(str).ToString(Formatting.Indented);
                    case MiHoYoBinDataType.Bytes:
                        return Regex.Replace(str, @"[^\u0020-\u007E]", string.Empty);
                    default:
                        return "";
                }
            }
        }

        public MiHoYoBinDataType Type
        {
            get
            {
                try
                {
                    var str = Encoding.UTF8.GetString(Data);
                    var asToken = JToken.Parse(str);
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

        public MiHoYoBinData(ObjectReader reader) : base(reader)
        {
            var length = reader.ReadInt32();
            RawData = reader.ReadBytes(length);
        }

        public new dynamic Dump()
        {
            switch (Type)
            {
                case MiHoYoBinDataType.JSON:
                    return Str;
                case MiHoYoBinDataType.Bytes:
                    return Data;
                default:
                    return null;
            }
        }
    }
}
