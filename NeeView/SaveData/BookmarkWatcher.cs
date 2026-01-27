using NeeLaboratory.ComponentModel;
using NeeView.Threading;
using System;
using System.IO;

namespace NeeView
{
    /// <summary>
    /// Monitors changes to the bookmark file and triggers appropriate actions when updates are detected.
    /// </summary>
    /// <remarks>This class watches the bookmark file specified in the application configuration and responds
    /// to changes by invoking delayed actions to reload bookmark data. It automatically updates its monitoring target
    /// if the bookmark file path in the configuration changes.</remarks>
    public class BookmarkWatcher : IDisposable
    {
        private const int _delayTime = 500; // ms

        private readonly DataFileWatcher _watcher;
        private readonly DelayAction _delayAction = new();
        private readonly DisposableCollection _disposables = new();
        private bool _disposedValue;

        public BookmarkWatcher()
        {
            _watcher = new DataFileWatcher(Config.Current.Bookmark.BookmarkFilePath);
            _watcher.Changed += BookmarkWatcher_Changed;
            _disposables.Add(_watcher);

            _disposables.Add(Config.Current.Bookmark.SubscribePropertyChanged(nameof(BookmarkConfig.BookmarkFilePath),
                (s, e) => Reset()));

            _disposables.Add(_delayAction);
        }

        public void Reset()
        {
            _watcher.Start(Config.Current.Bookmark.BookmarkFilePath);
        }

        private void BookmarkWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Reload();
        }

        public void Reload()
        {
            _delayAction.Request(() => SaveData.Current.LoadBookmark(), TimeSpan.FromMilliseconds(_delayTime));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();
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
