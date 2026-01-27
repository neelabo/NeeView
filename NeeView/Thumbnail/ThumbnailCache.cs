using NeeView.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// サムネイルキャッシュ
    /// </summary>
    public class ThumbnailCache : IDisposable
    {
        private static readonly Lazy<ThumbnailCache> _current = new();
        public static ThumbnailCache Current => _current.Value;


        private ThumbnailCacheDatabase? _thumbnailCacheTable;
        private readonly Lock _lock = new();
        private Dictionary<string, ThumbnailCacheItem> _saveQueue;
        private Dictionary<string, ThumbnailCacheHeader> _updateQueue;
        private readonly DelayAction _delaySaveQueue;
        private readonly Lock _lockSaveQueue = new();
        private bool _isEnabled = true;



        public ThumbnailCache()
        {
            _saveQueue = new Dictionary<string, ThumbnailCacheItem>();
            _updateQueue = new Dictionary<string, ThumbnailCacheHeader>();
            _delaySaveQueue = new DelayAction(SaveQueue, TimeSpan.FromSeconds(2.0));

            App.Current.CriticalError += (s, e) => Disable();
        }


        /// <summary>
        /// キャッシュ有効フラグ
        /// </summary>
        private bool IsEnabled => Config.Current.Thumbnail.IsCacheEnabled && _isEnabled;

        /// <summary>
        /// サムネイルキャッシュテーブル
        /// </summary>
        private ThumbnailCacheDatabase? ThumbnailCacheTable
        {
            get
            {
                lock (_lock)
                {
                    if (_disposedValue) return null;
                    if (!IsEnabled) return null;
                    if (_thumbnailCacheTable != null) return _thumbnailCacheTable;

                    try
                    {
                        _thumbnailCacheTable = new ThumbnailCacheDatabase(Database.Current);
                    }
                    catch (Exception ex)
                    {
                        ToastService.Current.Show(new Toast($"Cannot open thumbnail database.\n{ex.Message}"));
                        _isEnabled = false;
                        return null;
                    }

                    return _thumbnailCacheTable;
                }
            }
        }


        /// <summary>
        /// 機能停止
        /// </summary>
        public void Disable()
        {
            _isEnabled = false;
        }

        /// <summary>
        /// すべてのサムネイルを削除 
        /// </summary>
        internal void Remove()
        {
            if (_disposedValue) return;
            if (!IsEnabled) return;

            ThumbnailCacheTable?.DeleteAll();

            // とてもおもい
            //Database.Current.Vacuum();
        }

        /// <summary>
        /// DB掃除 (とても重い)
        /// </summary>
        internal void Vacuum()
        {
            if (_disposedValue) return;
            if (!IsEnabled) return;

            Database.Current.Vacuum();
        }

        /// <summary>
        /// 古いサムネイルを削除
        /// </summary>
        /// <param name=""></param>
        internal void Delete(TimeSpan limitTime)
        {
            if (_disposedValue) return;
            if (!IsEnabled) return;

            ThumbnailCacheTable?.Delete(limitTime);
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

            ThumbnailCacheTable?.Save(header, data);
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

            var thumbnailCache = ThumbnailCacheTable;
            var record = thumbnailCache != null ? await thumbnailCache.LoadAsync(header, token) : null;
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

            ThumbnailCacheTable?.SaveQueue(saveQueue, updateQueue);
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
                        _thumbnailCacheTable = null;
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
