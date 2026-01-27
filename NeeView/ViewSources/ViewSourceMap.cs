using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace NeeView
{
    public class ViewSourceMap : INotifyCollectionChanged, IDisposable
    {
        private readonly Dictionary<ViewSourceKey, ViewSource> _map = new();
        private readonly System.Threading.Lock _lock = new();
        private readonly BookMemoryService _bookMemoryService;
        private bool _disposedValue;


        public ViewSourceMap(BookMemoryService bookMemoryService)
        {
            _bookMemoryService = bookMemoryService;
        }


        public event NotifyCollectionChangedEventHandler? CollectionChanged;


        public bool TryGet(Page page, PagePart pagePart, out ViewSource? viewSource)
        {
            if (_disposedValue)
            {
                viewSource = null;
                return false;
            }

            lock (_lock)
            {
                var key = new ViewSourceKey(page, pagePart);
                return _map.TryGetValue(key, out viewSource);
            }
        }

        public ViewSource Get(Page page, PagePart pagePart, PageDataSource pageDataSource)
        {
            if (_disposedValue)
            {
                // TODO: 理想はダミーを返す
                return new ViewSource(page.Content, pageDataSource, _bookMemoryService);
            }

            lock (_lock)
            {
                var key = new ViewSourceKey(page, pagePart);
                if (_map.TryGetValue(key, out var value))
                {
                    return value;
                }
                else
                {
                    //Debug.WriteLine($"ViewSourceMap.Create: {key}");
                    var viewSource = new ViewSource(page.Content, pageDataSource, _bookMemoryService);
                    _map.Add(key, viewSource);
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, key));
                    return viewSource;
                }
            }
        }

        public void Clear()
        {
            if (_disposedValue) return;

            lock (_lock)
            {
                _map.Clear();
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    lock (_lock)
                    {
                        _map.Clear();
                    }
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


    public record struct ViewSourceKey(Page Page, PagePart PagePart);
}
