using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.Threading
{
    public static class AsyncWrapper
    {
        private static readonly SemaphoreSlim _semaphore = new(3, 3);

        /// <summary>
        /// 同期メソッドをAsync実行する
        /// </summary>
        /// <remarks>
        /// 処理はバックグラウンドで実行され、キャンセルされても止まらないことに注意してください。
        /// </remarks>
        /// <param name="syncFunction"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        public static async Task ToAsync(Action syncFunction, CancellationToken token)
        {
            await _semaphore.WaitAsync(token);

            using var internalCts = CancellationTokenSource.CreateLinkedTokenSource(token);

            var backgroundTask = Task.Run(() =>
            {
                try { syncFunction(); }
                finally { _semaphore.Release(); }
            });

            var cancelTask = Task.Delay(Timeout.Infinite, internalCts.Token);

            var completedTask = await Task.WhenAny(backgroundTask, cancelTask);

            if (completedTask == cancelTask)
            {
                throw new OperationCanceledException(token);
            }

            internalCts.Cancel();

            await backgroundTask;
        }

        public static async Task<T> ToAsync<T>(Func<T> syncFunction, CancellationToken token)
        {
            await _semaphore.WaitAsync(token);

            using var internalCts = CancellationTokenSource.CreateLinkedTokenSource(token);

            Task<T> backgroundTask = Task.Run(() =>
            {
                try { return syncFunction(); }
                finally { _semaphore.Release(); }
            });

            var cancelTask = Task.Delay(Timeout.Infinite, internalCts.Token);

            var completedTask = await Task.WhenAny(backgroundTask, cancelTask);

            if (completedTask == cancelTask)
            {
                throw new OperationCanceledException(token);
            }

            internalCts.Cancel();

            return await backgroundTask;
        }
    }
}
