using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView.Threading
{
    public class RetryAction
    {
        public static async ValueTask RetryActionAsync(Action saveAction, int retryLimit, int intervalMilliseconds, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            int retryCount = 0;
            while (true)
            {
                try
                {
                    saveAction.Invoke();
                    return;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch
                {
                    if (retryCount >= retryLimit) throw;
                    await Task.Delay(intervalMilliseconds, token);
                    retryCount++;
                }
            }
        }
    }
}
