using NeeLaboratory.ComponentModel;
using NeeView.PageFrames;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// ページタイトル
    /// </summary>
    public class PageTitle : BindableBase
    {
        static PageTitle() => Current = new PageTitle();
        public static PageTitle Current { get; }


        private readonly string _defaultPageTitle = "";
        private string _title = "";

        private readonly BookHub _bookHub;
        private readonly PageFrameBoxPresenter _presenter;
        private readonly TitleStringService _titleStringService;
        private readonly TitleString _titleString;


        public PageTitle()
        {
            _bookHub = BookHub.Current;
            _presenter = MainViewComponent.Current.PageFrameBoxPresenter;
            _titleStringService = TitleStringService.Default;

            _titleString = new TitleString(_titleStringService);
            _titleString.AddPropertyChanged(nameof(TitleString.Title), TitleString_TitleChanged);

            _presenter.ViewContentChanged += (s, e) =>
            {
                if (e.Action < ViewContentChangedAction.ContentLoading) return;
                UpdateFormat();
            };

            _presenter.Loading += (s, e) =>
            {
                UpdateTitle();
            };

            Config.Current.PageTitle.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(PageTitleConfig.PageTitleFormat1):
                    case nameof(PageTitleConfig.PageTitleFormat2):
                    case nameof(PageTitleConfig.PageTitleFormatMedia):
                        UpdateFormat();
                        break;
                }
            };

            UpdateTitle();
        }


        public string Title
        {
            get { return _title; }
            private set { SetProperty(ref _title, value); }
        }


        private void TitleString_TitleChanged(object? sender, PropertyChangedEventArgs e)
        {
            UpdateTitle();
        }

        private void UpdateFormat()
        {
            var frameContent = _presenter.GetSelectedPageFrameContent();

            var contents = (frameContent?.ViewContents ?? new List<ViewContent>()).Where(e => !e.Element.IsDummy).ToList();
            var isMedia = contents.FirstOrDefault()?.Element.Page.ArchiveEntry.Archive is MediaArchive == true;

            string format = isMedia
                ? Config.Current.PageTitle.PageTitleFormatMedia
                : contents.Count >= 2 ? Config.Current.PageTitle.PageTitleFormat2 : Config.Current.PageTitle.PageTitleFormat1;

            _titleString.SetFormat(format);
        }

        private void UpdateTitle()
        {
            if (_presenter.IsLoading)
            {
                Title = _defaultPageTitle;
            }
            else if (_bookHub.GetCurrentBook()?.Path == null)
            {
                Title = _defaultPageTitle;
            }
            else if (_presenter.GetSelectedPageFrameContent() == null)
            {
                Title = _defaultPageTitle;
            }
            else
            {
                Title = _titleString.Title;
            }
        }

    }

}
