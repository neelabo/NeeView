using NeeLaboratory.Linq;
using System;
using System.Collections.Generic;
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
    /// フィールドはなるべく同名プロパティの位置になるような順番で出力する。
    /// シリアライズ専用。以下の属性やオプションには対応していません。
    /// - JsonPropertyOrder
    /// - PropertyNamingPolicy など
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class DiffJsonConverter<T> : JsonConverter<T> where T : new()
    {
        private sealed class PropMeta
        {
            public PropMeta(string jsonName, Type propertyType, Func<T, object?> getter)
            {
                JsonName = jsonName;
                PropertyType = propertyType;
                Getter = getter;
            }

            public string JsonName { get; }
            public Type PropertyType { get; }
            public Func<T, object?> Getter { get; }
        }

        private static readonly PropMeta[] _props = BuildProps();

        private static PropMeta[] BuildProps()
        {
            var type = typeof(T);
            var map = new OrderedDictionary<string, PropMeta?>();

            foreach (var p in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var nameAttr = p.GetCustomAttribute<JsonPropertyNameAttribute>(false);
                var jsonName = nameAttr?.Name ?? p.Name;

                if (p.GetMethod is { IsPublic: true } &&
                    p.SetMethod is { IsPublic: true } &&
                    p.GetIndexParameters().Length == 0 &&
                    p.GetCustomAttribute<JsonIgnoreAttribute>(false)?.Condition is not (JsonIgnoreCondition.Always or JsonIgnoreCondition.WhenWriting))
                {
                    var instanceParam = Expression.Parameter(typeof(T), "target");
                    Expression propertyAccess = Expression.Property(instanceParam, p);
                    Expression convertResult = Expression.Convert(propertyAccess, typeof(object));
                    var lambda = Expression.Lambda<Func<T, object?>>(convertResult, instanceParam);
                    var getter = lambda.Compile();

                    AddToMap(jsonName, new PropMeta(jsonName, p.PropertyType, getter));
                }
                else
                {
                    AddToMap(jsonName, null);
                }
            }

            foreach (var f in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                var nameAttr = f.GetCustomAttribute<JsonPropertyNameAttribute>(false);
                var jsonName = nameAttr?.Name ?? f.Name;

                if (f.IsPublic &&
                    f.GetCustomAttribute<JsonIncludeAttribute>(false) is not null &&
                    f.GetCustomAttribute<JsonIgnoreAttribute>(false)?.Condition is not (JsonIgnoreCondition.Always or JsonIgnoreCondition.WhenWriting))
                {
                    var instanceParam = Expression.Parameter(type, "target");
                    Expression fieldAccess = Expression.Field(instanceParam, f);
                    Expression convertResult = Expression.Convert(fieldAccess, typeof(object));
                    var lambda = Expression.Lambda<Func<T, object?>>(convertResult, instanceParam);
                    var getter = lambda.Compile();

                    AddToMap(jsonName, new PropMeta(jsonName, f.FieldType, getter));
                }
            }

            return map.Values.WhereNotNull().ToArray();

            void AddToMap(string jsonName, PropMeta? propMeta)
            {
                if (map.TryGetValue(jsonName, out var value))
                {
                    if (value is not null)
                    {
                        throw new InvalidOperationException($"Duplicate JSON property name '{jsonName}' in type '{type.FullName}'.");
                    }
                    map[jsonName] = propMeta;
                }
                else
                {
                    map.Add(jsonName, propMeta);
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
