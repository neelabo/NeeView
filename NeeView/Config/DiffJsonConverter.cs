using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace NeeView
{
    /// <summary>
    /// 初期値を出力しないようにする JsonConverter
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DiffJsonConverter<T> : JsonConverter<T> where T : new()
    {
        private static readonly T _defaultValue = new();

        private readonly JsonTypeInfo<T> _typeInfo;

        public DiffJsonConverter(JsonTypeInfo<T> typeInfo)
        {
            Debug.Assert(typeInfo is not null);
            _typeInfo = typeInfo;
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize(ref reader, _typeInfo)!;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            Debug.Assert(value is not null);
            Debug.Assert(_defaultValue is not null);

            if (AppSettings.Current.TrimSaveData)
            {
                writer.WriteStartObject();

                foreach (var prop in _typeInfo.Properties)
                {
                    if (prop.Get is null)
                        continue;

                    var current = prop.Get(value);
                    var def = prop.Get(_defaultValue);

                    if (!Equals(current, def))
                    {
                        writer.WritePropertyName(prop.Name);
                        JsonSerializer.Serialize(writer, current, prop.PropertyType, options);
                    }
                }

                writer.WriteEndObject();
            }
            else
            {
                // default serializer
                JsonSerializer.Serialize(writer, value, _typeInfo);
            }
        }
    }
}
