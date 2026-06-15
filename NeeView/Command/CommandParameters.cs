using CommunityToolkit.Mvvm.ComponentModel;
using Generator.Equals;
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
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public abstract partial class CommandParameter : ObservableObject, ICloneable
    {
        public object Clone()
        {
            var clone = (CommandParameter)MemberwiseClone();
            clone.OnPropertyChanged();
            return clone;
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
    [Equatable(Explicit = true)]
    public partial class ReversibleCommandParameter : CommandParameter
    {
        [DefaultEquality] private bool _isReverse = true;

        [PropertyMember]
        public bool IsReverse
        {
            get => _isReverse;
            set => SetProperty(ref _isReverse, value);
        }
    }


    /// <summary>
    /// CommandParameter の PolymorphicDiffJsonConverter
    /// </summary>
    public sealed class JsonCommandParameterConverter : PolymorphicDiffJsonConverter<CommandParameter>
    {
        private const string _typeNamePostfix = "CommandParameter";

        private readonly static JsonDerivedTypeData[] _knownTypes =
        [
            new(typeof(ReversibleCommandParameter)),
            new(typeof(ToggleCommandParameter)),
            new(typeof(MoveSizePageCommandParameter)),
            new(typeof(TogglePageModeCommandParameter)),
            new(typeof(ToggleStretchModeCommandParameter)),
            new(typeof(StretchModeCommandParameter)),
            new(typeof(ViewScrollCommandParameter)),
            new(typeof(ViewPresetScrollCommandParameter)),
            new(typeof(ViewScaleCommandParameter)),
            new(typeof(ViewRotateCommandParameter)),
            new(typeof(MovePlaylistItemInBookCommandParameter)),
            new(typeof(ScrollPageCommandParameter)),
            new(typeof(FocusMainViewCommandParameter)),
            new(typeof(ExportImageAsCommandParameter)),
            new(typeof(ExportImageCommandParameter)),
            new(typeof(OpenExternalAppCommandParameter)),
            new(typeof(OpenExternalAppAsCommandParameter)),
            new(typeof(OpenBookExternalAppAsCommandParameter)),
            new(typeof(CopyFileCommandParameter)),
            new(typeof(ViewScrollNTypeCommandParameter)),
            new(typeof(ScriptCommandParameter)),
            new(typeof(ImportBackupCommandParameter)),
            new(typeof(ExportBackupCommandParameter)),
            new(typeof(CopyToFolderAsCommandParameter)),
            new(typeof(MoveToFolderAsCommandParameter)),
            new(typeof(CopyBookToFolderAsCommandParameter)),
            new(typeof(MoveBookToFolderAsCommandParameter)),
            new(typeof(ToggleBookmarkCommandParameter)),
            new(typeof(MoveMediaPositionCommandParameter)),
            new(typeof(SetEffectProfileCommandParameter)),
        ];

        static JsonCommandParameterConverter()
        {
            Initialize(_knownTypes, ToTypeName);
        }

        public JsonCommandParameterConverter()
        {
            IsTrimEnabled = AppSettings.Current.TrimSaveData;
        }

        private static string ToTypeName(JsonDerivedTypeData data)
        {
            return data.TypeDiscriminator ?? ClassTools.CreateName(data.DerivedType.Name, _typeNamePostfix);
        }

        public static bool ContainsKnownType(Type type)
        {
            return _knownTypes.Select(e => e.DerivedType).Contains(type);
        }

        public override CommandParameter? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected StartObject");
            }

            reader.Read();

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Json polymorphism format exception");
            }

            var typePropertyName = reader.GetString();

            reader.Read();

            var typeString = reader.GetString();

            CommandParameter? result;

            if (typePropertyName == "$type")
            {
                result = ReadBody(ref reader, Type, options, typeString);
            }
            else if (typePropertyName == "Type")
            {
                result = ReadLegacy(ref reader, Type, options, typeString);
            }
            else
            {
                throw new JsonException("Json polymorphism format exception");
            }

            if (reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException("Expected EndObject");
            }

            return result;
        }

        private CommandParameter? ReadLegacy(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options, string? typeString)
        {
            // Typo ver.41
            if (typeString == "MovePlaylsitItemInBookCommandParameter")
            {
                typeString = "MovePlaylistItemInBookCommandParameter";
            }

            Type? type = _knownTypes.Select(e => e.DerivedType).FirstOrDefault(e => e.Name == typeString);
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
                Debug.WriteLine($"Not support type: {typeString}");
                reader.Skip();
                instance = null;
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException();
            }

            return instance as CommandParameter;
        }
    }

}
