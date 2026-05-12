using NeeLaboratory.IO;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class AppRemoteCommandServer : RemoteCommandServer, IDisposable
    {
        private readonly static Lazy<AppRemoteCommandServer> _current = new();
        public static AppRemoteCommandServer Current => _current.Value;

        public static string PipeName { get; } = AppRemoteCommandTools.CreatePipeName(Process.GetCurrentProcess());
        private Task? _task;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _disposedValue;

        public AppRemoteCommandServer() : base(PipeName, AppRemoteCommandJsonContext.Default)
        {
            RegisterMethod<string[]>(nameof(Restart), Restart);
            RegisterMethod<int, bool>(nameof(IsHideWindow), IsHideWindow);
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

        public void Start()
        {
            if (_task is not null) return;

            _cancellationTokenSource = new();
            _task = StartAsync(_cancellationTokenSource.Token);
        }

        public void Stop()
        {
            if (_task is null) return;

            _cancellationTokenSource?.Cancel();
            _task = null;
        }


        public async void Restart(string[] args)
        {
            try
            {
                await AppState.Current.ResumeAsync(args);
            }
            catch { }
        }

        public bool IsHideWindow(int _)
        {
            try
            {
                return AppState.Current.IsTaskTrayEnabled && AppState.Current.IsHideWindow;
            }
            catch
            {
                return false;
            }
        }
    }

}
