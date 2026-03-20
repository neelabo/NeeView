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

            if (AppSettings.Current.TrimSaveData)
            {
                var defaultValue = new T();

                writer.WriteStartObject();

                foreach (var prop in _typeInfo.Properties)
                {
                    // Setter/Getter がないものは除外
                    // NOTE: JsonIgnre属性を完全に反映したものでないで不完全です
                    if (prop.Get is null || prop.Set is null)
                        continue;

                    var current = prop.Get(value);
                    var def = prop.Get(defaultValue);

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
