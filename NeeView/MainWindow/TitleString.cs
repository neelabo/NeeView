using NeeLaboratory.ComponentModel;
using NeeView.PageFrames;
using System.Linq;

namespace NeeView
{
    public class TitleString : BindableBase
    {
        private string _title = "";
        private string _format = "";
        private TitleFormatSource _formatSource = new();
        private TitleStringChangedAction _changedAction = TitleStringChangedAction.None;


        public TitleString()
        {
            var presenter = MainViewComponent.Current.PageFrameBoxPresenter;

            presenter.ViewContentChanged += (s, e) =>
            {
                if (AppState.Instance.IsProcessingBook) return;
                if (e.Action < ViewContentChangedAction.ContentLoading) return;

                UpdateTitle(TitleStringChangedAction.ViewContentChanged);
            };

            presenter.TransformChanged += (s, e) =>
            {
                if (e.Action == PageFrames.TransformAction.Scale)
                {
                    UpdateTitle(TitleStringChangedAction.ScaleChanged);
                }
            };

            presenter.StretchChanged += (s, e) =>
            {
                UpdateTitle(TitleStringChangedAction.StretchChanged);
            };
        }


        public string Title
        {
            get { return _title; }
            private set { SetProperty(ref _title, value); }
        }


        public void SetFormat(string format)
        {
            if (_format == format) return;

            _format = format;

            _formatSource = TitleStringFormatter.CreateFormatSource(_format);

            _changedAction = _formatSource.Words
                .Select(e => e.FormatInfo?.ChangedAction ?? TitleStringChangedAction.None)
                .Aggregate(TitleStringChangedAction.FormatChanged, (a, b) => a | b);

            UpdateTitle();
        }


        private void UpdateTitle(TitleStringChangedAction action)
        {
            if ((_changedAction | action) != 0)
            {
                UpdateTitle();
            }
        }

        public void UpdateTitle()
        {
            var titleSource = new TitleSource(BookHub.Current.GetCurrentBook(), MainViewComponent.Current);
            Title = TitleStringFormatter.Format(_formatSource, titleSource);
        }
    }
}
