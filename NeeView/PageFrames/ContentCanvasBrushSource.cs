using NeeLaboratory.ComponentModel;
using NeeView.Windows;
using System;
using System.Windows;
using System.Windows.Media;


namespace NeeView.PageFrames
{
    public class ContentCanvasBrushSource : IContentCanvasBrushSource, IDisposable
    {
        private Page? _page;
        private readonly DpiScaleProvider _dpiScaleProvider;
        private bool _disposedValue;
        private IDisposable? _disposable;
        private readonly DisposableCollection _disposables = new();

        public ContentCanvasBrushSource(DpiScaleProvider dpiScaleProvider)
            : this(dpiScaleProvider, null)
        {
        }

        public ContentCanvasBrushSource(DpiScaleProvider dpiScaleProvider, Page? page)
        {
            _dpiScaleProvider = dpiScaleProvider;
            _disposables.Add(_dpiScaleProvider.SubscribeDpiChanged((s, e) => DpiChanged?.Invoke(this, e)));

            SetPage(page);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposable?.Dispose();
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


        public event EventHandler? ContentChanged;
        public event EventHandler? DpiChanged;


        public void SetPage(Page? page)
        {
            if (_page == page) return;

            if (_page is not null)
            {
                _disposable?.Dispose();
                _disposable = null;
            }

            _page = page;

            if (_page is not null)
            {
                _disposable = _page.SubscribeContentChanged((s, e) => ContentChanged?.Invoke(this, e));
            }

            ContentChanged?.Invoke(this, EventArgs.Empty);
        }

        public DpiScale Dpi => _dpiScaleProvider.DpiScale.ToFixedScale();

        public Color GetContentColor() => _page?.Color ?? Colors.Black;
    }
}
