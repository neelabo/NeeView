using NeeLaboratory;
using NeeLaboratory.Remote;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView.Susie.Client
{
    public class SusiePluginRemoteClient : IDisposable
    {
        private SubProcess? _subProcess;
        private SimpleClient? _client;
        private CancellationTokenSource? _cancellationTokenSource;

        public bool IsConnected => _subProcess != null && _subProcess.IsActive;

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_subProcess != null)
                    {
                        _subProcess.Dispose();
                        _subProcess = null;
                    }

                    _client = null;
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


        public void Connect()
        {
            if (_subProcess != null) return;

            var subProcessFileName = Path.Combine(Environment.LibrariesPath, @"Libraries\Susie\NeeView.Susie.Server.exe");

            // 開発中は直接プロジェクトを参照する
            if (Environment.IsDevPackage)
            {
                subProcessFileName = Path.GetFullPath(Path.Combine(Environment.AssemblyFolder, @"..\..\..\..\..\NeeView.Susie.Server\bin\x86", Environment.ConfigType, @"net9.0\NeeView.Susie.Server.exe"));
            }

            if (!File.Exists(subProcessFileName))
            {
                throw new FileNotFoundException($"File not found: {subProcessFileName}");
            }

            var subProcess = new SubProcess(subProcessFileName, SusiePluginRemote.BootKeyword);
            subProcess.Start();
            if (subProcess.Process is null) throw new InvalidOperationException($"Cannot start process: {subProcessFileName}");
            _subProcess = subProcess;

            _cancellationTokenSource = new CancellationTokenSource();
            _subProcess.Exited += (s, e) => _cancellationTokenSource.Cancel();

            var name = SusiePluginRemote.CreateServerName(_subProcess.Process);
            _client = new SimpleClient(name);
        }

        public void Disconnect()
        {
            if (_subProcess == null) return;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _subProcess.Dispose();
            _subProcess = null;
            _client = null;
        }

        public async ValueTask<List<Chunk>> CallAsync(List<Chunk> args, CancellationToken token)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Disconnected from SusiePlugin Server.");
            }

            if (_client is null) throw new InvalidOperationException("_client must not be null");
            if (_cancellationTokenSource is null) throw new InvalidOperationException("_cancellationTokenSource must not be null");

            using (var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, token))
            {
                return await _client.CallAsync(args, linkedCancellationTokenSource.Token);
            }
        }
    }
}
