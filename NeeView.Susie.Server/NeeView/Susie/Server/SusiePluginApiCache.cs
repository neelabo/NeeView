using System;
using System.Diagnostics;

namespace NeeView.Susie.Server
{
    /// <summary>
    /// プラグインAPIのキャッシュ
    /// </summary>
    /// <remarks>
    /// APIのセッション単位でモジュールをアンロードするか、そのまま使い回すかを管理する。
    /// セッションは Lock() で取得し、その Dispose() で解放する。
    /// </remarks>
    public class SusiePluginApiCache : IDisposable
    {
        private readonly SusiePlugin _plugin;
        private readonly Locker _locker;
        private SusiePluginApi? _api;
        private bool _disposedValue;

        public SusiePluginApiCache(SusiePlugin plugin)
        {
            _plugin = plugin;

            _locker = new Locker();
            _locker.LockCountChanged += Locker_LockCountChanged;
        }

        public event EventHandler<ModuleLoadedEventArgs>? ModuleLoaded;


        public SusiePluginApi? Api => _api;


        public SusiePluginApiAdapter Lock()
        {
            if (_disposedValue) throw new ObjectDisposedException(nameof(SusiePluginApiCache));

            var key = _locker.Lock();
            var module = _api ?? throw new InvalidOperationException();
            return new SusiePluginApiAdapter(key, module);
        }

        private void Locker_LockCountChanged(object? sender, LockCountChangedEventArgs e)
        {
            if (e.IsLocked)
            {
                Open();
            }
            else
            {
                Close();
            }
        }

        private void Open()
        {
            if (_plugin.FileName == null) throw new InvalidOperationException();

            if (_api == null)
            {
                Trace.WriteLine($"SusiePlugin.LoadModule: {_plugin.FileName}");
                _api = SusiePluginApi.Create(_plugin.FileName);
                ModuleLoaded?.Invoke(this, new ModuleLoadedEventArgs(_api));
            }
        }

        private void Close()
        {
            // reset FPU
            // セッション単位でFPUをリセットする
            NativeMethods._fpreset();

            if (_plugin.IsCacheEnabled)
            {
                return;
            }

            UnloadModule();
        }

        public void UnloadModule()
        {
            if (_api is null) return;

            Trace.WriteLine($"SusiePlugin.UnloadModule: {_plugin.FileName}");
            _api.Dispose();
            _api = null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    UnloadModule();
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


    public class ModuleLoadedEventArgs : EventArgs
    {
        public ModuleLoadedEventArgs(SusiePluginApi module)
        {
            Module = module;
        }
        public SusiePluginApi Module { get; }
    }

}
