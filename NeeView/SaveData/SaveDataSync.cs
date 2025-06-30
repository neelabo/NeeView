//#define LOCAL_DEBUG

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using NeeLaboratory.Generators;
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
    [LocalDebug]
    public partial class SaveDataSync : IDisposable
    {
        // Note: Initialize()必須
        static SaveDataSync() => Current = new SaveDataSync();
        public static SaveDataSync Current { get; }


        private readonly DelayAction _delaySaveBookmark;
        private readonly IntervalAction _delaySaveHistory;
        private readonly UserSettingWatcher _userSettingWatcher = new UserSettingWatcher();
        private readonly BookmarkWatcher _bookmarkWatcher = new BookmarkWatcher();
        private readonly PlaylistWatcher _playlistWatcher = new PlaylistWatcher();
        private bool _disposedValue = false;


        private SaveDataSync()
        {
            _delaySaveBookmark = new DelayAction(() => SaveBookmark(true, true), TimeSpan.FromSeconds(0.5));
            _delaySaveHistory = new IntervalAction(() => SaveHistory(true), TimeSpan.FromMinutes(5.0));
        }


        public void Initialize()
        {
            BookmarkCollection.Current.BookmarkChanged += BookmarkCollection_BookmarkChanged;
            QuickAccessCollection.Current.RoutedValuePropertyChanged += QuickAccessCollection_RoutedValuePropertyChanged;
            QuickAccessCollection.Current.RoutedCollectionChanged += QuickAccessCollection_RoutedCollectionChanged;
            BookHistoryCollection.Current.HistoryChanged += BookHistoryCollection_HistoryChanged;
            BookHistoryCollection.Current.SearchChanged += Search_HistoryChanged;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _playlistWatcher.Dispose();
                    _bookmarkWatcher.Dispose();
                    _userSettingWatcher.Dispose();
                    BookmarkCollection.Current.BookmarkChanged -= BookmarkCollection_BookmarkChanged;
                    QuickAccessCollection.Current.RoutedValuePropertyChanged -= QuickAccessCollection_RoutedValuePropertyChanged;
                    QuickAccessCollection.Current.RoutedCollectionChanged -= QuickAccessCollection_RoutedCollectionChanged;
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


        public void DisposeWatcher()
        {
            _playlistWatcher.Dispose();
            _bookmarkWatcher.Dispose();
            _userSettingWatcher.Dispose();
        }

        private void QuickAccessCollection_RoutedValuePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            LocalDebug.WriteLine($"PropertyName={e.PropertyName}");
            _delaySaveBookmark.Request();
        }

        private void QuickAccessCollection_RoutedCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            LocalDebug.WriteLine($"Action={e.Action}");
            if (e.Action == NotifyCollectionChangedAction.Reset) return;
            _delaySaveBookmark.Request();
        }

        private void BookmarkCollection_BookmarkChanged(object? sender, BookmarkCollectionChangedEventArgs e)
        {
            LocalDebug.WriteLine($"Action={e.Action}");
            if (e.Action == EntryCollectionChangedAction.Reset) return;
            _delaySaveBookmark.Request();
        }

        private void BookHistoryCollection_HistoryChanged(object? sender, BookMementoCollectionChangedArgs e)
        {
            if (e.HistoryChangedType == BookMementoCollectionChangedType.Load) return;
            LocalDebug.WriteLine($"");
            _delaySaveHistory.Request();
        }

        private void Search_HistoryChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset) return;
            LocalDebug.WriteLine($"Sender={sender}, Action={e.Action}");
            _delaySaveHistory.Request();
        }

        public void Flush()
        {
            _delaySaveBookmark.Flush();
            _delaySaveHistory.Flush();
            PlaylistHub.Current.Flush();
        }

        public void SaveUserSetting(bool sync, bool handleException)
        {
            LocalDebug.WriteLine($"Save UserSetting");

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
        }

        public void SaveHistory(bool handleException)
        {
            LocalDebug.WriteLine($"Save History");

            try
            {
                _delaySaveHistory.Cancel();
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

        public void SaveBookmark(bool sync, bool handleException)
        {
            LocalDebug.WriteLine($"Save Bookmark");

            try
            {
                _delaySaveBookmark?.Cancel();
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
