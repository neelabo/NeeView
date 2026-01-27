using NeeView.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeeView
{
    /// <summary>
    /// ファイル拡張子コレクション
    /// </summary>
    [ObjectMergeReferenceCopy]
    [JsonConverter(typeof(JsonFileTypeCollectionConverter))]
    public class FileTypeCollection : StringCollection, IEquatable<FileTypeCollection>
    {
        public FileTypeCollection()
        {
        }

        public FileTypeCollection(string exts) : base(exts)
        {
        }

        public FileTypeCollection(IEnumerable<string> exts) : base(exts)
        {
        }

        public override string ValidateItem(string item)
        {
            return string.IsNullOrWhiteSpace(item) ? "" : "." + ReplaceInvalidFileNameChars(item).Trim().TrimStart('.').ToLowerInvariant();
        }

        private static string ReplaceInvalidFileNameChars(string s)
        {
            if (s is null) return "";

            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            return string.Concat(s.Select(c => invalidChars.Contains(c) ? '_' : c));
        }

        public new static FileTypeCollection Parse(string s)
        {
            return new FileTypeCollection(s);
        }

        public bool Equals(FileTypeCollection? other)
        {
            if (other == null) return false;
            return this.ToString() == other.ToString();
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as FileTypeCollection);
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode(StringComparison.Ordinal);
        }
    }

    public sealed class JsonFileTypeCollectionConverter : JsonConverter<FileTypeCollection>
    {
        public override FileTypeCollection? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            if (s is null) return null;

            return FileTypeCollection.Parse(s);
        }

        public override void Write(Utf8JsonWriter writer, FileTypeCollection value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
