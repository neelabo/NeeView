using NeeLaboratory.ComponentModel;
using System;
using System.ComponentModel;

namespace NeeView
{
    public class ThumbnailListItemDetailToolTip : BindableBase, IToolTipService, IDisposable
    {
        static ThumbnailListItemDetailToolTip() => Current = new();
        public static ThumbnailListItemDetailToolTip Current { get; }

        private readonly FilmStripConfig _filmstrip;
        private bool _isToolTipEnabled = true;
        private bool _disposedValue;

        public ThumbnailListItemDetailToolTip()
        {
            _filmstrip = Config.Current.FilmStrip;
            _filmstrip.PropertyChanged += Filmstrip_PropertyChanged;
        }

        public bool IsEnabled
        {
            get { return _filmstrip.IsDetailPopupEnabled && _isToolTipEnabled; }
        }

        // for Rename
        bool IToolTipService.IsToolTipEnabled
        {
            get { return _isToolTipEnabled; }
            set
            {
                if (SetProperty(ref _isToolTipEnabled, value))
                {
                    RaisePropertyChanged(nameof(IsEnabled));
                }
            }
        }
        private void Filmstrip_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(FilmStripConfig.IsDetailPopupEnabled))
            {
                RaisePropertyChanged(nameof(IsEnabled));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _filmstrip.PropertyChanged -= Filmstrip_PropertyChanged;
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
