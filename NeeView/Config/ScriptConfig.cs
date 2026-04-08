using Generator.Equals;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Text.Json.Serialization;

namespace NeeView
{
    [Equatable(Explicit = true, IgnoreInheritedMembers = true)]
    public partial class ScriptConfig : BindableBase
    {
        [DefaultEquality] private bool _isScriptFolderEnabled;
        [DefaultEquality] private ScriptErrorLevel _errorLevel = ScriptErrorLevel.Error;
        [DefaultEquality] private bool _onBookLoadedWhenRenamed = true;
        [DefaultEquality] private bool _isSQLiteEnabled;
        [DefaultEquality] private string? _scriptFolder = null;


        [PropertyMember]
        public bool IsScriptFolderEnabled
        {
            get { return _isScriptFolderEnabled; }
            set { SetProperty(ref _isScriptFolderEnabled, value); }
        }

        [JsonIgnore]
        [PropertyPath(FileDialogType = Windows.Controls.FileDialogType.Directory)]
        public string ScriptFolder
        {
            get { return _scriptFolder ?? SaveDataProfile.DefaultScriptsFolder; }
            set { SetProperty(ref _scriptFolder, (string.IsNullOrEmpty(value) || value.Trim() == SaveDataProfile.DefaultScriptsFolder) ? null : value.Trim()); }
        }

        [JsonPropertyName(nameof(ScriptFolder))]
        [PropertyMapIgnore]
        public string? ScriptFolderRaw
        {
            get { return _scriptFolder; }
            set { _scriptFolder = value; }
        }

        [PropertyMember]
        public ScriptErrorLevel ErrorLevel
        {
            get { return _errorLevel; }
            set { SetProperty(ref _errorLevel, value); }
        }

        [PropertyMember]
        public bool OnBookLoadedWhenRenamed
        {
            get { return _onBookLoadedWhenRenamed; }
            set { SetProperty(ref _onBookLoadedWhenRenamed, value); }
        }

        [PropertyMember]
        public bool IsSQLiteEnabled
        {
            get { return _isSQLiteEnabled; }
            set { SetProperty(ref _isSQLiteEnabled, value); }
        }

    }
}
