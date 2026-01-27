using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeeView
{
    [JsonConverter(typeof(JsonTouchGestureConverter))]
    public record TouchGesture : IEquatable<TouchGesture>
    {
        public static TouchGesture Empty { get; } = new TouchGesture();

        private static readonly TouchAreaConverter _touchAreaConverter = new();

        public TouchGesture()
        {
            Areas = new();
        }

        public TouchGesture(string s)
        {
            Areas = ConvertToTouchAreasFromString(s);
        }

        public TouchGesture(IEnumerable<TouchArea> areas)
        {
            Areas = areas.ToList();
        }


        public List<TouchArea> Areas { get; }

        public bool IsEmpty => Areas.Count == 0;


        public virtual bool Equals(TouchGesture? other)
        {
            if (other is null) return false;
            if (EqualityContract != other.EqualityContract) return false;
            if (ReferenceEquals(this, other)) return true;

            return this.Areas.SequenceEqual(other.Areas);
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var area in Areas)
            {
                hash.Add(area);
            }
            return hash.ToHashCode();
        }

        private static List<TouchArea> ConvertToTouchAreasFromString(string s)
        {
            var list = new List<TouchArea>();
            if (!string.IsNullOrWhiteSpace(s))
            {
                foreach (var key in s.Split(','))
                {
                    var areas = _touchAreaConverter.ConvertFromString(key);
                    if (areas is not null)
                    {
                        list.Add((TouchArea)areas);
                    }
                }
            }
            return list;
        }

        public static TouchGesture Parse(string s)
        {
            return new TouchGesture(ConvertToTouchAreasFromString(s));
        }

        public override string ToString()
        {
            return string.Join(',', Areas.Select(e => e.ToString()));
        }

        public string GetDisplayString()
        {
            return string.Join(',', Areas.Select(e => e.GetDisplayString()));
        }
    }


    public sealed class JsonTouchGestureConverter : JsonConverter<TouchGesture>
    {
        public override TouchGesture? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            if (s is null) return null;
            return TouchGesture.Parse(s);
        }

        public override void Write(Utf8JsonWriter writer, TouchGesture value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}

