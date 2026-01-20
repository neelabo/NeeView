using System;
using System.Diagnostics;
using System.IO;


namespace NeeView
{
    public class SaveData
    {
        public const string BackupExtension = ".bak";

        static SaveData() => Current = new SaveData();
        public static SaveData Current { get; }

        private string? _pagemarkFilenameToDelete;
        private bool _historyMergeFlag;
        private DateTime _historyLastWriteTime;
        private FileStamp _userSettingFileStamp = new();
        private FileStamp _bookmarkFileStamp = new();
        private FileStamp _folderConfigFileStamp = new();

        // 設定のバックアップを１起動に付き１回に制限するフラグ
        private bool _backupOnce = true;


        private SaveData()
        {
            App.Current.CriticalError += (s, e) => DisableSave();
        }


        public static string UserSettingFilePath => App.Current.Option.SettingFilename ?? throw new InvalidOperationException("UserSettingFilePath must not be null");
        public static string HistoryFilePath => Config.Current.History.HistoryFilePath;
        public static string BookmarkFilePath => Config.Current.Bookmark.BookmarkFilePath;
        public static string FolderConfigFilePath => Config.Current.Bookshelf.FolderConfigFilePath;


        public bool IsEnableSave { get; private set; } = true;

        public bool BackupOnce
        {
            get => _backupOnce;
            set => _backupOnce = value;
        }

        public void DisableSave()
        {
            IsEnableSave = false;
        }

        public void SetUserSettingFileStamp(FileStamp? fileStamp)
        {
            if (fileStamp is null) return;
            _userSettingFileStamp = fileStamp;
        }

        public bool IsLatestUserSettingFileStamp()
        {
            return _userSettingFileStamp.IsLatest();
        }

        #region Load

        /// <summary>
        /// 設定の読み込み
        /// </summary>
        /// <returns>UserSetting</returns>
        public UserSetting LoadUserSetting(bool cancellable)
        {
            if (App.Current.IsMainWindowLoaded)
            {
                Setting.SettingWindow.Current?.Cancel();
                MainWindowModel.Current.CloseCommandParameterDialog();
            }

            UserSetting? setting;

            using (ProcessLock.Lock())
            {
                var filename = App.Current.Option.SettingFilename;
                if (File.Exists(filename))
                {
                    var fileStamp = FileStamp.Create(filename);
                    var failedDialog = new UserSettingLoadFailedDialog(cancellable);
                    setting = SafetyLoad(UserSettingTools.Load, filename, failedDialog, LoadUserSettingBackupCallback);
                    setting?.SetFileStamp(fileStamp);
                }
                else
                {
                    setting = null;
                }
            }

            return setting ?? new UserSetting();
        }

        private void LoadUserSettingBackupCallback()
        {
            _backupOnce = false;
        }


        // 履歴読み込み
        public void LoadHistory()
        {
            using (ProcessLock.Lock())
            {
                var filename = HistoryFilePath;
                var failedDialog = new LoadFailedDialog("Notice.LoadHistoryFailed", "Notice.LoadHistoryFailedTitle");

                var fileInfo = new FileInfo(filename);
                if (fileInfo.Exists)
                {
                    BookHistoryCollection.Memento? memento = SafetyLoad(BookHistoryCollection.Memento.Load, HistoryFilePath, failedDialog);
                    BookHistoryCollection.Current.Restore(memento, true);
                    _historyLastWriteTime = fileInfo.GetSafeLastWriteTime();
                }
            }
        }

        // ブックマーク読み込み
        public void LoadBookmark()
        {
            LoadBookmark(BookmarkFilePath);
        }

        public void LoadBookmark(string filename)
        {
            using (ProcessLock.Lock())
            {
                if (!File.Exists(filename))
                {
                    return;
                }

                var fileStamp = FileStamp.Create(filename);
                if (fileStamp == _bookmarkFileStamp)
                {
                    return;
                }
                _bookmarkFileStamp = fileStamp;
                var failedDialog = new LoadFailedDialog("Notice.LoadBookmarkFailed", "Notice.LoadBookmarkFailedTitle");
                BookmarkCollection.Memento? memento = SafetyLoad(BookmarkCollection.Memento.Load, filename, failedDialog);
                BookmarkCollection.Current.Restore(memento);
            }
        }

        public void LoadFolderConfig()
        {
            LoadFolderConfig(FolderConfigFilePath);
        }

        // フォルダー設定読み込み
        public void LoadFolderConfig(string filename)
        {
            using (ProcessLock.Lock())
            {
                if (!File.Exists(filename))
                {
                    return;
                }

                var fileStamp = FileStamp.Create(filename);
                if (fileStamp == _folderConfigFileStamp)
                {
                    return;
                }
                _folderConfigFileStamp = fileStamp;
                var failedDialog = new LoadFailedDialog("Notice.LoadFolderConfigFailed", "Notice.LoadFolderConfigFailedTitle");
                FolderConfigCollection.Memento? memento = SafetyLoad(FolderConfigCollection.Memento.Load, filename, failedDialog);
                FolderConfigCollection.Current.Restore(memento);
            }
        }

        /// <summary>
        /// 正規ファイルの読み込みに失敗したらバックアップからの復元を試みる。
        /// エラー時にはダイアログ表示。選択によってはOperationCancelExceptionを発生させる。
        /// </summary>
        private static T? SafetyLoad<T>(Func<string, T?> load, string path, LoadFailedDialog loadFailedDialog, Action? loadBackupCallback = null)
            where T : class
        {
            try
            {
                return SafetyLoad(load, path, loadBackupCallback);
            }
            catch (Exception ex)
            {
                if (loadFailedDialog != null)
                {
                    var result = AppDispatcher.Invoke(() => loadFailedDialog.ShowDialog(ex));
                    if (result != true)
                    {
                        throw new OperationCanceledException();
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// 正規ファイルの読み込みに失敗したらバックアップからの復元を試みる
        /// </summary>
        private static T? SafetyLoad<T>(Func<string, T?> load, string path, Action? loadBackupCallback)
            where T : class
        {
            var old = path + BackupExtension;

            if (File.Exists(path))
            {
                try
                {
                    return load(path);
                }
                catch
                {
                    if (File.Exists(old))
                    {
                        loadBackupCallback?.Invoke();
                        return load(old);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else if (File.Exists(old))
            {
                return load(old);
            }
            else
            {
                return null;
            }
        }

#endregion Load

        #region Save

        /// <summary>
        /// 設定の保存
        /// </summary>
        public void SaveUserSetting()
        {
            if (!IsEnableSave) return;

            using (ProcessLock.Lock())
            {
                bool createBackup = Config.Current.System.IsSettingBackup && _backupOnce;
                SafetySave(UserSettingTools.Save, UserSettingFilePath, createBackup);
                SetUserSettingFileStamp(FileStamp.Create(UserSettingFilePath));
                _backupOnce = false;
            }
        }

        /// <summary>
        /// 古いファイルを削除
        /// </summary>
        private static void DeleteLegacyFile(string filename)
        {
            using (ProcessLock.Lock())
            {
                Debug.WriteLine($"Remove: {filename}");
                FileIO.DeleteFile(filename);

                // バックアップファイルも削除
                var backup = filename + ".old";
                if (File.Exists(backup))
                {
                    Debug.WriteLine($"Remove: {backup}");
                    FileIO.DeleteFile(backup);
                }
            }
        }

        /// <summary>
        /// 履歴をファイルに保存
        /// </summary>
        public void SaveHistory()
        {
            if (!IsEnableSave) return;
            if (!Config.Current.History.IsSaveHistory) return;

            using (ProcessLock.Lock())
            {
                //Debug.WriteLine("SaveData.SaveHistory(): saving.");
                var bookHistoryMemento = BookHistoryCollection.Current.CreateMemento();

                try
                {
                    // NOTE: 一度マージが発生したらその後は常にマージを行う。負荷が高いのが問題。
                    var fileInfo = new FileInfo(HistoryFilePath);
                    if (fileInfo.Exists && (_historyMergeFlag || fileInfo.GetSafeLastWriteTime() > _historyLastWriteTime))
                    {
                        //Debug.WriteLine("SaveData.SaveHistory(): merge.");
                        var failedDialog = new LoadFailedDialog("Notice.LoadHistoryFailed", "Notice.LoadHistoryFailedTitle");
                        var margeMemento = SafetyLoad(BookHistoryCollection.Memento.Load, HistoryFilePath, failedDialog);
                        bookHistoryMemento.Merge(margeMemento);
                        _historyMergeFlag = true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                SafetySave(bookHistoryMemento.Save, HistoryFilePath, false);
                _historyLastWriteTime = new FileInfo(HistoryFilePath).GetSafeLastWriteTime();
            }
        }

        /// <summary>
        /// 必要であるならば、Historyを削除
        /// </summary>
        public void DeleteHistoryIfNotSave()
        {
            if (!IsEnableSave) return;
            if (Config.Current.History.IsSaveHistory) return;

            using (ProcessLock.Lock())
            {
                FileIO.DeleteFile(HistoryFilePath);
            }
        }

        /// <summary>
        /// Bookmarkの保存
        /// </summary>
        public void SaveBookmark()
        {
            if (!IsEnableSave) return;
            if (!Config.Current.Bookmark.IsSaveBookmark) return;

            using (ProcessLock.Lock())
            {
                var bookmarkMemento = BookmarkCollection.Current.CreateMemento();
                SafetySave(bookmarkMemento.Save, BookmarkFilePath, false);
                _bookmarkFileStamp = FileStamp.Create(BookmarkFilePath);
            }
        }

        /// <summary>
        /// 必要であるならば、Bookmarkを削除
        /// </summary>
        public void DeleteBookmarkIfNotSave()
        {
            if (!IsEnableSave) return;
            if (Config.Current.Bookmark.IsSaveBookmark) return;

            using (ProcessLock.Lock())
            {
                FileIO.DeleteFile(BookmarkFilePath);
            }
        }

        /// <summary>
        /// 必要であるならば、古い設定ファイルを削除
        /// </summary>
        public void DeleteLegacyPagemark()
        {
            if (_pagemarkFilenameToDelete == null) return;

            DeleteLegacyFile(_pagemarkFilenameToDelete);
            _pagemarkFilenameToDelete = null;
        }

        /// <summary>
        /// フォルダー設定をファイルに保存
        /// </summary>
        public void SaveFolderConfig()
        {
            if (!IsEnableSave) return;
            //if (!Config.Current.Bookshelf.IsSaveFolderConfig) return;

            using (ProcessLock.Lock())
            {
                var folderConfigMemento = FolderConfigCollection.Current.CreateMemento();
                SafetySave(folderConfigMemento.Save, FolderConfigFilePath, false);
                _folderConfigFileStamp = FileStamp.Create(FolderConfigFilePath);
            }
        }

        /// <summary>
        /// アプリ強制終了でもファイルがなるべく破壊されないような保存
        /// </summary>
        /// <param name="save">SAVE関数</param>
        /// <param name="path">保存ファイル名</param>
        /// <param name="createBackup">バックアップを作る</param>
        private static void SafetySave(Action<string> save, string path, bool createBackup)
        {
            if (save is null) throw new ArgumentNullException(nameof(save));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

            var newFileName = path + ".new.tmp";
            var backupFileName = createBackup ? path + BackupExtension : null;

            lock (App.Current.Lock)
            {
                if (File.Exists(path))
                {
                    try
                    {
                        File.Delete(newFileName);
                        save(newFileName);
                        File.Replace(newFileName, path, backupFileName);
                    }
                    catch
                    {
                        File.Delete(newFileName);
                        throw;
                    }
                }
                else
                {
                    save(path);
                }
            }
        }

        #endregion Save
    }
}
