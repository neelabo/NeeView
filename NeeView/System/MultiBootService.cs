using NeeLaboratory.IO;
using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class MultiBootService
    {
        private Process? _serverProcess;


        /// <summary>
        /// サーバの存在チェック
        /// </summary>
        public bool IsServerExists => _serverProcess != null;


        /// <summary>
        /// サーバープロセス更新
        /// </summary>
        public async Task UpdateServerProcess()
        {
            var currentProcess = Process.GetCurrentProcess();
            _serverProcess = await GetServerProcess(currentProcess);
        }

        /// <summary>
        /// サーバープロセスを検索
        /// </summary>
        private static async Task<Process?> GetServerProcess(Process currentProcess)
        {
            var processName = currentProcess.ProcessName;
            Trace.WriteLine($"GetServerProcess: CurrentProcess: ProcessName={processName}, Id={currentProcess.Id}");

            for (int retry = 0; retry < 2; ++retry)
            {
                try
                {
                    var processes = Process.GetProcessesByName(processName)
                        .ToList();

#if DEBUG
                    foreach (var p in processes)
                    {
                        Trace.WriteLine($"GetServerProcess: FindProcess: ProcessName={p.ProcessName}, ProcessFileName={p.MainModule?.FileName}, Id={p.Id}");
                    }
#endif
                    // 自身以外のプロセスをターゲットにする
                    var appProcess = processes
                        // 自身以外のプロセス
                        .Where(p => p.Id != currentProcess.Id)
                        // 設定により、実行ファイルパスが一致したものに限定
                        .Where(p => !AppSettings.Current.PathProcessGroup || p.MainModule?.FileName == currentProcess.MainModule?.FileName)
                        .ToList();

                    var serverProcess = appProcess
                        // ウィンドウハンドルが存在するもの
                        .Where(p => p.MainWindowHandle != IntPtr.Zero)
                        // 最新のもの
                        .LastOrDefault();

                    if (serverProcess == null)
                    {
                        // タスクトレイプロセス選択
                        foreach (var p in appProcess)
                        {
                            var isHideWindowProcess = await AppRemoteCommandClient.Current.IsHideWindowAsync(new RemoteCommandDelivery(p));
                            if (isHideWindowProcess)
                            {
                                serverProcess = p;
                                break;
                            }
                        }
                    }

                    if (serverProcess == null)
                    {
                        Trace.WriteLine($"GetServerProcess: ServerProcess not found.");
                    }
                    else
                    {
                        Trace.WriteLine($"GetServerProcess: ServerProcess: ProcessName={serverProcess.ProcessName}, Id={serverProcess.Id}");
                    }

                    return serverProcess;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                    Thread.Sleep(500);
                }
            }

            Trace.WriteLine($"GetServerProcess: ServerProcess not found from exception.");
            return null;
        }


        /// <summary>
        /// サーバーに起動引数を送る
        /// </summary>
        public async Task RemoteRestartAsync(string[] args)
        {
            if (_serverProcess is null) return;

            try
            {
                ProcessActivator.AppActivate(_serverProcess);

                await AppRemoteCommandClient.Current.RestartAsync(new RemoteCommandDelivery(_serverProcess), args);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Debug.Assert(false, ex.Message);
            }
        }

        /// <summary>
        /// 複数プロセスのうちの何番目か
        /// </summary>
        /// <returns></returns>
        public static int GetProcessIndex()
        {
            var currentProcess = Process.GetCurrentProcess();

            var index = Process.GetProcessesByName(currentProcess.ProcessName)
                .Where(p => p.MainModule?.FileName == currentProcess.MainModule?.FileName)
                .OrderBy(p => p.StartTime)
                .FindIndex(p => p.Id == currentProcess.Id);

            return index;
        }

        /// <summary>
        /// アクティブなプロセスを習得
        /// </summary>
        /// <returns></returns>
        public static Process? GetLatestActiveProcess()
        {
            var currentProcess = Process.GetCurrentProcess();

            var appProcess = Process.GetProcessesByName(currentProcess.ProcessName)
                .Where(p => p.MainModule?.FileName == currentProcess.MainModule?.FileName)
                .Where(p => p.MainWindowHandle != IntPtr.Zero)
                .LastOrDefault();

            return appProcess;
        }

    }
}
