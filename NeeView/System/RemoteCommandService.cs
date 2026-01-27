using NeeLaboratory.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public delegate void RemoteCommandReceiver(RemoteCommand command);

    /// <summary>
    /// RemoteCommandの送受信を管理
    /// </summary>
    public class RemoteCommandService : IDisposable
    {
        static RemoteCommandService() => Current = new RemoteCommandService();
        public static RemoteCommandService Current { get; }


        private readonly RemoteCommandServer _server;
        private readonly RemoteCommandClient _client;

        private readonly Dictionary<string, RemoteCommandReceiver> _receivers = new();


        public RemoteCommandService()
        {
            _server = new RemoteCommandServer();
            _server.Called += Receiver;
            _server.Start();

            _client = new RemoteCommandClient(Environment.SolutionName);

            // アプリ終了前の開放予約
            ApplicationDisposer.Current.Add(this);
        }


        [Conditional("DEBUG")]
        public void StopServer()
        {
            _server.Stop();
        }

        public void AddReceiver(string ID, RemoteCommandReceiver receiver)
        {
            if (_disposedValue) return;

            _receivers.Add(ID, receiver);
        }

        public void RemoveReceiver(string ID)
        {
            if (_disposedValue) return;

            _receivers.Remove(ID);
        }

        private void Receiver(object? sender, RemoteCommandEventArgs e)
        {
            if (_disposedValue) return;

            if (_receivers.TryGetValue(e.Command.Id, out RemoteCommandReceiver? receiver))
            {
                AppDispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        receiver(e.Command);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                });
            }
            else
            {
                Debug.WriteLine($"RemoteCommand not found: {e.Command.Id}");
            }
        }

        public void Send(RemoteCommand command, RemoteCommandDelivery delivery)
        {
            if (_disposedValue) return;

            _ = SendAsync(command, delivery, CancellationToken.None);
        }

        public async ValueTask SendAsync(RemoteCommand command, RemoteCommandDelivery delivery, CancellationToken token)
        {
            if (_disposedValue) return;

            try
            {
                await _client.SendAsync(command, delivery, token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                // TODO: ここで例外を握りつぶすのはどうなんだろう？
                Debug.WriteLine(ex.Message);
            }
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _server.Dispose();
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
