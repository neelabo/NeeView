using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NeeView
{
    public class BookControlProxy : BindableBase, IBookControl, IDisposable
    {
        private IBookControl? _source;
        private bool _disposedValue;



        public bool IsBookmark => _source?.IsBookmark ?? false;

        public bool IsBusy => _source?.IsBusy ?? false;

        public PageSortModeClass PageSortModeClass => _source?.PageSortModeClass ?? PageSortModeClass.Full;

        public string? Path => _source?.Path;


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Detach();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void SetSource(IBookControl? source)
        {
            if (_source == source) return;

            Detach();
            Attach(source);

            RaisePropertyChanged(nameof(IsBookmark));
            RaisePropertyChanged(nameof(IsBusy));
            RaisePropertyChanged(nameof(PageSortModeClass));
        }

        private void Attach(IBookControl? source)
        {
            if (_source == source) return;
            _source = source;

            if (_source is not null)
            {
                _source.PropertyChanged += Source_PropertyChanged;
            }
        }

        private void Detach()
        {
            if (_source is null) return;

            _source.PropertyChanged -= Source_PropertyChanged;
            _source = null;
        }

        private void Source_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //Debug.WriteLine($"{e.PropertyName}: IsBusy={_source?.IsBusy}");
            RaisePropertyChanged(e.PropertyName);
        }

        public bool CanDeleteBook()
        {
            return _source?.CanDeleteBook() ?? false;
        }

        public void DeleteBook()
        {
            _source?.DeleteBook();
        }

        public void ReLoad()
        {
            _source?.ReLoad();
        }

        public bool CanBookmark()
        {
            return _source?.CanBookmark() ?? false;
        }

        public void SetBookmark(bool isBookmark, string? parent = null)
        {
            _source?.SetBookmark(isBookmark, parent);
        }

        public void ToggleBookmark(string? parent = null)
        {
            _source?.ToggleBookmark(parent);
        }

        public bool IsBookmarkOn(string? parent)
        {
            return _source?.IsBookmarkOn(parent) ?? false;
        }

    }




}
