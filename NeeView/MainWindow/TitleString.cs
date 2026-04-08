using NeeLaboratory.ComponentModel;
using NeeView.PageFrames;
using NeeView.StringTemplate;
using System.Linq;

namespace NeeView
{
    public class TitleString : BindableBase
    {
        private string _title = "";
        private string _format = "";
        private StringFormat<TitleSource> _formatSource = new();
        private StringFormatChangedAction _changedAction = StringFormatChangedAction.None;


        public TitleString()
        {
            var presenter = MainViewComponent.Current.PageFrameBoxPresenter;

            presenter.ViewContentChanged += (s, e) =>
            {
                if (AppState.Instance.IsProcessingBook) return;
                if (e.Action < ViewContentChangedAction.ContentLoading) return;

                UpdateTitle(StringFormatChangedAction.ViewContentChanged);
            };

            presenter.TransformChanged += (s, e) =>
            {
                if (e.Action == PageFrames.TransformAction.Scale)
                {
                    UpdateTitle(StringFormatChangedAction.ScaleChanged);
                }
            };

            presenter.StretchChanged += (s, e) =>
            {
                UpdateTitle(StringFormatChangedAction.StretchChanged);
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
                .Select(e => e.FormatInfo?.ChangedAction ?? StringFormatChangedAction.None)
                .Aggregate(StringFormatChangedAction.FormatChanged, (a, b) => a | b);

            UpdateTitle();
        }


        private void UpdateTitle(StringFormatChangedAction action)
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
