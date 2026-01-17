using NeeView.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// サムネイルキャッシュ
    /// </summary>
    public class ThumbnailCache : IDisposable
    {
        static ThumbnailCache() => Current = new ThumbnailCache();
        public static ThumbnailCache Current { get; }


        public static string ThumbnailCacheFileName => "Cache.db";
        public static string DefaultThumbnailCacheFilePath => Path.Combine(Environment.LocalApplicationDataPath, ThumbnailCacheFileName);


        private ThumbnailCacheConnection? _connection;
        private readonly System.Threading.Lock _lock = new();
        private Dictionary<string, ThumbnailCacheItem> _saveQueue;
        private Dictionary<string, ThumbnailCacheHeader> _updateQueue;
        private readonly DelayAction _delaySaveQueue;
        private readonly System.Threading.Lock _lockSaveQueue = new();
        private bool _isEnabled = true;



        private ThumbnailCache()
        {
            _saveQueue = new Dictionary<string, ThumbnailCacheItem>();
            _updateQueue = new Dictionary<string, ThumbnailCacheHeader>();
            _delaySaveQueue = new DelayAction(SaveQueue, TimeSpan.FromSeconds(2.0));

            App.Current.CriticalError += (s, e) => Disable();
        }



        /// <summary>
        /// キャッシュDBのパス
        /// </summary>
        public static string DatabasePath => Config.Current.Thumbnail.ThumbnailCacheFilePath ?? DefaultThumbnailCacheFilePath;

        /// <summary>
        /// キャッシュ有効フラグ
        /// </summary>
        private bool IsEnabled => Config.Current.Thumbnail.IsCacheEnabled && _isEnabled;


        /// <summary>
        /// 機能停止
        /// </summary>
        public void Disable()
        {
            _isEnabled = false;
        }

        /// <summary>
        /// DBファイルサイズを取得
        /// </summary>
        public static long GetCacheDatabaseSize()
        {
            if (DatabasePath == null) throw new InvalidOperationException();

            var fileInfo = new FileInfo(DatabasePath);
            if (fileInfo.Exists)
            {
                return fileInfo.Length;
            }
            else
            {
                return 0L;
            }
        }

        /// <summary>
        /// DBを開く
        /// </summary>
        /// <param name="filename"></param>
        internal ThumbnailCacheConnection? Open()
        {
            lock (_lock)
            {
                if (_disposedValue) return null;
                if (!IsEnabled) return null;

                if (_connection != null) return _connection;

                try
                {
                    _connection = new ThumbnailCacheConnection(DatabasePath);
                }
                catch (ThumbnailCacheFormatException)
                {
                    Remove();
                    try
                    {
                        _connection = new ThumbnailCacheConnection(DatabasePath);
                    }
                    catch (Exception ex)
                    {
                        ToastService.Current.Show(new Toast($"Cannot open thumbnail database.\n{ex.Message}"));
                        _isEnabled = false;
                        return null;
                    }
                }

                return _connection;
            }
        }

        /// <summary>
        /// DBを閉じる
        /// </summary>
        internal void Close()
        {
            lock (_lock)
            {
                if (_connection != null)
                {
                    _connection.Dispose();
                    _connection = null;
                }
            }
        }

        /// <summary>
        /// DB削除
        /// </summary>
        internal void Remove()
        {
            Close();

            var fileInfo = new FileInfo(DatabasePath);
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }
        }

        /// <summary>
        /// DB掃除 (とても重い)
        /// </summary>
        internal void Vacuum()
        {
            if (_disposedValue) return;
            if (!IsEnabled) return;

            Open()?.Vacuum();
        }

        /// <summary>
        /// 古いサムネイルを削除
        /// </summary>
        /// <param name=""></param>
        internal void Delete(TimeSpan limitTime)
        {
            if (_disposedValue) return;
            if (!IsEnabled) return;

            Open()?.Delete(limitTime);
        }

        /// <summary>
        /// サムネイルの保存
        /// </summary>
        /// <param name="header"></param>
        /// <param name="data"></param>
        internal void Save(ThumbnailCacheHeader header, byte[] data)
        {
            if (_disposedValue) return;
            if (!IsEnabled) return;

            Open()?.Save(header, data);
        }


        /// <summary>
        /// サムネイルの読込
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        internal async ValueTask<byte[]?> LoadAsync(ThumbnailCacheHeader header, CancellationToken token)
        {
            if (_disposedValue) return null;
            if (!IsEnabled) return null;
            if (!header.IsValid()) return null;

            var connection = Open();
            var record = connection != null ? await connection.LoadAsync(header, token) : null;
            if (record != null)
            {
                // 1日以上古い場合はキャッシュのアクセス日時を更新する
                if ((header.AccessTime - record.DateTime).TotalDays > 1.0)
                {
                    EntryUpdateQueue(header);
                }
                return record.Bytes;
            }

            // SaveQueueからも探す
            lock (_lockSaveQueue)
            {
                if (_saveQueue.TryGetValue(header.Key, out ThumbnailCacheItem? item))
                {
                    return item.Body;
                }
            }

            return null;
        }

        /// <summary>
        /// サムネイルの保存予約
        /// </summary>
        /// <param name="header"></param>
        /// <param name="data"></param>
        internal void EntrySaveQueue(ThumbnailCacheHeader header, byte[] data)
        {
            if (_disposedValue) return;
            if (!IsEnabled) return;
            if (!header.IsValid()) return;

            lock (_lockSaveQueue)
            {
                _saveQueue[header.Key] = new ThumbnailCacheItem(header, data);
            }

            _delaySaveQueue.Request();
        }

        /// <summary>
        /// 日付更新の予約
        /// </summary>
        /// <param name="header"></param>
        internal void EntryUpdateQueue(ThumbnailCacheHeader header)
        {
            if (_disposedValue) return;
            if (!IsEnabled) return;

            lock (_lockSaveQueue)
            {
                _updateQueue[header.Key] = header;
            }

            _delaySaveQueue.Request();
        }

        /// <summary>
        /// サムネイルの保存予約実行
        /// </summary>
        private void SaveQueue()
        {
            if (_disposedValue) return;
            if (!IsEnabled) return;

            var saveQueue = _saveQueue;
            var updateQueue = _updateQueue;
            lock (_lockSaveQueue)
            {
                _saveQueue = new Dictionary<string, ThumbnailCacheItem>();
                _updateQueue = new Dictionary<string, ThumbnailCacheHeader>();
            }

            Debug.WriteLine($"ThumbnailCache.Save: {saveQueue.Count},{updateQueue.Count} ..");

            Open()?.SaveQueue(saveQueue, updateQueue);
        }

        /// <summary>
        /// キャッシュ吐き出し、キャッシュリミット適用
        /// </summary>
        public void Cleanup()
        {
            if (_disposedValue) return;
            if (!IsEnabled) return;

            _delaySaveQueue.Flush();

            if (Config.Current.Thumbnail.CacheLimitSpan != default)
            {
                Delete(Config.Current.Thumbnail.CacheLimitSpan);
            }
        }


        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _delaySaveQueue.Dispose();

                    lock (_lock)
                    {
                        _connection?.Dispose();
                        _connection = null;
                    }
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
