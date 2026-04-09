using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// UIスレッドのロック監視。ロックが1秒以上続くとスタックトレースを出力する。
    /// </summary>
    public class SystemLockMonitor : IDisposable
    {
        private const int _thresholdMs = 1000;
        private readonly Stopwatch? _stopwatch;

        public SystemLockMonitor()
        {
            if (!App.Current.IsTraceLogEnabled) return;

            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            if (_stopwatch is null) return;

            if (!App.Current.IsTraceLogEnabled) return;
           
            _stopwatch.Stop();

            var elapsed = _stopwatch.ElapsedMilliseconds;

            if (elapsed > _thresholdMs)
            {
                bool isSTA = Thread.CurrentThread.GetApartmentState() == ApartmentState.STA;
                
                bool isUIThread = App.Current?.Dispatcher.CheckAccess() ?? false;

                if (isSTA && isUIThread)
                {
                    Trace.WriteLine($"[Warning] UI Thread Lock detected: {elapsed}ms");
                    Trace.WriteLine(System.Environment.StackTrace);
                }
            }
        }
    }
}
