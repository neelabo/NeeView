using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeeView
{
    /// <summary>
    /// コマンドパラメータ（基底）
    /// </summary>
    [JsonConverter(typeof(JsonCommandParameterConverter))]
    public abstract class CommandParameter : BindableBase, ICloneable
    {
        private readonly Func<object, object, bool> _equals;

        public CommandParameter()
        {
            _equals = ObjectExtensions.MakeEqualsMethod(this.GetType());
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public T Clone<T>() where T : CommandParameter
        {
            return (T)MemberwiseClone();
        }

        public bool MemberwiseEquals(CommandParameter? other)
        {
            if (other is null) return false;
            return _equals(this, other);
        }
    }

    public static class CommandParameterExtensions
    {
        public static T Cast<T>(this CommandParameter? self) where T : CommandParameter
        {
            var param = self as T ?? throw new InvalidCastException();
            return param;
        }
    }


    /// <summary>
    /// 操作反転コマンドパラメータ基底
    /// </summary>
    public class ReversibleCommandParameter : CommandParameter
    {
        private bool _isReverse = true;

        [PropertyMember]
        public bool IsReverse
        {
            get => _isReverse;
            set => SetProperty(ref _isReverse, value);
        }
    }

    /// <summary>
    /// JsonConverter for CommandParameter.
    /// Support polymorphism.
    /// </summary>
    public sealed class JsonCommandParameterConverter : JsonConverter<CommandParameter>
    {
        // NOTE: need add polymorphism class type.
        public static Type[] KnownTypes { get; set; } = new Type[]
        {
            typeof(ReversibleCommandParameter),
            typeof(MoveSizePageCommandParameter),
            typeof(TogglePageModeCommandParameter),
            typeof(ToggleStretchModeCommandParameter),
            typeof(StretchModeCommandParameter),
            typeof(ViewScrollCommandParameter),
            typeof(ViewScaleCommandParameter),
            typeof(ViewRotateCommandParameter),
            typeof(MovePlaylistItemInBookCommandParameter),
            typeof(ScrollPageCommandParameter),
            typeof(FocusMainViewCommandParameter),
            typeof(ExportImageAsCommandParameter),
            typeof(ExportImageCommandParameter),
            typeof(OpenExternalAppCommandParameter),
            typeof(OpenExternalAppAsCommandParameter),
            typeof(CopyFileCommandParameter),
            typeof(ViewScrollNTypeCommandParameter),
            typeof(ScriptCommandParameter),
            typeof(ImportBackupCommandParameter),
            typeof(ExportBackupCommandParameter),
            typeof(CopyToFolderAsCommandParameter),
            typeof(MoveToFolderAsCommandParameter),
            typeof(CopyBookToFolderAsCommandParameter),
            typeof(MoveBookToFolderAsCommandParameter),
            typeof(ToggleBookmarkCommandParameter),
        };


        public override CommandParameter? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "Type")
            {
                throw new JsonException();
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException();
            }
            var typeString = reader.GetString();

            // Typo ver.41
            if (typeString == "MovePlaylsitItemInBookCommandParameter")
            {
                typeString = "MovePlaylistItemInBookCommandParameter";
            }

            Type? type = KnownTypes.FirstOrDefault(e => e.Name == typeString);
            Debug.Assert(type != null, $"Not support type: {typeString}");

            if (!reader.Read() || reader.GetString() != "Value")
            {
                throw new JsonException();
            }
            if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            object? instance;
            if (type != null)
            {
                instance = JsonSerializer.Deserialize(ref reader, type, options);
            }
            else
            {
                SkipBlock(ref reader);
                instance = null;
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException();
            }

            return instance as CommandParameter;
        }

        private static void SkipBlock(ref Utf8JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.StartObject || reader.TokenType == JsonTokenType.StartArray)
            {
                int depth = reader.CurrentDepth;
                do
                {
                    bool result = reader.Read();
                    if (!result)
                    {
                        throw new JsonException();
                    }
                }
                while (depth < reader.CurrentDepth);
            }
            else
            {
                throw new JsonException($"Unexpected TokenType: {reader.TokenType}");
            }
        }

        public override void Write(Utf8JsonWriter writer, CommandParameter value, JsonSerializerOptions options)
        {

            var type = value.GetType();
            Debug.Assert(KnownTypes.Contains(type));

            var def = Activator.CreateInstance(type) as CommandParameter ?? throw new InvalidOperationException();

            if (value.MemberwiseEquals(def))
            {
                //Debug.WriteLine($"{type} is default.");
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStartObject();
                writer.WriteString("Type", type.Name);
                writer.WritePropertyName("Value");
                JsonSerializer.Serialize(writer, value, type, options);
                writer.WriteEndObject();
            }

        }
    }
}
