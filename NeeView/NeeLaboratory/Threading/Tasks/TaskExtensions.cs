using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NeeLaboratory.Threading.Tasks
{
    public static class TaskExtensions
    {
        /// <summary>
        /// タスクを投げっぱなしにするための拡張メソッド
        /// </summary>
        public static void FireAndForget(this Task task, Action<AggregateException>? errorHandler = null)
        {
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    errorHandler ??= DefaultErrorHandler;
                    errorHandler(t.Exception);
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        private static void DefaultErrorHandler(AggregateException ex)
        {
            ex.Flatten().Handle(x =>
            {
                if (x is OperationCanceledException)
                {
                    return true;
                }
                else
                {
                    Trace.WriteLine($"Task error: {x}");
                    return true;
                }
            });
        }
    }
}
