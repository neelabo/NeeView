using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace NeeView
{
    public class Importer : IDisposable
    {
        private readonly System.IO.Compression.ZipArchive _archive;
        private ZipArchiveEntry? _settingEntry;
        private ZipArchiveEntry? _historyEntry;
        private ZipArchiveEntry? _folderConfigEntry;
        private ZipArchiveEntry? _bookmarkEntry;
        private ZipArchiveEntry? _pagemarkEntry;
        private bool _disposedValue;
        private bool _isUserSettingEnabled = true;
        private bool _isHistoryEnabled = false;
        private bool _isBookmarkEnabled = false;
        private bool _isPagemarkEnabled = false;
        private bool _isPlaylistsEnabled = false;
        private bool _isThemesEnabled = false;
        private bool _isScriptsEnabled = false;


        public Importer(ImportBackupCommandParameter parameter)
        {
            this.FileName = parameter.FileName;
            _archive = ZipFile.OpenRead(this.FileName);

            _isUserSettingEnabled = parameter.UserSetting == ImportAction.Import;
            _isHistoryEnabled = parameter.History == ImportAction.Import;
            _isBookmarkEnabled = parameter.Bookmark == ImportAction.Import;
            _isPagemarkEnabled = parameter.Playlists == ImportAction.Import;
            _isPlaylistsEnabled = parameter.Playlists == ImportAction.Import;
            _isThemesEnabled = parameter.Themes == ImportAction.Import;
            _isScriptsEnabled = parameter.Scripts == ImportAction.Import;

            Initialize();
        }


        public string FileName { get; private set; }

        public bool UserSettingExists { get; set; }

        public bool IsUserSettingEnabled
        {
            get => _isUserSettingEnabled && UserSettingExists;
            set => _isUserSettingEnabled = value;
        }

        public bool HistoryExists { get; set; }

        public bool IsHistoryEnabled
        {
            get => _isHistoryEnabled && HistoryExists;
            set => _isHistoryEnabled = value;
        }

        public bool BookmarkExists { get; set; }

        public bool IsBookmarkEnabled
        {
            get => _isBookmarkEnabled && BookmarkExists;
            set => _isBookmarkEnabled = value;
        }

        public bool PagemarkExists { get; set; }

        public bool IsPagemarkEnabled
        {
            get => _isPagemarkEnabled && PagemarkExists;
            set => _isPagemarkEnabled = value;
        }

        public bool PlaylistsExists { get; set; }

        public bool IsPlaylistsEnabled
        {
            get => _isPlaylistsEnabled && PlaylistsExists;
            set => _isPlaylistsEnabled = value;
        }

        public List<ZipArchiveEntry> PlaylistEntries { get; private set; }

        public bool ThemesExists { get; set; }

        public bool IsThemesEnabled
        {
            get => _isThemesEnabled && ThemesExists;
            set => _isThemesEnabled = value;
        }

        public List<ZipArchiveEntry> ThemeEntries { get; private set; }


        public bool ScriptsExists { get; set; }

        public bool IsScriptsEnabled
        {
            get => _isScriptsEnabled && ScriptsExists;
            set => _isScriptsEnabled = value;
        }

        public List<ZipArchiveEntry> ScriptEntries { get; private set; }


        [MemberNotNull(nameof(PlaylistEntries), nameof(ThemeEntries), nameof(ScriptEntries))]
        public void Initialize()
        {
            _settingEntry = _archive.GetEntry(SaveDataProfile.UserSettingFileName);
            _folderConfigEntry = _archive.GetEntry(SaveDataProfile.FolderConfigFileName);
            _historyEntry = _archive.GetEntry(SaveDataProfile.HistoryFileName);
            _bookmarkEntry = _archive.GetEntry(SaveDataProfile.BookmarkFileName);
            _pagemarkEntry = _archive.GetEntry(SaveDataProfile.PagemarkFileName);

            this.PlaylistEntries = _archive.Entries.Where(e => e.FullName.StartsWith(@"Playlists\", StringComparison.Ordinal)).ToList();
            this.ThemeEntries = _archive.Entries.Where(e => e.FullName.StartsWith(@"Themes\", StringComparison.Ordinal)).ToList();
            this.ScriptEntries = _archive.Entries.Where(e => e.FullName.StartsWith(@"Scripts\", StringComparison.Ordinal)).ToList();

            this.UserSettingExists = _settingEntry != null;
            this.HistoryExists = _historyEntry != null;
            this.BookmarkExists = _bookmarkEntry != null;
            this.PagemarkExists = _pagemarkEntry != null;
            this.PlaylistsExists = PlaylistEntries.Any();
            this.ThemesExists = ThemeEntries.Any();
            this.ScriptsExists = ScriptEntries.Any();
        }

        public void Import()
        {
            MainWindowModel.Current.CloseCommandParameterDialog();
            bool recoverySettingWindow = MainWindowModel.Current.CloseSettingWindow();

            ImportUserSetting();
            ImportFolderConfig();
            ImportHistory();
            ImportBookmark();
            ImportPlaylists();
            ImportThemes();
            ImportScripts();

            if (recoverySettingWindow)
            {
                MainWindowModel.Current.OpenSettingWindow();
            }
        }

        public void ImportUserSetting()
        {
            if (!this.IsUserSettingEnabled) return;

            UserSetting? setting = null;

            if (_settingEntry != null)
            {
                using (var stream = _settingEntry.Open())
                {
                    setting = UserSettingTools.Load(stream);
                }
            }

            if (setting != null)
            {
                Setting.SettingWindow.Current?.Cancel();
                MainWindowModel.Current.CloseCommandParameterDialog();

                if (setting.Config is not null)
                {
                    setting.Config.Window.State = Config.Current.Window.State; // ウィンドウ状態は維持する
                }
                UserSettingTools.Restore(setting);
            }
        }

        public void ImportFolderConfig()
        {
            // NOTE: フォルダー設定はユーザー設定の一部とみなす
            if (!this.IsUserSettingEnabled) return;

            FolderConfigCollection.Memento? folderConfig = null;

            if (_folderConfigEntry != null)
            {
                using (var stream = _folderConfigEntry.Open())
                {
                    folderConfig = FolderConfigCollection.Memento.Load(stream);
                }
            }

            if (folderConfig != null)
            {
                FolderConfigCollection.Current.Restore(folderConfig);
                SaveDataSync.Current.SaveFolderConfig(true);
            }
        }

        public void ImportHistory()
        {
            if (!this.IsHistoryEnabled) return;

            BookHistoryCollection.Memento? history = null;

            if (_historyEntry != null)
            {
                using (var stream = _historyEntry.Open())
                {
                    history = BookHistoryCollection.Memento.Load(stream);
                }
            }

            if (history != null)
            {
                BookHistoryCollection.Current.Restore(history, true);
                SaveDataSync.Current.SaveHistory(true);
            }
        }

        public void ImportBookmark()
        {
            if (!this.IsBookmarkEnabled) return;

            BookmarkCollection.Memento? bookmark = null;

            if (_bookmarkEntry != null)
            {
                using (var stream = _bookmarkEntry.Open())
                {
                    bookmark = BookmarkCollection.Memento.Load(stream);
                }
            }

            if (bookmark != null)
            {
                BookmarkCollection.Current.Restore(bookmark);
                SaveDataSync.Current.SaveBookmark(true);
            }
        }

        public void ImportPlaylists()
        {
            if (!IsPlaylistsEnabled) return;

            if (string.IsNullOrEmpty(Config.Current.Playlist.PlaylistFolder))
            {
                return;
            }

            var directory = new DirectoryInfo(Config.Current.Playlist.PlaylistFolder);
            if (!directory.Exists)
            {
                directory.Create();
            }

            foreach (var entry in this.PlaylistEntries)
            {
                var path = Path.Combine(directory.FullName, entry.Name);
                entry.ExtractToFile(path, true);
            }

            PlaylistHub.Current.Refresh();
        }

        public void ImportThemes()
        {
            if (!IsThemesEnabled) return;

            if (string.IsNullOrEmpty(Config.Current.Theme.CustomThemeFolder))
            {
                return;
            }

            var directory = new DirectoryInfo(Config.Current.Theme.CustomThemeFolder);
            if (!directory.Exists)
            {
                directory.Create();
            }

            foreach (var entry in this.ThemeEntries)
            {
                var path = Path.Combine(directory.FullName, entry.Name);
                entry.ExtractToFile(path, true);
            }

            // テーマの再適用
            ThemeManager.Current.RefreshThemeColor();
        }

        public void ImportScripts()
        {
            if (!IsScriptsEnabled) return;

            if (string.IsNullOrEmpty(Config.Current.Script.ScriptFolder))
            {
                return;
            }

            var directory = new DirectoryInfo(Config.Current.Script.ScriptFolder);
            if (!directory.Exists)
            {
                directory.Create();
            }

            foreach (var entry in this.ScriptEntries)
            {
                var path = Path.Combine(directory.FullName, entry.Name);
                entry.ExtractToFile(path, true);
            }

            // スクリプトの再適用
            ScriptManager.Current.UpdateScriptCommands(isForce: true, isReplace: false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _archive.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}
