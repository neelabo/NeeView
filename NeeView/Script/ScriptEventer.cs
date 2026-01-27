using System;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// ブック関連のスクリプトイベント発行用
    /// </summary>
    public class ScriptEventer : IDisposable
    {
        private readonly PageFrameBoxPresenter _presenter;
        private bool _disposedValue;


        public ScriptEventer()
        {
            _presenter = PageFrameBoxPresenter.Current;
            _presenter.PageFrameBoxChanged += Presenter_PageFrameBoxChanged;
            _presenter.ViewPageChanged += Presenter_ViewPageChanged;
            _presenter.PageTerminated += Presenter_PageTerminated;
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _presenter.PageFrameBoxChanged += Presenter_PageFrameBoxChanged;
                    _presenter.ViewPageChanged += Presenter_ViewPageChanged;
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Presenter_PageFrameBoxChanged(object? sender, PageFrameBoxChangedEventArgs e)
        {
            NVDebug.AssertSTA();
            if (e.Box is null) return;

            if (e.Box.BookContext.Book.LoadOption.HasFlag(BookLoadOption.Rename) && !Config.Current.Script.OnBookLoadedWhenRenamed)
            {
                return;
            }

            // Script: OnBookLoaded
            CommandTable.Current.TryExecute(this, ScriptCommand.EventOnBookLoaded, null, CommandOption.None);
        }

        private void Presenter_ViewPageChanged(object? sender, ViewPageChangedEventArgs e)
        {
            NVDebug.AssertSTA();
            if (!e.Pages.Any()) return;

            // Script: OnPageChanged
            CommandTable.Current.TryExecute(this, ScriptCommand.EventOnPageChanged, null, CommandOption.None);
        }

        private void Presenter_PageTerminated(object? sender, PageTerminatedEventArgs e)
        {
            NVDebug.AssertSTA();

            // Script: OnPageEnd
            var args = new object[] { e.Direction };
            CommandTable.Current.TryExecute(this, ScriptCommand.EventOnPageEnd, args, CommandOption.None);
        }
    }
}
