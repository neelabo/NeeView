using NeeLaboratory.ComponentModel;
using NeeView.Threading;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public class ArchiveViewContentStrategy : IDisposable, IViewContentStrategy
    {
        private readonly ViewContent _viewContent;
        private ArchivePageControl? _pageControl;
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();

        public ArchiveViewContentStrategy(ViewContent viewContent)
        {
            _viewContent = viewContent;
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

        public void OnSourceChanged()
        {
            if (_disposedValue) return;

            _viewContent.RequestLoadViewSource(CancellationToken.None);
        }

        public FrameworkElement CreateLoadedContent(object data)
        {
            if (_disposedValue) throw new ObjectDisposedException(this.GetType().FullName);

            if (_pageControl is not null)
            {
                return _pageControl;
            }

            _pageControl = new ArchivePageControl((ArchiveViewData)data);
            return _pageControl;
        }
    }
}
