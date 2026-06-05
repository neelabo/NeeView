using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// FilmStrip : ViewModel
    /// </summary>
    public class FilmStripViewModel : ObservableObject
    {
        private FilmStrip _model;


        public FilmStripViewModel(FilmStrip model, FilmStripItemDetailToolTip detailToolTip)
        {
            _model = model ?? throw new InvalidOperationException();

            _model.CollectionChanging +=
                (s, e) => CollectionChanging?.Invoke(s, e);

            _model.CollectionChanged +=
                (s, e) => CollectionChanged?.Invoke(s, e);

            _model.ViewItemsChanged +=
                (s, e) => AppDispatcher.Invoke(() => ViewItemsChanged?.Invoke(s, e));

            DetailToolTip = detailToolTip;
        }


        public event EventHandler? CollectionChanging;
        public event EventHandler? CollectionChanged;
        public event EventHandler<ViewItemsChangedEventArgs>? ViewItemsChanged;


        public FilmStrip Model
        {
            get { return _model; }
            set { SetProperty(ref _model, value); }
        }

        public FilmStripItemDetailToolTip DetailToolTip { get; }


        public void MoveWheel(object sender, MouseWheelEventArgs e)
        {
            _model.MoveWheel(sender, e);
        }

        public void MoveSelectedIndex(int delta)
        {
            _model.MoveSelectedIndex(delta);
        }

        public void RequestThumbnail(int start, int count, int margin, int direction)
        {
            _model.RequestThumbnail(start, count, margin, direction);
        }

        public void CancelThumbnailRequest()
        {
            _model.CancelThumbnailRequest();
        }

        internal void FlushSelectedIndex()
        {
            _model.FlushSelectedIndex();
        }
    }
}
