using NeeLaboratory.ComponentModel;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace NeeView
{
    public class BookPlaylistControlProxy : BindableBase, IBookPlaylistControl, IDisposable
    {
        private BookPlaylistControl? _source;
        private bool _disposedValue;

        public BookPlaylistControlProxy()
        {
        }



        public event EventHandler? MarkersChanged;

        public bool IsMarked => _source?.IsMarked ?? false;



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


        public void SetSource(BookPlaylistControl? source)
        {
            if (_source == source) return;

            Detach();
            Attach(source);

            UpdateMarkers();
            RaisePropertyChanged(nameof(IsMarked));
        }

        private void Attach(BookPlaylistControl? source)
        {
            Debug.Assert(_source is null);
            if (source is null) return;

            _source = source;
            _source.PropertyChanged += Source_PropertyChanged;
            _source.MarkersChanged += Source_MarkersChanged;
        }

        private void Detach()
        {
            if (_source is null) return;

            _source.PropertyChanged -= Source_PropertyChanged;
            _source.MarkersChanged -= Source_MarkersChanged;
            _source.Dispose();
            _source = null;
        }


        private void Source_MarkersChanged(object? sender, EventArgs e)
        {
            MarkersChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Source_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            RelayPropertyChanged(e, nameof(_source.IsMarked), nameof(IsMarked));
        }

        private void RelayPropertyChanged(PropertyChangedEventArgs e, string srcName, string dstName)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == srcName)
            {
                RaisePropertyChanged(dstName);
            }
        }

        // TODO: 外部からイベント起動はよろしくない
        //public void UpdateMarkCondition()
        //{
        //    RaisePropertyChanged(nameof(IsMarked));
        //}


        public bool CanMark()
        {
            return _source?.CanMark() ?? false;
        }

        public bool CanMark(Page page)
        {
            return _source?.CanMark(page) ?? false;
        }

        public bool CanNextMarkInPlace(MovePlaylistItemInBookCommandParameter param)
        {
            return _source?.CanNextMarkInPlace(param) ?? false;
        }

        public bool CanPrevMarkInPlace(MovePlaylistItemInBookCommandParameter param)
        {
            return _source?.CanPrevMarkInPlace(param) ?? false;
        }

        public void NextMarkInPlace(object? sender, MovePlaylistItemInBookCommandParameter param)
        {
            _source?.NextMarkInPlace(sender, param);
        }

        public void PrevMarkInPlace(object? sender, MovePlaylistItemInBookCommandParameter param)
        {
            _source?.PrevMarkInPlace(sender, param);
        }

        public PlaylistItem? SetMark(bool isMark)
        {
            return _source?.SetMark(isMark);
        }

        public PlaylistItem? ToggleMark()
        {
            return _source?.ToggleMark();
        }

        public void UpdateMarkers()
        {
            _source?.UpdateMarkers();
        }

    }

}
