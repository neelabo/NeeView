using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeeLaboratory.Runtime.Serialization
{
    public static class DefaultSerializer
    {
        public static byte[] Serialize<T>(T data, JsonSerializerContext context)
        {
            return JsonSerializer.SerializeToUtf8Bytes(data, typeof(T), context);
        }

        public static T Deserialize<T>(byte[] source, JsonSerializerContext context)
            where T : class
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            return JsonSerializer.Deserialize(source, typeof(T), context) as T ?? throw new FormatException();
        }
    }


    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(bool))]
    [JsonSerializable(typeof(double))]
    [JsonSerializable(typeof(string))]
    public partial class BasicJsonSerializerContext : JsonSerializerContext
    {
    }
}
