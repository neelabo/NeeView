using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.IO
{
    /// <summary>
    /// パイプを使って他のプロセスにコマンドを送る
    /// </summary>
    public class RemoteCommandClient
    {
        private readonly string _processName;


        public RemoteCommandClient(string processName)
        {
            _processName = processName;
        }

        public async Task SendAsync(RemoteCommand command, RemoteCommandDelivery delivery, CancellationToken token)
        {
            var processes = await CollectProcess(delivery);
            foreach (var process in processes)
            {
                token.ThrowIfCancellationRequested();
                var pipeName = RemoteCommandServer.GetPipetName(process);
                try
                {
                    await SendAsync(pipeName, command, 1000);
                }
                catch (TimeoutException ex)
                {
                    Debug.WriteLine($"RemoteClient.SendAsync: {pipeName}: {command.Id}: {ex.Message}");
                }
            }
        }

        private async Task<List<Process>> CollectProcess(RemoteCommandDelivery delivery)
        {
            return await Task.Run(() =>
            {
                var currentProcess = Process.GetCurrentProcess();

                // collect NeeView processes
                var processes = Process.GetProcesses().Where(e => e.ProcessName == _processName).ToList();

                // 自身を基準として並び替え。自身は削除する
                var index = processes.FindIndex(e => e.Id == currentProcess.Id);
                processes = processes.Skip(index).Concat(processes.Take(index)).Where(e => e.Id != currentProcess.Id).ToList();

                return delivery.Type switch
                {
                    RemoteCommandDeliveryType.Custom => processes.Where(p => p.Id == delivery.ProcessId).Take(1).ToList(),
                    RemoteCommandDeliveryType.Latest => processes.OrderByDescending((p) => p.StartTime).Take(1).ToList(),
                    RemoteCommandDeliveryType.Previous => ((IEnumerable<Process>)processes).Reverse().Take(1).ToList(),
                    RemoteCommandDeliveryType.Next => processes.Take(1).ToList(),
                    _ => processes.ToList(),
                };
            });
        }

        private static async Task SendAsync(string pipeName, RemoteCommand command, int timeout)
        {
            using var tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(timeout);

            try
            {
                using var pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
                await pipeClient.ConnectAsync(tokenSource.Token);
                if (pipeClient.IsConnected)
                {
                    await JsonSerializer.SerializeAsync(pipeClient, command, RemoteCommandJsonSerializerContext.Default.RemoteCommand, tokenSource.Token);
                }
            }
            catch (OperationCanceledException ex)
            {
                throw new TimeoutException("Timeout send RemoteCommand.", ex);
            }
        }
    }


    public class RemoteCommandDelivery
    {
        public static RemoteCommandDelivery All { get; } = new RemoteCommandDelivery(RemoteCommandDeliveryType.All);
        public static RemoteCommandDelivery Latest { get; } = new RemoteCommandDelivery(RemoteCommandDeliveryType.Latest);
        public static RemoteCommandDelivery Previous { get; } = new RemoteCommandDelivery(RemoteCommandDeliveryType.Previous);
        public static RemoteCommandDelivery Next { get; } = new RemoteCommandDelivery(RemoteCommandDeliveryType.Next);


        public RemoteCommandDelivery(RemoteCommandDeliveryType type)
        {
            Type = type;
        }

        public RemoteCommandDelivery(int processId)
        {
            Type = RemoteCommandDeliveryType.Custom;
            ProcessId = processId;
        }

        public RemoteCommandDeliveryType Type { get; private set; }
        public int ProcessId { get; private set; }
    }

    /// <summary>
    /// 配信先ターゲット
    /// </summary>
    public enum RemoteCommandDeliveryType
    {
        /// <summary>
        /// 自身を除く全プロセス
        /// </summary>
        All,

        /// <summary>
        /// 自身を除く最新プロセス
        /// </summary>
        Latest,

        /// <summary>
        /// 指定のプロセス
        /// </summary>
        Custom,

        /// <summary>
        /// 前のプロセス
        /// </summary>
        Previous,

        /// <summary>
        /// 次のプロセス
        /// </summary>
        Next,
    }

}
