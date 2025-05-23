using System;
using System.Threading.Tasks;


namespace NeeView.Threading
{
    public static class TaskTools
    {
        /// <summary>
        /// リトライ処理の汎用化
        /// </summary>
        /// <param name="retryCount">リトライ回数(2以上)</param>
        /// <param name="millisecondsInterval">リトライインターバル(ms)</param>
        /// <param name="action">処理本体</param>
        public static async ValueTask RetryActionAsync(int retryCount, int millisecondsInterval, Action action)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            if (retryCount < 2) throw new ArgumentException("retryCount must be 2 or more");
            if (millisecondsInterval < 0) throw new ArgumentException("millisecondsInterval must be 0 or more");

            while (retryCount > 0)
            {
                try
                {
                    action.Invoke();
                    return;
                }
                catch
                {
                    if (retryCount > 1)
                    {
                        await Task.Delay(millisecondsInterval);
                        retryCount--;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }

}
