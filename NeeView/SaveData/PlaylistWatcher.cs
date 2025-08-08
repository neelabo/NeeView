using System;
using System.IO;
using NeeLaboratory.ComponentModel;
using NeeView.Threading;

namespace NeeView
{
    /// <summary>
    /// Monitors changes to the playlist file and triggers appropriate actions when the file is modified or deleted.
    /// </summary>
    /// <remarks>This class watches the current playlist file specified in the configuration and responds to
    /// changes such as file modifications or deletions. When a change is detected, it delays the reload operation to
    /// avoid frequent reloads caused by rapid successive changes. The watcher automatically updates its target file if
    /// the playlist configuration changes.</remarks>
    public class PlaylistWatcher : IDisposable
    {
        private const int _delayTime = 500; // ms

        private readonly DataFileWatcher _watcher;
        private readonly DelayAction _delayAction = new();
        private readonly DisposableCollection _disposables = new();
        private bool _disposedValue;

        public PlaylistWatcher()
        {
            _watcher = new DataFileWatcher(Config.Current.Playlist.CurrentPlaylist);
            _watcher.Changed += PlaylistWatcher_Changed;
            _watcher.Deleted += PlaylistWatcher_Changed; // ファイル削除や名前変更にも追従
            _disposables.Add(_watcher);

            _disposables.Add(Config.Current.Playlist.SubscribePropertyChanged(nameof(PlaylistConfig.CurrentPlaylist),
                (s, e) => Reset()));

            _disposables.Add(PlaylistHub.Current.SubscribePlaylistSaved(
                (s, e) => Reset()));

            _disposables.Add(PlaylistHub.Current.SubscribeRefreshed(
                (s, e) => Reset()));

            _disposables.Add(_delayAction);
        }

        public void Reset()
        {
            _watcher.Start(Config.Current.Playlist.CurrentPlaylist);
        }

        private void PlaylistWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            _delayAction.Request(() => PlaylistHub.Current.Reload(e.FullPath), TimeSpan.FromMilliseconds(_delayTime));
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
