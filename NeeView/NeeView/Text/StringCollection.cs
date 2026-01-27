using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeeView.Text
{
    /// <summary>
    /// 文字列コレクション
    /// </summary>
    [ObjectMergeReferenceCopy]
    [JsonConverter(typeof(JsonStringCollectionConverter))]
    public class StringCollection : ICloneable, IEquatable<StringCollection>
    {
        public StringCollection()
        {
            Items = new List<string>();
        }

        public StringCollection(string items)
        {
            Restore(items);
        }

        public StringCollection(IEnumerable<string> items)
        {
            Restore(items);
        }


        public bool IsSorted { get; private set; } = true;

        public bool IsDistinction { get; private set; } = true;

        public bool IsNullable { get; private set; } = false;

        // immutable
        public List<string> Items { get; private set; }

        public string OneLine
        {
            get { return Store(); }
            set { Restore(value); }
        }


        public bool IsEmpty()
        {
            return !Items.Any();
        }

        public void Clear()
        {
            Items = new List<string>();
        }

        public bool Contains(string item)
        {
            item = ValidateItem(item);
            return Items.Contains(item);
        }

        public bool ConainsOrdinalIgnoreCase(string item)
        {
            item = ValidateItem(item);
            return Items.Contains(item, StringComparer.OrdinalIgnoreCase);
        }

        public string Add(string item)
        {
            item = ValidateItem(item);
            AddRange(new List<string>() { item });
            return item;
        }

        public void AddRange(IEnumerable<string> items)
        {
            Items = ValidateCollection(Items.Concat(items));
        }

        public void Remove(string item)
        {
            RemoveRange(new List<string>() { item });
        }

        public void RemoveRange(IEnumerable<string> items)
        {
            Items = Items.Except(ValidateCollection(items)).ToList();
        }

        public string Store()
        {
            return StringCollectionParser.Create(Items);
        }

        [MemberNotNull(nameof(Items))]
        public void Restore(IEnumerable<string> items)
        {
            Items = ValidateCollection(items);
        }

        /// <summary>
        /// セミコロン区切りの文字列を分解してコレクションにする
        /// </summary>
        [MemberNotNull(nameof(Items))]
        public void Restore(string items)
        {
            Items = ValidateCollection(StringCollectionParser.Parse(items));
        }

        /// <summary>
        /// セミコロンで連結した１つの文字列を作成する
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return OneLine;
        }

        /// <summary>
        /// 項目のフォーマット
        /// </summary>
        public virtual string ValidateItem(string items)
        {
            return items;
        }

        private List<string> ValidateCollection(IEnumerable<string> items)
        {
            if (items == null) return new List<string>();

            var collection = items;

            if (!IsNullable)
            {
                collection = collection.Where(e => !string.IsNullOrEmpty(e));
            }
            if (IsDistinction)
            {
                collection = collection.Distinct();
            }
            if (IsSorted)
            {
                collection = collection.OrderBy(e => e);
            }

            return collection.ToList();
        }

        public static StringCollection Parse(string s)
        {
            return new StringCollection(s);
        }

        public virtual object Clone()
        {
            var clone = (StringCollection)MemberwiseClone();
            clone.Items = new List<string>(this.Items);
            return clone;
        }

        public bool Equals(StringCollection? other)
        {
            if (other == null) return false;
            return this.Items.SequenceEqual(other.Items);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as StringCollection);
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode(StringComparison.Ordinal);
        }

    }


    public sealed class JsonStringCollectionConverter : JsonConverter<StringCollection>
    {
        public override StringCollection? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            if (s is null) return null;

            return StringCollection.Parse(s);
        }

        public override void Write(Utf8JsonWriter writer, StringCollection value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
