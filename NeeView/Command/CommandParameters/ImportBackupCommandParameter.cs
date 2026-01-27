using NeeView.Windows.Property;

namespace NeeView
{
    public class ImportBackupCommandParameter : CommandParameter
    {
        private string? _fileName;
        private ImportAction _userSetting = ImportAction.Import;
        private ImportAction _history;
        private ImportAction _bookmark;
        private ImportAction _playlists;
        private ImportAction _themes;
        private ImportAction _scripts;


        [PropertyPath(Filter = "NeeView BackupFile|*.nvzip")]
        public string FileName
        {
            get { return _fileName ?? ""; }
            set { SetProperty(ref _fileName, value); }
        }

        [PropertyMember]
        public ImportAction UserSetting
        {
            get { return _userSetting; }
            set { SetProperty(ref _userSetting, value); }
        }

        [PropertyMember]

        public ImportAction History
        {
            get { return _history; }
            set { SetProperty(ref _history, value); }
        }

        [PropertyMember]
        public ImportAction Bookmark
        {
            get { return _bookmark; }
            set { SetProperty(ref _bookmark, value); }
        }

        [PropertyMember]
        public ImportAction Playlists
        {
            get { return _playlists; }
            set { SetProperty(ref _playlists, value); }
        }

        [PropertyMember]
        public ImportAction Themes
        {
            get { return _themes; }
            set { SetProperty(ref _themes, value); }
        }

        [PropertyMember]
        public ImportAction Scripts
        {
            get { return _scripts; }
            set { SetProperty(ref _scripts, value); }
        }

        public bool IsImportActionValid()
        {
            return UserSetting != ImportAction.Undefined
                && History != ImportAction.Undefined
                && Bookmark != ImportAction.Undefined
                && Playlists != ImportAction.Undefined
                && Themes != ImportAction.Undefined
                && Scripts != ImportAction.Undefined;
        }
    }


    public enum ImportAction
    {
        Undefined,
        Skip,
        Import,
    }
}
