using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace NeeView
{
    public record class PlaylistSourceItem
    {
        private string _path = "";
        private string? _name;
        private bool _invalid;

        public PlaylistSourceItem()
        {
        }

        public PlaylistSourceItem(string path)
        {
            _path = path;
        }

        public PlaylistSourceItem(string path, string? name, bool invalid)
        {
            _path = path;
            _name = name;
            _invalid = invalid;
        }


        public string Path 
        {
            get { return _path; }
            set
            {
                if (_path != value)
                {
                    _path = value;
                    _invalid = false;
                }
            }
        }

        [JsonIgnore]
        [NotNull]
        public string? Name
        {
            get { return _name ?? LoosePath.GetFileName(Path); }
            set { _name = (string.IsNullOrEmpty(value) || value.Trim() == LoosePath.GetFileName(Path)) ? null : value.Trim(); }
        }

        [JsonPropertyName(nameof(Name))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [PropertyMapIgnore]
        public string? NameRaw
        {
            get { return _name; }
            set { _name = value; }
        }

        public bool IsNameChanged => _name != null;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool Invalid
        {
            get { return _invalid; }
            set { _invalid = value; }
        }
    }

}
