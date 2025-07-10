using NeeView.Properties;
using System.Linq;

namespace NeeView
{
    public class ImportControlViewModel
    {
        private readonly Importer _model;

        public ImportControlViewModel(Importer model)
        {
            _model = model;
        }

        public string Title => $"{TextResources.GetString("Word.Import")}: {System.IO.Path.GetFileName(_model.FileName)}";

        public bool UserSettingExists => _model.UserSettingExists;

        public bool IsUserSettingEnabled
        {
            get => _model.IsUserSettingEnabled;
            set => _model.IsUserSettingEnabled = value;
        }

        public bool HistoryExists => _model.HistoryExists;

        public bool IsHistoryEnabled
        {
            get => _model.IsHistoryEnabled;
            set => _model.IsHistoryEnabled = value;
        }

        public bool BookmarkExists => _model.BookmarkExists;

        public bool IsBookmarkEnabled
        {
            get => _model.IsBookmarkEnabled;
            set => _model.IsBookmarkEnabled = value;
        }

        public bool PagemarkExists => _model.PagemarkExists;

        public bool IsPagemarkEnabled
        {
            get => _model.IsPagemarkEnabled;
            set => _model.IsPagemarkEnabled = value;
        }

        public bool PlaylistsExists => _model.PlaylistsExists;

        public bool IsPlaylistsEnabled
        {
            get => _model.IsPlaylistsEnabled;
            set => _model.IsPlaylistsEnabled = value;
        }

        public string PlaylistsCheckBoxContent => TextResources.GetFormatString("ImportControl.Playlist", _model.PlaylistEntries.Count);

        public bool ThemesExists => _model.ThemesExists;

        public bool IsThemesEnabled
        {
            get => _model.IsThemesEnabled;
            set => _model.IsThemesEnabled = value;
        }

        public string ThemesCheckBoxContent => TextResources.GetFormatString("ImportControl.Theme", _model.ThemeEntries.Count);

        public bool ScriptsExists => _model.ScriptsExists;

        public bool IsScriptsEnabled
        {
            get => _model.IsScriptsEnabled;
            set => _model.IsScriptsEnabled = value;
        }

        public string ScriptsCheckBoxContent => TextResources.GetFormatString("ImportControl.Script", _model.ScriptEntries.Count);
    }
}
