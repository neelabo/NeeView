using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// 処理の例外をダイアログ通知等で処理する
    /// </summary>
    public static class ExceptionHandling
    {
        public static async Task<bool> WithToastAsync(Func<CancellationToken, Task> task, string errorDialogCaption, CancellationToken token)
        {
            return await WithToastAsync(task, errorDialogCaption, null, token);
        }

        public static async Task<bool> WithToastAsync(Func<CancellationToken, Task> task, string errorDialogCaption, FrameworkElement? element, CancellationToken token)
        {
            try
            {
                element?.Cursor = Cursors.Wait;
                await task(token);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                ToastService.Current.Show(new Toast(ex.Message, errorDialogCaption, ToastIcon.Error));
                return false;
            }
            finally
            {
                element?.Cursor = null;
            }
        }

        public static async Task<bool> WithDialogAsync(Func<CancellationToken, Task> task, string errorDialogCaption, CancellationToken token)
        {
            return await WithDialogAsync(task, errorDialogCaption, null, token);
        }

        public static async Task<bool> WithDialogAsync(Func<CancellationToken, Task> task, string errorDialogCaption, FrameworkElement? element, CancellationToken token)
        {
            try
            {
                element?.Cursor = Cursors.Wait;
                await task(token);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                element?.Cursor = null;
                new MessageDialog(errorDialogCaption, ex.Message).ShowDialog();
                return false;
            }
            finally
            {
                element?.Cursor = null;
            }
        }
    }
}
