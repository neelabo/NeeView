using NeeLaboratory;
using NeeLaboratory.Runtime.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView.Susie.Client
{
    public class SusiePluginClient : IRemoteSusiePlugin, IDisposable
    {
        private readonly SusiePluginRemoteClient _remote;
        private Action? _recoveryAction;
        private bool _isRecoveryDoing;
        private bool _disposedValue;


        public SusiePluginClient()
        {
            _remote = new SusiePluginRemoteClient();
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _remote.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void SetRecoveryAction(Action action)
        {
            _recoveryAction = action;
        }

        private void Recovery()
        {
            if (_isRecoveryDoing) return;

            _isRecoveryDoing = true;
            try
            {
                Debug.WriteLine($"SusiePluginClient: Recovery...");
                _remote.Disconnect();
                _remote.Connect();
                _recoveryAction?.Invoke();
            }
            finally
            {
                _isRecoveryDoing = false;
            }
        }


        private List<Chunk> Call<T>(int id, T arg)
        {
            var chunk = new Chunk(id, SusieCommandSerializer.Serialize(arg));
            return Call(new List<Chunk>() { chunk });
        }

        private List<Chunk> Call<T>(int id, T arg1, byte[]? arg2)
        {
            var chunk1 = new Chunk(id, SusieCommandSerializer.Serialize(arg1));
            var chunk2 = new Chunk(id, arg2);
            return Call(new List<Chunk>() { chunk1, chunk2 });
        }

        private List<Chunk> Call(List<Chunk> args)
        {
            try
            {
                var task = Task.Run(async () => await CallAsync(args, CancellationToken.None));
                return task.Result;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException != null && ex.InnerExceptions.Count == 1)
                {
                    throw ex.InnerException;
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task<List<Chunk>> CallAsync(List<Chunk> args, CancellationToken token)
        {
            //Debug.WriteLine($"SusiePluginClient.Call: {args[0].Id}");

            if (!_remote.IsConnected)
            {
                Recovery();
            }

            Exception? exception = null;

            // 5 retries
            for (int retry = 0; retry < 5; retry++)
            {
                try
                {
                    return await _remote.CallAsync(args, token);
                }
                catch (TimeoutException ex)
                {
                    exception = ex;
                    Debug.WriteLine($"SusiePluginClient.CallAsync: {ex.Message})");
                    Recovery();
                }
            }

            throw exception ?? new InvalidOperationException("Exception is null");
        }

        private static TResult DeserializeChunk<TResult>(Chunk chunk)
            where TResult : class
        {
            if (chunk.Data is null) throw new InvalidOperationException("chunk.Data must not be null");

            //Debug.WriteLine($"Chunk.ID: {chunk.Id}");
            //Debug.WriteLine($"Chunk.Data: " + System.Text.Encoding.UTF8.GetString(chunk.Data));
            return SusieCommandSerializer.Deserialize<TResult>(chunk.Data);
        }



        public void Initialize(string pluginFolder, List<SusiePluginInfo> plugins)
        {
            _remote.Connect();
            Call(SusiePluginCommandId.Initialize, new SusiePluginCommandInitialize(pluginFolder, plugins));
        }

        public List<SusiePluginInfo> GetPlugin(List<string>? pluginNames)
        {
            var chunks = Call(SusiePluginCommandId.GetPlugin, new SusiePluginCommandGetPlugin(pluginNames));
            return DeserializeChunk<SusiePluginCommandGetPluginResult>(chunks[0]).Plugins;
        }

        public void SetPlugin(List<SusiePluginInfo> plugins)
        {
            Call(SusiePluginCommandId.SetPlugin, new SusiePluginCommandSetPlugin(plugins));
        }

        public void SetPluginOrder(List<string> order)
        {
            Call(SusiePluginCommandId.SetPluginOrder, new SusiePluginCommandSetPluginOrder(order));
        }

        public void ShowConfigurationDlg(string pluginName, int hwnd)
        {
            Call(SusiePluginCommandId.ShowConfigurationDlg, new SusiePluginCommandShowConfigurationDlg(pluginName, hwnd));
        }

        public SusiePluginInfo? GetArchivePlugin(string fileName, byte[]? buff, bool isCheckExtension)
        {
            var chunks = Call(SusiePluginCommandId.GetArchivePlugin, new SusiePluginCommandGetArchivePlugin(fileName, isCheckExtension), buff);
            return DeserializeChunk<SusiePluginCommandGetArchivePluginResult>(chunks[0]).Plugin;
        }

        public SusiePluginInfo? GetImagePlugin(string fileName, byte[] buff, bool isCheckExtension)
        {
            var chunks = Call(SusiePluginCommandId.GetImage, new SusiePluginCommandGetImagePlugin(fileName, isCheckExtension), buff);
            return DeserializeChunk<SusiePluginCommandGetImagePluginResult>(chunks[0]).Plugin;
        }

        public SusieImage? GetImage(string? pluginName, string fileName, byte[]? buff, bool isCheckExtension)
        {
            var chunks = Call(SusiePluginCommandId.GetImage, new SusiePluginCommandGetImage(pluginName, fileName, isCheckExtension), buff);
            if (chunks[1].Data is null) return null;

            return new SusieImage(DeserializeChunk<SusiePluginCommandGetImageResult>(chunks[0]).Plugin, chunks[1].Data);
        }

        public List<SusieArchiveEntry> GetArchiveEntries(string pluginName, string fileName)
        {
            var chunks = Call(SusiePluginCommandId.GetArchiveEntries, new SusiePluginCommandGetArchiveEntries(pluginName, fileName));
            return DeserializeChunk<SusiePluginCommandGetArchiveEntriesResult>(chunks[0]).Entries;
        }

        public byte[] ExtractArchiveEntry(string pluginName, string fileName, int position)
        {
            var chunks = Call(SusiePluginCommandId.ExtractArchiveEntry, new SusiePluginCommandExtractArchiveEntry(pluginName, fileName, position));
            return chunks[0].Data ?? throw new SusieException("Cannot get ArchiveEntry");
        }

        public void ExtractArchiveEntryToFolder(string pluginName, string fileName, int position, string extractFolder)
        {
            Call(SusiePluginCommandId.ExtractArchiveEntryToFolder, new SusiePluginCommandExtractArchiveEntryToFolder(pluginName, fileName, position, extractFolder));
        }
    }
}
