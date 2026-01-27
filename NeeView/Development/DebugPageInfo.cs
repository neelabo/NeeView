using NeeLaboratory.Generators;
using System;
using System.ComponentModel;
using System.Linq;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class DebugPageInfo : INotifyPropertyChanged, IDisposable
    {
        private readonly ViewSourceMap _viewSourceMap;
        private ViewSource? _viewSourceAll;
        private ViewSource? _viewSourceLeft;
        private ViewSource? _viewSourceRight;
        private bool _disposedValue;

        public DebugPageInfo(Page page, ViewSourceMap viewSourceMap)
        {
            Page = page;
            _viewSourceMap = viewSourceMap;
            _viewSourceMap.CollectionChanged += ViewSourceMap_CollectionChanged;

            UpdateViewSource(viewSourceMap);
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        public Page Page { get; }

        public string Name => Page.EntryLastName;

        public PageContent PageContent => Page.Content;


        public ViewSource? ViewSourceAll
        {
            get { return _viewSourceAll; }
            set { SetProperty(ref _viewSourceAll, value); }
        }

        public ViewSource? ViewSourceLeft
        {
            get { return _viewSourceLeft; }
            set { SetProperty(ref _viewSourceLeft, value); }
        }

        public ViewSource? ViewSourceRight
        {
            get { return _viewSourceRight; }
            set { SetProperty(ref _viewSourceRight, value); }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _viewSourceMap.CollectionChanged -= ViewSourceMap_CollectionChanged;
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void ViewSourceMap_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_disposedValue) return;

            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    if (e.NewItems is not null && e.NewItems.Cast<ViewSourceKey>().Any(e => e.Page == Page))
                    {
                        UpdateViewSource(_viewSourceMap);
                    }
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    UpdateViewSource(_viewSourceMap);
                    break;

                default:
                    throw new NotSupportedException();
            }
        }


        private void UpdateViewSource(ViewSourceMap viewSourceMap)
        {
            if (_viewSourceMap.TryGet(Page, PagePart.All, out var viewSourceAll))
            {
                ViewSourceAll = viewSourceAll;
            }
            if (_viewSourceMap.TryGet(Page, PagePart.Left, out var viewSourceLeft))
            {
                ViewSourceLeft = viewSourceLeft;
            }
            if (_viewSourceMap.TryGet(Page, PagePart.Right, out var viewSourceRight))
            {
                ViewSourceRight = viewSourceRight;
            }
        }

    }
}
