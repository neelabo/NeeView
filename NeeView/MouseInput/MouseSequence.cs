using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeeView
{
    /// <summary>
    /// マウスジェスチャー シーケンス
    /// </summary>
    [JsonConverter(typeof(JsonMouseSequenceConverter))]
    public record MouseSequence : IEquatable<MouseSequence>
    {
        public static MouseSequence Empty { get; } = new();

        private static readonly MouseDirectionConverter _directionConverter = new();


        public MouseSequence()
        {
            Gestures = new();
        }

        public MouseSequence(string s)
        {
            try
            {
                Gestures = ConvertStringToMouseGestureDirectionList(s);
            }
            catch
            {
                Debug.WriteLine($"Invalid mouse gesture: {s}");
                Gestures = new();
            }
        }

        public MouseSequence(IEnumerable<MouseDirection> directions)
        {
            Gestures = directions.ToList();
        }


        public List<MouseDirection> Gestures { get; }

        public bool IsEmpty => Gestures.Count == 0;


        public virtual bool Equals(MouseSequence? other)
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

        private static List<MouseDirection> ConvertStringToMouseGestureDirectionList(string s)
        {
            var list = new List<MouseDirection>();
            if (!string.IsNullOrEmpty(s))
            {
                foreach (char c in s)
                {
                    var result = _directionConverter.ConvertFromString(c.ToString());
                    if (result is not null)
                    {
                        list.Add((MouseDirection)result);
                    }
                }
            }
            return list;
        }

        public static MouseSequence Parse(string s)
        {
            return new MouseSequence(s);
        }

        // 記録用文字列に変換(U,D,L,R,Cの組み合わせ)
        public override string ToString()
        {
            return string.Concat(Gestures.Select(e => _directionConverter.ConvertToString(e)));
        }


        // 表示文字列に変換(矢印の組み合わせ)
        public string GetDisplayString()
        {
            return string.Concat(Gestures.Select(e => e.GetDisplayString()));
        }
    }


    public sealed class JsonMouseSequenceConverter : JsonConverter<MouseSequence>
    {
        public override MouseSequence? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            if (s is null) return null;
            return MouseSequence.Parse(s);
        }

        public override void Write(Utf8JsonWriter writer, MouseSequence value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
