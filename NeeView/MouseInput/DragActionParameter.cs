using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeeView
{
    [JsonConverter(typeof(JsonDragActionParameterConverter))]
    public class DragActionParameter : BindableBase, ICloneable
    {
        private readonly Func<object, object, bool> _equals;

        public DragActionParameter()
        {
            _equals = ObjectExtensions.MakeEqualsMethod(this.GetType());
        }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        public bool MemberwiseEquals(DragActionParameter? other)
        {
            if (other is null) return false;

            return _equals(this, other);
        }
    }

    public static class DragActionParameterExtensions
    {
        public static T Cast<T>(this DragActionParameter? self) where T : DragActionParameter
        {
            var param = self as T ?? throw new InvalidCastException();
            return param;
        }
    }



    public class SensitiveDragActionParameter : DragActionParameter
    {
        private double _sensitivity = 1.0;

        /// <summary>
        /// 感度
        /// </summary>
        [PropertyRange(0.0, 2.0, TickFrequency = 0.05)]
        public double Sensitivity
        {
            get { return _sensitivity; }
            set { SetProperty(ref _sensitivity, AppMath.Round(value)); }
        }
    }

    public class MoveDragActionParameter : DragActionParameter
    {
        private bool _isInertiaEnabled = true;

        /// <summary>
        /// 慣性
        /// </summary>
        [PropertyMember]
        public bool IsInertiaEnabled
        {
            get { return _isInertiaEnabled; }
            set { SetProperty(ref _isInertiaEnabled, value); }
        }
    }

    public class MoveScaleDragActionParameter : SensitiveDragActionParameter
    {
        private bool _isInertiaEnabled = true;

        /// <summary>
        /// 慣性
        /// </summary>
        [PropertyMember]
        public bool IsInertiaEnabled
        {
            get { return _isInertiaEnabled; }
            set { SetProperty(ref _isInertiaEnabled, value); }
        }
    }



    /// <summary>
    /// JsonConverter for DragActionParameter.
    /// Support polymorphism.
    /// </summary>
    public sealed class JsonDragActionParameterConverter : JsonConverter<DragActionParameter>
    {
        // NOTE: need add polymorphism class type.
        public static Type[] KnownTypes { get; set; } = new Type[]
        {
            typeof(MoveScaleDragActionParameter),
            typeof(MoveDragActionParameter),
            typeof(SensitiveDragActionParameter),
        };


        public override DragActionParameter? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

            Type? type = KnownTypes.FirstOrDefault(e => e.Name == typeString);
            Debug.Assert(type != null);

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
                Debug.WriteLine($"Nor support type: {typeString}");
                reader.Skip();
                instance = null;
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException();
            }

            return (DragActionParameter?)instance;
        }

        public override void Write(Utf8JsonWriter writer, DragActionParameter value, JsonSerializerOptions options)
        {
            var type = value.GetType();
            Debug.Assert(KnownTypes.Contains(type));

            writer.WriteStartObject();
            writer.WriteString("Type", type.Name);
            writer.WritePropertyName("Value");
            JsonSerializer.Serialize(writer, value, type, options);
            writer.WriteEndObject();
        }
    }
}
