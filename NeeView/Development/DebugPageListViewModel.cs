using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class DebugPageListViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly PageFrameBoxPresenter _presenter;

        private ObservableCollection<DebugPageInfo> _items = new();
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();

        public DebugPageListViewModel()
        {
            _presenter = PageFrameBoxPresenter.Current;
            _disposables.Add(_presenter.SubscribePageFrameBoxChanged(Presenter_PageFrameBoxChanged));
            _disposables.Add(_presenter.SubscribePagesChanged(Presenter_PagesChanged));

            UpdateItems();
        }



        public event PropertyChangedEventHandler? PropertyChanged;


        public Book? Book => _presenter.Book;

        public ObservableCollection<DebugPageInfo> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();
                    RemoveItems();
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
            RaisePropertyChanged(nameof(Book));
        }

        private void Presenter_PagesChanged(object? sender, EventArgs e)
        {
            UpdateItems();
        }

        private void UpdateItems()
        {
            RemoveItems();

            var box = _presenter.View;
            if (box is null) return;

            var viewSourceMap = box.ViewSourceMap;
            Items = new ObservableCollection<DebugPageInfo>(_presenter.Pages.Select(e => new DebugPageInfo(e, viewSourceMap)));
        }

        private void RemoveItems()
        {
            foreach (var item in _items)
            {
                item.Dispose();
            }
            Items = new();
        }

        internal void Clear()
        {
            Book?.Source.BookMemoryService.CleanupDeep();
        }
    }
}
