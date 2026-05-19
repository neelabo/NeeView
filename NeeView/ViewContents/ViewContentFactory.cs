using NeeView.PageFrames;
using System;

namespace NeeView
{
    public class ViewContentFactory : IDisposable
    {
        private readonly PageFrameContext _context;
        private readonly ViewSourceMap _viewSourceMap;
        private readonly PageBackgroundSource _backgroundSource;
        private bool _disposedValue;

        public ViewContentFactory(PageFrameContext context, ViewSourceMap viewSourceMap)
        {
            _context = context;
            _viewSourceMap = viewSourceMap;
            _backgroundSource = new PageBackgroundSource();
        }

        public ViewContent Create(PageFrameElement element, PageFrameElementScale scale, PageFrameActivity activity, int index)
        {
            var viewSource = _viewSourceMap.Get(element.Page, element.PagePart, element.PageDataSource);
            return new ViewContent(_context, element, scale, viewSource, activity, _backgroundSource, index);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _backgroundSource.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
