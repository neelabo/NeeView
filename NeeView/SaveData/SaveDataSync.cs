﻿using System;
using System.Diagnostics;
using System.IO;
using NeeLaboratory.IO;
using NeeView.Data;
using NeeView.Threading;

namespace NeeView
{
    /// <summary>
    /// ブックマーク、ページマークは変更のたびに保存。
    /// 他プロセスからの要求でリロードを行う。
    /// </summary>
    public class SaveDataSync : IDisposable
    {
        // Note: Initialize()必須
        static SaveDataSync() => Current = new SaveDataSync();
        public static SaveDataSync Current { get; }


        private readonly DelayAction _delaySaveBookmark;
        private readonly DelayAction _delaySaveHistory;


        private SaveDataSync()
        {
            _delaySaveBookmark = new DelayAction(() => SaveBookmark(true, true), TimeSpan.FromSeconds(0.5));
            _delaySaveHistory = new DelayAction(() => SaveHistory(true), TimeSpan.FromSeconds(30.0));

            RemoteCommandService.Current.AddReciever("LoadUserSetting", LoadUserSetting);
            RemoteCommandService.Current.AddReciever("LoadHistory", LoadHistory);
            RemoteCommandService.Current.AddReciever("LoadBookmark", LoadBookmark);
            RemoteCommandService.Current.AddReciever("LoadPlaylist", LoadPlaylist);
            RemoteCommandService.Current.AddReciever("RenamePlaylist", RenamePlaylist);
        }


        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    BookmarkCollection.Current.BookmarkChanged -= BookmarkCollection_BookmarkChanged;
                    BookHistoryCollection.Current.HistoryChanged -= BookHistoryCollection_HistoryChanged;
                    _delaySaveBookmark.Dispose();
                    _delaySaveHistory.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion IDisposable Support


        public void Initialize()
        {
            BookmarkCollection.Current.BookmarkChanged += BookmarkCollection_BookmarkChanged;
            BookHistoryCollection.Current.HistoryChanged += BookHistoryCollection_HistoryChanged;
        }


        private void BookmarkCollection_BookmarkChanged(object? sender, BookmarkCollectionChangedEventArgs e)
        {
            if (e.Action == EntryCollectionChangedAction.Reset) return;
            _delaySaveBookmark.Request();
        }

        private void BookHistoryCollection_HistoryChanged(object? sender, BookMementoCollectionChangedArgs e)
        {
            //Debug.WriteLine($"{nameof(SaveDataSync)}.{nameof(BookHistoryCollection_HistoryChanged)}: Request.");
            _delaySaveHistory.Request();
        }

        public void Flush()
        {
            _delaySaveBookmark.Flush();
            _delaySaveHistory.Flush();
            PlaylistHub.Current.Flush();
        }

        private void LoadUserSetting(RemoteCommand command)
        {
            Debug.WriteLine($"{SaveData.UserSettingFileName} is updated by other process.");
            var setting = SaveData.Current.LoadUserSetting(false);
            UserSettingTools.Restore(setting);
        }

        private void LoadHistory(RemoteCommand command)
        {
            throw new NotImplementedException();
            // TODO: フラグ管理のみ？
        }

        private void LoadBookmark(RemoteCommand command)
        {
            Debug.WriteLine($"{SaveData.BookmarkFileName} is updated by other process.");
            SaveData.Current.LoadBookmark();
        }

        private void LoadPlaylist(RemoteCommand command)
        {
            PlaylistHub.Current.Reload(command.Args[0]);
        }

        private void RenamePlaylist(RemoteCommand command)
        {
            PlaylistHub.Current.Rename(command.Args[0], command.Args[1]);
        }


        public void SaveUserSetting(bool sync, bool handleException)
        {
            Debug.WriteLine($"Save UserSetting");

            try
            {
                SaveData.Current.SaveUserSetting();
            }
            catch (Exception ex)
            {
                var message = Properties.Resources.FailedToSaveDataDialog_Setting_Message + System.Environment.NewLine + ex.Message;
                if (handleException)
                {
                    ToastService.Current.Show(new Toast(message, Properties.Resources.FailedToSaveDataDialog_Title, ToastIcon.Error));
                    return;
                }
                else
                {
                    throw new IOException(message, ex);
                }
            }

            // TODO: 動作検証用に古い形式のデータも保存する
            ////SaveData.Current.SaveUserSettingV1();

            if (sync)
            {
                RemoteCommandService.Current.Send(new RemoteCommand("LoadUserSetting"), RemoteCommandDelivery.All);
            }
        }

        private static void SaveHistory(bool handleException)
        {
            Debug.WriteLine($"Save History");

            try
            {
                SaveData.Current.SaveHistory();
            }
            catch (Exception ex)
            {
                var message = Properties.Resources.FailedToSaveDataDialog_History_Message + System.Environment.NewLine + ex.Message;
                if (handleException)
                {
                    ToastService.Current.Show(new Toast(message, Properties.Resources.FailedToSaveDataDialog_Title, ToastIcon.Error));
                    return;
                }
                else
                {
                    throw new IOException(message, ex);
                }
            }
        }

        public void SaveBookmark(bool sync, bool handleException)
        {
            Debug.WriteLine($"Save Bookmark");

            try
            {
                SaveData.Current.SaveBookmark();
            }
            catch (Exception ex)
            {
                var message = Properties.Resources.FailedToSaveDataDialog_Bookmark_Message + System.Environment.NewLine + ex.Message;
                if (handleException)
                {
                    ToastService.Current.Show(new Toast(message, Properties.Resources.FailedToSaveDataDialog_Title, ToastIcon.Error));
                    return;
                }
                else
                {
                    throw new IOException(message, ex);
                }
            }

            if (sync)
            {
                RemoteCommandService.Current.Send(new RemoteCommand("LoadBookmark"), RemoteCommandDelivery.All);
            }
        }

        private static void RemoveHistoryIfNotSave()
        {
            SaveData.Current.RemoveHistoryIfNotSave();
        }

        private static void RemoveBookmarkIfNotSave()
        {
            SaveData.Current.RemoveBookmarkIfNotSave();
        }

        /// <summary>
        /// すべてのセーブ処理を行う
        /// </summary>
        public void SaveAll(bool sync, bool handleException)
        {
            Flush();
            SaveUserSetting(sync, handleException);

            RemoveHistoryIfNotSave();
            RemoveBookmarkIfNotSave();
        }
    }
}
