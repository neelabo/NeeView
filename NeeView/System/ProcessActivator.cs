using NeeView.Interop;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    public static class ProcessActivator
    {
        public static Process? NextActivate(int direction)
        {
            var currentProcess = Process.GetCurrentProcess();

            // collect NeeView processes
            var processes = Process.GetProcesses().Where(e => e.ProcessName == currentProcess.ProcessName).OrderBy(e => e.StartTime).ToList();

            // 自身を基準として並び替え。自身は削除する
            var index = processes.FindIndex(e => e.Id == currentProcess.Id);
            processes = processes.Skip(index).Concat(processes.Take(index)).Where(e => e.Id != currentProcess.Id).ToList();
            var process = (direction > 0) ? processes.FirstOrDefault() : processes.LastOrDefault();
            AppActivate(process);
            return process;
        }

        public static void AppActivate(Process? process)
        {
            if (process == null) return;

            var hWnd = process.MainWindowHandle;

            // アクティブにする
            NativeMethods.SetForegroundWindow(hWnd);

            // ウィンドウが最小化されている場合は元に戻す
            if (NativeMethods.IsIconic(hWnd))
            {
                NativeMethods.ShowWindowAsync(hWnd, (int)ShowWindowCommands.SW_RESTORE);
            }
        }
    }

}
