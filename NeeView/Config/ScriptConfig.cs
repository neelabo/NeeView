using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class ScriptConfig : BindableBase
    {
        private bool _isScriptFolderEnabled;
        private ScriptErrorLevel _errorLevel = ScriptErrorLevel.Error;
        private bool _onBookLoadedWhenRenamed = true;
        private bool _isSQLiteEnabled;


        [JsonInclude]
        [JsonPropertyName(nameof(ScriptFolder))]
        public string? _scriptFolder = null;


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
