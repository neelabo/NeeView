using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// App.Current.Dispatcher.Invoke系のラッパー
    /// </summary>
    public static class AppDispatcher
    {
        public static Dispatcher UIDispatcher { get; } = Application.Current.Dispatcher;

        public static void Invoke(Action action)
        {
            if (UIDispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                UIDispatcher.Invoke(action);
            }
        }


        public static TResult Invoke<TResult>(Func<TResult> func)
        {
            if (UIDispatcher.CheckAccess())
            {
                return func.Invoke();
            }
            else
            {
                return UIDispatcher.Invoke(func);
            }
        }

        public static async ValueTask InvokeAsync(Action action)
        {
            if (UIDispatcher.CheckAccess())
            {
                action.Invoke();
            }
            else
            {
                await UIDispatcher.InvokeAsync(action);
            }
        }

        public static DispatcherOperation<TResult> InvokeAsync<TResult>(Func<TResult> callback)
        {
            return UIDispatcher.InvokeAsync(callback);
        }

        public static DispatcherOperation BeginInvoke(Action action)
        {
            return UIDispatcher.BeginInvoke(action);
        }

        public static EventHandler BeginInvokeHandler(EventHandler eventHandler)
        {
            return (s, e) => UIDispatcher.BeginInvoke(() => eventHandler(s, e));
        }

        public static EventHandler<T> BeginInvokeHandler<T>(EventHandler<T> eventHandler)
        {
            return (s, e) => UIDispatcher.BeginInvoke(() => eventHandler(s, e));
        }
    }
}
