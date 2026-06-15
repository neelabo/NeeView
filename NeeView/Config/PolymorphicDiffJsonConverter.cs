using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class PolymorphicDiffJsonConverter<T> : JsonConverter<T>
        where T : class
    {
        private static Dictionary<Type, string> _nameMap = new();
        private static Dictionary<string, Type> _typeMap = new();
        private static Dictionary<Type, IDiffJsonConverter> _converters = new();

        static PolymorphicDiffJsonConverter()
        {
        }


        // Diff出力サポート
        public bool IsTrimEnabled { get; set; } = true;


        protected static void Initialize(JsonDerivedTypeData[] knownTypes, Func<JsonDerivedTypeData, string>? toTypeName)
        {
            toTypeName ??= TypeToName;

            CheckAllClassHasEquals(knownTypes);

            _nameMap = knownTypes.ToDictionary(e => e.DerivedType, e => toTypeName(e));
            _typeMap = knownTypes.ToDictionary(e => toTypeName(e), e => e.DerivedType);
            _converters = knownTypes.ToDictionary(e => e.DerivedType, e => (IDiffJsonConverter)Activator.CreateInstance(typeof(DiffJsonConverter<>).MakeGenericType(e.DerivedType))!);
        }

        private static string TypeToName(JsonDerivedTypeData data)
        {
            return data.TypeDiscriminator ?? data.DerivedType.Name;
        }

        // すべてのクラスが Equals を実装しているかチェック（デバッグ用）
        [Conditional("DEBUG")]
        private static void CheckAllClassHasEquals(JsonDerivedTypeData[] knownTypes)
        {
            foreach (var type in knownTypes)
            {
                NVDebug.CheckHasEqualsMethod(type.DerivedType);
            }
        }


        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected StartObject");
            }

            reader.Read();

            if (reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "$type")
            {
                throw new JsonException("Json polymorphism format exception");
            }

            reader.Read();

            var typeString = reader.GetString();

            var result = ReadBody(ref reader, Type, options, typeString);

            if (reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException("Expected EndObject");
            }

            return result;
        }

        protected T? ReadBody(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options, string? typeString)
        {
            if (typeString is null)
            {
                throw new JsonException("Expected String");
            }
            if (!_typeMap.TryGetValue(typeString, out var type))
            {
                throw new JsonException($"Not support polymorphism $type: {typeString}");
            }
            if (!_converters.TryGetValue(type, out var converter))
            {
                throw new JsonException($"Not support polymorphism class: {type.FullName}");
            }

            reader.Read();

            var result = converter.ReadInner(ref reader, type, options);

            return result as T;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var type = value.GetType();
            Debug.Assert(_typeMap.Values.Contains(type));

            if (!_nameMap.TryGetValue(type, out var typeName))
            {
                throw new JsonException($"Not support polymorphism class: {type.FullName}");
            }
            if (!_converters.TryGetValue(type, out var converter))
            {
                throw new JsonException($"Not support polymorphism class: {type.FullName}");
            }

            writer.WriteStartObject();
            writer.WriteString("$type", typeName);
            if (IsTrimEnabled)
            {
                converter.WriteInner(writer, value, options);
            }
            else
            {
                converter.WriteInnerNoDiff(writer, value, options);
            }
            writer.WriteEndObject();
        }
    }


    public class JsonDerivedTypeData
    {
        public JsonDerivedTypeData(Type derivedType, string? typeDiscriminator = null)
        {
            DerivedType = derivedType;
            TypeDiscriminator = typeDiscriminator;
        }

        public Type DerivedType { get; }

        public string? TypeDiscriminator { get; }
    }

}
