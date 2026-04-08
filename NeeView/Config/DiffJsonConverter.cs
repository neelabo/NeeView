using NeeLaboratory.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// シリアライズ専用。読み書き可能なプロパティのみ対応。フィールドはサポートしません。
    /// 以下の属性やオプションには対応していません。
    /// - JsonPropertyOrder
    /// - PropertyNamingPolicy
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class DiffJsonConverter<T> : JsonConverter<T> where T : new()
    {
        private sealed record PropMeta(string JsonName, Type PropertyType, Func<T, object?> Getter);

        private static readonly PropMeta[] _props = BuildProps();

        private static PropMeta[] BuildProps()
        {
            TestFieldMembers();

            return typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.GetIndexParameters().Length == 0 &&
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

                    return new PropMeta(jsonName, p.PropertyType, getter);
                })
                .ToArray();
        }

        [Conditional("DEBUG")]
        private static void TestFieldMembers()
        {
            // Field members are not supported.
            foreach (var f in typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                var nameAttr = f.GetCustomAttribute<JsonPropertyNameAttribute>(false);
                var jsonName = nameAttr?.Name ?? f.Name;

                if (f.IsPublic &&
                    f.GetCustomAttribute<JsonIncludeAttribute>(false) is not null &&
                    f.GetCustomAttribute<JsonIgnoreAttribute>(false)?.Condition is not (JsonIgnoreCondition.Always or JsonIgnoreCondition.WhenWriting))
                {
                    throw new InvalidOperationException("Field members are not supported.");
                }
            }
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
