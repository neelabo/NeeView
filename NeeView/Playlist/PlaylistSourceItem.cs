using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace NeeView
{
    public record class PlaylistSourceItem
    {
        [JsonInclude, JsonPropertyName(nameof(Name))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? _name;

        public PlaylistSourceItem()
        {
        }

        public PlaylistSourceItem(string path)
        {
            Path = path;
        }

        public PlaylistSourceItem(string path, string? name)
        {
            Path = path;
            Name = name;
        }

        public string Path { get; set; } = "";

        [JsonIgnore]
        [NotNull]
        public string? Name
        {
            get { return _name ?? LoosePath.GetFileName(Path); }
            set { _name = (string.IsNullOrEmpty(value) || value.Trim() == LoosePath.GetFileName(Path)) ? null : value.Trim(); }
        }

        public bool IsNameChanged => _name != null;
    }

}
