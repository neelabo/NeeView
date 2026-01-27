using NeeLaboratory.ComponentModel;
using NeeView.PageFrames;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// ウィンドウタイトル
    /// </summary>
    public class WindowTitle : BindableBase
    {
        static WindowTitle() => Current = new WindowTitle();
        public static WindowTitle Current { get; }


        private readonly string _defaultWindowTitle;
        private string _title = "";

        private readonly BookHub _bookHub;
        //private readonly MainViewComponent _mainViewComponent;
        private readonly PageFrameBoxPresenter _presenter;
        private readonly TitleStringService _titleStringService;
        private readonly TitleString _titleString;


        public WindowTitle()
        {
            _bookHub = BookHub.Current;
            //_mainViewComponent = MainViewComponent.Current;
            _presenter = MainViewComponent.Current.PageFrameBoxPresenter;
            _titleStringService = TitleStringService.Default;

            _titleString = new TitleString(_titleStringService);
            _titleString.AddPropertyChanged(nameof(TitleString.Title), TitleString_TitleChanged);

            _defaultWindowTitle = $"{Environment.ApplicationName} {Environment.DisplayVersion}";

            _presenter.ViewContentChanged += (s, e) =>
            {
                if (e.Action < ViewContentChangedAction.ContentLoading) return;
                UpdateFormat();
            };

            _presenter.Loading += (s, e) =>
            {
                UpdateTitle();
            };

            Config.Current.WindowTitle.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(WindowTitleConfig.WindowTitleFormat1):
                    case nameof(WindowTitleConfig.WindowTitleFormat2):
                    case nameof(WindowTitleConfig.WindowTitleFormatMedia):
                        UpdateFormat();
                        break;
                }
            };

            UpdateTitle();
        }


        public string Title
        {
            get { return _title; }
            private set { _title = value; RaisePropertyChanged(); }
        }


        private void TitleString_TitleChanged(object? sender, PropertyChangedEventArgs e)
        {
            UpdateTitle();
        }

        private void UpdateFormat()
        {
            var contents = (_presenter.GetSelectedPageFrameContent()?.ViewContents ?? new List<ViewContent>()).Where(e => !e.Element.IsDummy).ToList();
            var isMedia = contents.FirstOrDefault()?.Element.Page.ArchiveEntry.Archive is MediaArchive == true;

            string format = isMedia
                ? Config.Current.WindowTitle.WindowTitleFormatMedia
                : contents.Count >= 2 ? Config.Current.WindowTitle.WindowTitleFormat2 : Config.Current.WindowTitle.WindowTitleFormat1;

            _titleString.SetFormat(format);
        }

        private void UpdateTitle()
        {
            var address = _bookHub.GetCurrentBook()?.Path;

            if (address == null)
            {
                Title = _defaultWindowTitle;
            }
            else
            {
                Title = _titleString.Title;
            }
        }

    }
}
