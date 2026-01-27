using NeeLaboratory.Generators;
using NeeView.Threading;
using System;
using System.IO;


namespace NeeView
{
    public partial class ScriptFolderWatcher : IDisposable
    {
        private FileSystemWatcher? _watcher;
        private readonly DelayAction _delayUpdate = new();
        private bool _disposedValue;


        public ScriptFolderWatcher()
        {
        }


        [Subscribable]
        public event FileSystemEventHandler? Changed;


        public void Start(string path)
        {
            if (_disposedValue) return;

            if (_watcher != null)
            {
                if (_watcher.Path == path) return;
            }

            Stop();

            try
            {
                _watcher = new FileSystemWatcher(path, "*.nvjs");
                _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
                _watcher.Created += Watcher_Changed;
                _watcher.Deleted += Watcher_Changed;
                _watcher.Renamed += Watcher_Changed;
                _watcher.Changed += Watcher_Changed;
                _watcher.EnableRaisingEvents = true;
            }
            catch
            {
                // 監視できなくても大きな問題はないので例外を無視する
                _watcher = null;
            }
        }

        public void Stop()
        {
            _watcher?.Dispose();
            _watcher = null;
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            _delayUpdate.Request(() => RaiseChanged(sender, e), TimeSpan.FromSeconds(1.0));
        }

        private void RaiseChanged(object sender, FileSystemEventArgs e)
        {
            Changed?.Invoke(this, e);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Stop();
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
