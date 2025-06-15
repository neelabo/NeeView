using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using NeeLaboratory.IO;
using NeeView.Collections.Generic;
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
        private readonly IntervalAction _delaySaveHistory;


        private SaveDataSync()
        {
            _delaySaveBookmark = new DelayAction(() => SaveBookmark(true, true), TimeSpan.FromSeconds(0.5));
            _delaySaveHistory = new IntervalAction(() => SaveHistory(true), TimeSpan.FromMinutes(5.0));

            RemoteCommandService.Current.AddReceiver("LoadUserSetting", LoadUserSetting);
            RemoteCommandService.Current.AddReceiver("LoadHistory", LoadHistory);
            RemoteCommandService.Current.AddReceiver("LoadBookmark", LoadBookmark);
            RemoteCommandService.Current.AddReceiver("LoadPlaylist", LoadPlaylist);
            RemoteCommandService.Current.AddReceiver("RenamePlaylist", RenamePlaylist);
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
                    QuickAccessCollection.Current.RoutedPropertyChanged -= QuickAccessCollection_RoutedPropertyChanged;
                    QuickAccessCollection.Current.CollectionChanged -= QuickAccessCollection_CollectionChanged;
                    BookHistoryCollection.Current.HistoryChanged -= BookHistoryCollection_HistoryChanged;
                    BookHistoryCollection.Current.SearchChanged -= Search_HistoryChanged;
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
            QuickAccessCollection.Current.RoutedPropertyChanged += QuickAccessCollection_RoutedPropertyChanged;
            QuickAccessCollection.Current.CollectionChanged += QuickAccessCollection_CollectionChanged;
            BookHistoryCollection.Current.HistoryChanged += BookHistoryCollection_HistoryChanged;
            BookHistoryCollection.Current.SearchChanged += Search_HistoryChanged;
        }

        private void QuickAccessCollection_RoutedPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            _delaySaveBookmark.Request();
        }

        private void QuickAccessCollection_CollectionChanged(object? sender, TreeCollectionChangeEventArgs<IQuickAccessEntry> e)
        {
            if (e.Action == TreeCollectionChangeAction.Refresh) return;
            _delaySaveBookmark.Request();
        }

        private void BookmarkCollection_BookmarkChanged(object? sender, BookmarkCollectionChangedEventArgs e)
        {
            if (e.Action == EntryCollectionChangedAction.Reset) return;
            _delaySaveBookmark.Request();
        }

        private void BookHistoryCollection_HistoryChanged(object? sender, BookMementoCollectionChangedArgs e)
        {
            if (e.HistoryChangedType == BookMementoCollectionChangedType.Load) return;
            //Debug.WriteLine($"{nameof(SaveDataSync)}.{nameof(BookHistoryCollection_HistoryChanged)}: Request.");
            _delaySaveHistory.Request();
        }
        
        private void Search_HistoryChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset) return;
            //Debug.WriteLine($"{nameof(SaveDataSync)}.{nameof(Search_HistoryChanged)}: Request: {sender}, {e.Action}");
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
            Debug.WriteLine($"{SaveDataProfile.UserSettingFileName} is updated by other process.");
            var setting = SaveData.Current.LoadUserSetting(false);
            try
            {
                Config.Current.Window.FreezeWindowState = true;
                UserSettingTools.Restore(setting);
            }
            finally
            {
                Config.Current.Window.FreezeWindowState = false;
            }
        }

        private void LoadHistory(RemoteCommand command)
        {
            // TODO: フラグ管理のみ？
        }

        private void LoadBookmark(RemoteCommand command)
        {
            Debug.WriteLine($"{SaveDataProfile.BookmarkFileName} is updated by other process.");
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
                var message = Properties.TextResources.GetString("FailedToSaveDataDialog.Setting.Message") + System.Environment.NewLine + ex.Message;
                if (handleException)
                {
                    ToastService.Current.Show(new Toast(message, Properties.TextResources.GetString("FailedToSaveDataDialog.Title"), ToastIcon.Error));
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

        public static void SaveHistory(bool handleException)
        {
            Debug.WriteLine($"Save History");

            try
            {
                SaveData.Current.SaveHistory();
            }
            catch (Exception ex)
            {
                var message = Properties.TextResources.GetString("FailedToSaveDataDialog.History.Message") + System.Environment.NewLine + ex.Message;
                if (handleException)
                {
                    ToastService.Current.Show(new Toast(message, Properties.TextResources.GetString("FailedToSaveDataDialog.Title"), ToastIcon.Error));
                    return;
                }
                else
                {
                    throw new IOException(message, ex);
                }
            }
        }

        public static void SaveBookmark(bool sync, bool handleException)
        {
            Debug.WriteLine($"Save Bookmark");

            try
            {
                SaveData.Current.SaveBookmark();
            }
            catch (Exception ex)
            {
                var message = Properties.TextResources.GetString("FailedToSaveDataDialog.Bookmark.Message") + System.Environment.NewLine + ex.Message;
                if (handleException)
                {
                    ToastService.Current.Show(new Toast(message, Properties.TextResources.GetString("FailedToSaveDataDialog.Title"), ToastIcon.Error));
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
            SaveData.Current.DeleteHistoryIfNotSave();
        }

        private static void RemoveBookmarkIfNotSave()
        {
            SaveData.Current.DeleteBookmarkIfNotSave();
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
