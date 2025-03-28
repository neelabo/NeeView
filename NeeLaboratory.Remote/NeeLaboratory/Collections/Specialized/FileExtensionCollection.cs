using NeeLaboratory.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeeLaboratory.Collections.Specialized
{
    /// <summary>
    /// ファイル拡張子コレクション (immutable)
    /// </summary>
    [JsonConverter(typeof(FileExtensionCollectionJsonConverter))]
    public class FileExtensionCollection : IEnumerable<string>, IEquatable<FileExtensionCollection>
    {
        public static FileExtensionCollection Empty { get; } = new FileExtensionCollection();

        private readonly List<string> _items;

        public FileExtensionCollection()
        {
            _items = new List<string>();
        }

        public FileExtensionCollection(IEnumerable<string> items)
        {
            _items = ValidateCollection(items);
        }

        public FileExtensionCollection(string items)
        {
            _items = ValidateCollection(items?.Split(';').Select(e => e.Trim()));
        }


        public bool IsEmpty()
        {
            return !_items.Any();
        }

        public bool Contains(string item)
        {
            var s = ValidateItem(item);
            if (s is null) return false;

            return _items.Contains(s);
        }


        public string? ToOneLine()
        {
            return _items.Count > 0 ? string.Join(";", _items) : null;
        }

        private static string? ValidateItem(string item)
        {
            return string.IsNullOrWhiteSpace(item) ? null : "." + item.Trim().TrimStart('.').ToLowerInvariant();
        }

        private static List<string> ValidateCollection(IEnumerable<string>? items)
        {
            if (items == null) return new List<string>();

            return items
                .Select(e => ValidateItem(e))
                .Where(e => !string.IsNullOrEmpty(e))
                .MyWhereNotNull()
                .Distinct()
                .OrderBy(e => e)
                .ToList();
        }

        public IEnumerator<string> GetEnumerator()
        {
            return ((IEnumerable<string>)_items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<string>)_items).GetEnumerator();
        }

        public bool Equals(FileExtensionCollection? other)
        {
            if (other is null) return false;

            return _items.SequenceEqual(other._items);
        }

        public override bool Equals(object? obj)
        {
            if (obj is FileExtensionCollection other)
            {
                return Equals(other);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return _items.GetHashCode();
        }

        public FileExtensionCollection Clone()
        {
            return new FileExtensionCollection(_items);
        }
    }


    public sealed class FileExtensionCollectionJsonConverter : JsonConverter<FileExtensionCollection>
    {
        public override FileExtensionCollection? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            if (s is null) return null;

            return new FileExtensionCollection(s);
        }

        public override void Write(Utf8JsonWriter writer, FileExtensionCollection value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToOneLine());
        }
    }

}
