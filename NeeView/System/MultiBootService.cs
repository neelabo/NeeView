using NeeLaboratory.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class MultiBootService
    {
        private readonly Process _currentProcess;
        private readonly Process? _serverProcess;


        public MultiBootService()
        {
            _currentProcess = Process.GetCurrentProcess();
            _serverProcess = GetServerProcess(_currentProcess);

            RemoteCommandService.Current.AddReceiver("LoadAs", LoadAs);
        }

        /// <summary>
        /// サーバの存在チェック
        /// </summary>
        public bool IsServerExists => _serverProcess != null;


        /// <summary>
        /// サーバープロセスを検索
        /// </summary>
        private static Process? GetServerProcess(Process currentProcess)
        {
            var processName = currentProcess.ProcessName;
            Trace.WriteLine($"GetServerProcess: CurrentProcess: ProcessName={processName}, Id={currentProcess.Id}");

            for (int retry = 0; retry < 2; ++retry)
            {
                var processes = Process.GetProcessesByName(processName)
                    .ToList();

                foreach (var p in processes)
                {
                    Trace.WriteLine($"GetServerProcess: FindProcess: ProcessName={p.ProcessName}, ProcessFileName={p.MainModule?.FileName}, Id={p.Id}");
                }

                try
                {
                    // 自身以外のプロセスをターゲットにする
                    var serverProcess = processes
                        // ウィンドウハンドルが存在しないものは除外
                        .Where(p => p.MainWindowHandle != IntPtr.Zero)
                        // 設定により、実行ファイルパスが一致したものに限定
                        .Where(p => !AppSettings.Current.PathProcessGroup || p.MainModule?.FileName == currentProcess.MainModule?.FileName)
                        // 自身以外の最も若いプロセス
                        .LastOrDefault((p) => p.Id != currentProcess.Id);

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
        /// サーバーにパスを送る
        /// </summary>
        public async Task RemoteLoadAsAsync(List<string> files)
        {
            if (_serverProcess is null) return;

            try
            {
                ProcessActivator.AppActivate(_serverProcess);
                await RemoteCommandService.Current.SendAsync(new RemoteCommand("LoadAs", files.ToArray()), new RemoteCommandDelivery(_serverProcess.Id), CancellationToken.None);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// リモートコマンド(LoadAs)
        /// </summary>
        private void LoadAs(RemoteCommand command)
        {
            try
            {
                // パスの指定があれば開く
                if (command.Args != null && command.Args.Length > 0 && command.Args[0] != null)
                {
                    BookHubTools.RequestLoad(this, command.Args, BookLoadOption.FocusOnLoaded, true);
                }
            }
            catch { }
        }
    }
}
