using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AssetStudio;
public class KVPConverter<T> : JsonConverter
{
    public KVPConverter() : base() { }

    public override bool CanConvert(Type objectType)
    {
        throw new NotImplementedException();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var list = (IList)value;
        writer.WriteStartObject();
        foreach (var val in list)
        {
            if (val is KeyValuePair<string, T> kvp)
            {
                writer.WritePropertyName(kvp.Key.ToString());
                serializer.Serialize(writer, kvp.Value);
            }
        }
        writer.WriteEndObject();
    }
}
