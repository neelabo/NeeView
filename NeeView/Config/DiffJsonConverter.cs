using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeeView
{
    /// <summary>
    /// 初期値と同じ値のプロパティを出力しない JsonConverter
    /// </summary>
    /// <remarks>
    /// シリアライズ専用。以下の属性やオプションには対応していません。
    /// - JsonPropertyOrder
    /// - PropertyNamingPolicy
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class DiffJsonConverter<T> : JsonConverter<T> where T : new()
    {
        private sealed class PropMeta
        {
            public required PropertyInfo Prop { get; init; }
            public required Type PropertyType { get; init; }
            public required string JsonName { get; init; }
            public required Func<T, object?> Getter { get; init; }
        }

        private static readonly PropMeta[] _props = BuildProps();

        private static PropMeta[] BuildProps()
        {
            return typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p =>
                    p.GetMethod is { IsPublic: true } &&
                    p.SetMethod is { IsPublic: true } &&
                    p.GetCustomAttribute<JsonIgnoreAttribute>(false)?.Condition is not (JsonIgnoreCondition.Always or JsonIgnoreCondition.WhenWriting))
                .Select(p =>
                {
                    var nameAttr = p.GetCustomAttribute<JsonPropertyNameAttribute>(false);
                    var jsonName = nameAttr?.Name ?? p.Name;

                    var instanceParam = Expression.Parameter(typeof(T), "target");
                    Expression propertyAccess = Expression.Property(instanceParam, p);
                    Expression convertResult = Expression.Convert(propertyAccess, typeof(object));
                    var lambda = Expression.Lambda<Func<T, object?>>(convertResult, instanceParam);
                    var getter = lambda.Compile();

                    return new PropMeta
                    {
                        Prop = p,
                        PropertyType = p.PropertyType,
                        JsonName = jsonName,
                        Getter = getter,
                    };
                })
                .ToArray();
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException("DiffJsonConverter is write-only.");
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var defaultValue = new T();

            writer.WriteStartObject();

            foreach (var prop in _props)
            {
                var current = prop.Getter(value!);
                var def = prop.Getter(defaultValue);

                if (!Equals(current, def))
                {
                    writer.WritePropertyName(prop.JsonName);
                    JsonSerializer.Serialize(writer, current, prop.PropertyType, options);
                }
            }

            writer.WriteEndObject();
        }
    }
}
