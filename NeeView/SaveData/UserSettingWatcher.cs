//#define LOCAL_DEBUG

using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.Threading;
using System;
using System.Diagnostics;
using System.IO;

namespace NeeView
{
    /// <summary>
    /// Monitors changes to the user settings file and triggers appropriate actions when updates are detected.
    /// </summary>
    /// <remarks>This class watches the user settings file for changes and ensures that updates are processed
    /// with a delay to avoid frequent reloads. It uses a <see cref="DataFileWatcher"/> to monitor file changes and a
    /// <see cref="DelayAction"/>  to debounce reload requests. When the settings file is updated, the class reloads the
    /// settings and applies them  to the application state.</remarks>
    [LocalDebug]
    public partial class UserSettingWatcher : IDisposable
    {
        private const int _delayTime = 500; // ms

        private readonly DataFileWatcher _watcher;
        private readonly DelayAction _delayAction = new();
        private readonly DisposableCollection _disposables = new();
        private bool _disposedValue;

        public UserSettingWatcher()
        {
            var path = App.Current.Option.SettingFilename ?? throw new InvalidOperationException("Setting filename is not specified in the application options.");

            _watcher = new DataFileWatcher(path);
            _watcher.Changed += DataFileWatcher_Changed;
            _disposables.Add(_watcher);

            _disposables.Add(_delayAction);
        }

        private void DataFileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            _delayAction.Request(() => LoadUserSetting(), TimeSpan.FromMilliseconds(_delayTime));
        }

        private void LoadUserSetting()
        {
            if (SaveData.Current.IsLatestUserSettingFileStamp())
            {
                return;
            }

            LocalDebug.WriteLine($"UserSetting file is updated.");
            var setting = SaveData.Current.LoadUserSetting(false);
            try
            {
                Config.Current.Window.FreezeWindowState = true;
                SaveData.Current.SetUserSettingFileStamp(setting.FileStamp);
                UserSettingTools.Restore(setting);
            }
            finally
            {
                Config.Current.Window.FreezeWindowState = false;
            }
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
