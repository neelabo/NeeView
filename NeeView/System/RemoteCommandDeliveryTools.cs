using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace NeeView
{
    public static class RemoteCommandDeliveryTools
    {
        public static async Task<Process?> GetProcessAsync(RemoteCommandDelivery delivery, bool hasWindow)
        {
            if (delivery.Type == RemoteCommandDeliveryType.Custom)
            {
                Debug.Assert(delivery.Process is not null);
                return delivery.Process;
            }

            return await Task.Run(() => GetProcess(delivery, hasWindow));
        }

        public static Process? GetProcess(RemoteCommandDelivery delivery, bool hasWindow)
        {
            var currentProcess = Process.GetCurrentProcess();

            // collect NeeView processes
            var processes = Process.GetProcesses().Where(e => e.ProcessName == currentProcess.ProcessName).ToList();

            // 自身を基準として並び替え。自身は削除する
            var index = processes.FindIndex(e => e.Id == currentProcess.Id);
            var items = processes.Skip(index).Concat(processes.Take(index)).Where(e => e.Id != currentProcess.Id);

            // 設定により、実行ファイルパスが一致したものに限定
            items = items.Where(p => !AppSettings.Current.PathProcessGroup || p.MainModule?.FileName == currentProcess.MainModule?.FileName);

            if (hasWindow)
            {
                // ウィンドウハンドルが存在しないものは除外
                items = items.Where(p => p.MainWindowHandle != IntPtr.Zero);
            }

            items = delivery.Type switch
            {
                RemoteCommandDeliveryType.Custom => items.Where(p => p == delivery.Process),
                RemoteCommandDeliveryType.Latest => items.OrderByDescending((p) => p.StartTime),
                RemoteCommandDeliveryType.Previous => items.Reverse(),
                RemoteCommandDeliveryType.Next => items,
                _ => items
            };

            return items.FirstOrDefault();
        }
    }
}
