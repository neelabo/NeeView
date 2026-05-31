using NeeLaboratory.ComponentModel;
using NeeView.PageFrames;
using NeeView.Properties;
using System;
using System.Threading;

namespace NeeView
{

    public class BookPageTerminator : IDisposable
    {
        private readonly PageFrameBox _box;
        private readonly Book _book;
        private int _pageTerminating;
        private readonly IBookPageControl _control;
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();


        public BookPageTerminator(PageFrameBox box, IBookPageControl control)
        {
            _box = box;
            _book = _box.Book;
            _control = control;

            _disposables.Add(_box.SubscribePageTerminated(
                (s, e) => AppDispatcher.Invoke(() => Box_PageTerminated(s, e))));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        // ページ終端を超えて移動しようとするときの処理
        private void Box_PageTerminated(object? sender, PageTerminatedEventArgs e)
        {
            if (_pageTerminating > 0) return;

            var pageEndAction = SlideShow.Current.IsPlaying
                ? Config.Current.SlideShow.PageEndAction
                : Config.Current.Book.PageEndAction;

            switch (pageEndAction)
            {
                case PageEndAction.NextBook:
                    PageEndAction_NextBook(sender, e);
                    break;

                case PageEndAction.Loop:
                    PageEndAction_Loop(sender, e);
                    break;

                case PageEndAction.SeamlessLoop:
                    // nop.
                    break;

                case PageEndAction.Dialog:
                    PageEndAction_Dialog(sender, e);
                    break;

                default:
                    PageEndAction_None(sender, e, true);
                    break;
            }
        }

        private void PageEndAction_Loop(object? sender, PageTerminatedEventArgs e)
        {
            if (e.Direction < 0)
            {
                _control.MoveToLast(sender);
            }
            else
            {
                _control.MoveToFirst(sender);
            }

            NotifyPageLoop();
        }

        public static void NotifyPageLoop()
        {
            if (Config.Current.Book.IsNotifyPageLoop)
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, TextResources.GetString("Notice.BookOperationPageLoop"));
            }
        }

        private void PageEndAction_NextBook(object? sender, PageTerminatedEventArgs e)
        {
            if (BookHub.Current.IsBookLocked) return;

            AppDispatcher.Invoke(async () =>
            {
                if (e.Direction < 0)
                {
                    await BookshelfFolderList.Current.PrevFolder(false, CreateNextBookLoadOption(BookLoadOption.LastPage));
                }
                else
                {
                    await BookshelfFolderList.Current.NextFolder(false, CreateNextBookLoadOption(BookLoadOption.FirstPage));
                }
            });
        }

        private BookLoadOption CreateNextBookLoadOption(BookLoadOption optionWhenContinue)
        {
            return Config.Current.Book.ResetNextBookPageMode switch
            {
                ResetNextBookPageMode.Reset
                    => BookLoadOption.FirstPage,
                ResetNextBookPageMode.Continue
                    => optionWhenContinue,
                _
                    => BookLoadOption.None,
            };
        }

        private void PageEndAction_None(object? sender, PageTerminatedEventArgs e, bool notify)
        {
            if (SlideShow.Current.IsPlaying)
            {
                // スライドショー解除
                SlideShow.Current.Stop();
                var message = CommandTable.Current.GetElement("ToggleSlideShow").GetStateExecuteMessage(false);
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, message);
            }

            // 通知。本の場合のみ処理。メディアでは不要
            else if (notify && this._book != null && !this._book.IsMedia)
            {
                if (e.Direction < 0)
                {
                    SoundPlayerService.Current.PlaySeCannotMove();
                    InfoMessage.Current.SetMessage(InfoMessageType.Notify, TextResources.GetString("Notice.FirstPage"));
                }
                else
                {
                    SoundPlayerService.Current.PlaySeCannotMove();
                    InfoMessage.Current.SetMessage(InfoMessageType.Notify, TextResources.GetString("Notice.LastPage"));
                }
            }
        }

        private void PageEndAction_Dialog(object? sender, PageTerminatedEventArgs e)
        {
            Interlocked.Increment(ref _pageTerminating);
            SlideShow.Current.Suspend();

            AppDispatcher.BeginInvoke(() =>
            {
                try
                {
                    PageEndAction_DialogCore(sender, e);
                }
                finally
                {
                    Interlocked.Decrement(ref _pageTerminating);
                    SlideShow.Current.Resume();
                }
            });
        }

        private void PageEndAction_DialogCore(object? sender, PageTerminatedEventArgs e)
        {
            var title = (e.Direction < 0) ? TextResources.GetString("Notice.FirstPage") : TextResources.GetString("Notice.LastPage");
            var dialog = new MessageDialog(title, TextResources.GetString("PageEndDialog.Message"));
            var nextCommand = new UICommand("PageEndAction.NextBook");
            var loopCommand = new UICommand("PageEndAction.Loop");
            var noneCommand = new UICommand("PageEndAction.None");
            dialog.Commands.Add(nextCommand);
            dialog.Commands.Add(loopCommand);
            dialog.Commands.Add(noneCommand);
            dialog.CloseWhenDeactivated = true;

            var result = dialog.ShowDialog(App.Current.MainWindow);

            if (result.Command == nextCommand)
            {
                PageEndAction_NextBook(sender, e);
            }
            else if (result.Command == loopCommand)
            {
                PageEndAction_Loop(sender, e);
            }
            else
            {
                PageEndAction_None(sender, e, false);
            }
        }

    }

}
