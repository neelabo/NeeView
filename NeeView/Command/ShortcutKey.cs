using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeeView
{
    [JsonConverter(typeof(JsonShortcutKeyConverter))]
    public record ShortcutKey : IEquatable<ShortcutKey>
    {
        public static ShortcutKey Empty { get; } = new ShortcutKey();


        public ShortcutKey()
        {
            Gestures = new();
        }

        public ShortcutKey(string? s)
        {
            Gestures = GetInputGestureSourceCollection(s);
        }

        public ShortcutKey(IEnumerable<InputGestureSource> gestures)
        {
            Gestures = gestures.ToList();
        }


        public List<InputGestureSource> Gestures { get; }

        public bool IsEmpty => Gestures.Count == 0;


        public virtual bool Equals(ShortcutKey? other)
        {
            if (other is null) return false;
            if (EqualityContract != other.EqualityContract) return false;
            if (ReferenceEquals(this, other)) return true;

            return this.Gestures.SequenceEqual(other.Gestures);
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var gesture in Gestures)
            {
                hash.Add(gesture);
            }
            return hash.ToHashCode();
        }

        public bool Contains(InputGestureSource gesture)
        {
            return Gestures.Contains(gesture);
        }

        public override string ToString()
        {
            return string.Join(',', Gestures.Select(e => InputGestureSourceConverter.ConvertToString(e)));
        }

        public string GetDisplayString()
        {
            return string.Join(',', Gestures.Select(e => InputGestureSourceConverter.GetDisplayString(e)));
        }

        private static List<InputGestureSource> GetInputGestureSourceCollection(string? s)
        {
            var list = new List<InputGestureSource>();
            if (!string.IsNullOrWhiteSpace(s))
            {
                foreach (var key in s.Split(','))
                {
                    var inputGesture = InputGestureSourceConverter.ConvertFromString(key);
                    if (inputGesture != null)
                    {
                        list.Add(inputGesture);
                    }
                }
            }

            return list;
        }

        public static ShortcutKey Parse(string? s)
        {
            return new ShortcutKey(GetInputGestureSourceCollection(s));
        }
    }


    public sealed class JsonShortcutKeyConverter : JsonConverter<ShortcutKey>
    {
        public override ShortcutKey? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            if (s is null) return null;
            return ShortcutKey.Parse(s);
        }

        public override void Write(Utf8JsonWriter writer, ShortcutKey value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }


}

