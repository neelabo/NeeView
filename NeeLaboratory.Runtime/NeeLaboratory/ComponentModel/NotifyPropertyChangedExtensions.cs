using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.ComponentModel
{
    public static class NotifyPropertyChangedExtensions
    {
        /// <summary>
        /// 特定のプロパティが条件を満たすまで待機します
        /// </summary>
        public static async ValueTask WaitPropertyAsync<T>(this T source, string propertyName, Func<T, bool> condition, CancellationToken cancellationToken = default)
            where T : INotifyPropertyChanged
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (condition(source)) return;

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            PropertyChangedEventHandler handler = (sender, e) =>
            {
                if (e.PropertyName == propertyName && condition(source))
                {
                    tcs.TrySetResult(true);
                }
            };

            using (cancellationToken.Register(() => tcs.TrySetCanceled()))
            {
                try
                {
                    source.PropertyChanged += handler;
                    await tcs.Task;
                }
                finally
                {
                    source.PropertyChanged -= handler;
                }
            }
        }
    }
}
