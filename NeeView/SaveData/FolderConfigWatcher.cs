using NeeLaboratory.ComponentModel;
using NeeView.Threading;
using System;
using System.IO;

namespace NeeView
{
    public class FolderConfigWatcher : IDisposable
    {
        private const int _delayTime = 500; // ms

        private readonly DataFileWatcher _watcher;
        private readonly DelayAction _delayAction = new();
        private readonly DisposableCollection _disposables = new();
        private bool _disposedValue;

        public FolderConfigWatcher()
        {
            _watcher = new DataFileWatcher(Config.Current.Bookshelf.FolderConfigFilePath);
            _watcher.Changed += FolderConfigWatcher_Changed;
            _disposables.Add(_watcher);

            _disposables.Add(Config.Current.Bookshelf.SubscribePropertyChanged(nameof(BookshelfConfig.FolderConfigFilePath),
                (s, e) => Reset()));

            _disposables.Add(_delayAction);
        }

        public void Reset()
        {
            _watcher.Start(Config.Current.Bookshelf.FolderConfigFilePath);
        }

        private void FolderConfigWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Reload();
        }

        public void Reload()
        {
            _delayAction.Request(() => SaveData.Current.LoadFolderConfig(), TimeSpan.FromMilliseconds(_delayTime));
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
