using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NeeLaboratory.Linq;
using NeeView.PageFrames;
using NeeView.Text;
using NeeView.Windows;

namespace NeeView
{
    public class TitleStringService
    {
        private static TitleStringService? _default;
        public static TitleStringService Default => _default = _default ?? new TitleStringService(MainViewComponent.Current);


        private readonly ReplaceString _replaceString = new();
        private readonly PageFrameBoxPresenter _presenter;
        private readonly DpiScaleProvider _dpiScaleProvider;
        private int _changedCount;


        public TitleStringService(MainViewComponent mainViewComponent)
        {
            _presenter = mainViewComponent.PageFrameBoxPresenter;
            _dpiScaleProvider = mainViewComponent.MainView.DpiProvider;

            _presenter.ViewContentChanged += (s, e) =>
            {
                if (e.Action < ViewContentChangedAction.ContentLoading) return;
                Update();
            };

            _presenter.TransformChanged += (s, e) =>
            {
                if (e.Action == PageFrames.TransformAction.Scale)
                {
                    Update();
                }
            };

            _presenter.StretchChanged += (s, e) =>
            {
                Update();
            };

            _replaceString.Changed += ReplaceString_Changed;
        }


        public event EventHandler? Changed;


        private void ReplaceString_Changed(object? sender, ReplaceStringChangedEventArgs e)
        {
            _changedCount++;
        }

        public string Replace(string src, IEnumerable<string> keys)
        {
            return _replaceString.Replace(src, keys);
        }

        private void Update()
        {
            _changedCount = 0;

            var frameContent = _presenter.GetSelectedPageFrameContent();
            var viewContentDirection = frameContent?.ViewContentsDirection ?? +1;
            var contents = (frameContent?.ViewContents ?? new List<ViewContent>()).Where(e => !e.Element.IsDummy).Direction(viewContentDirection).ToList();
            var viewScale = frameContent?.Transform.Scale ?? 1.0;

            bool isMainContent0 = contents.Count <= 1 || viewContentDirection == 1;  //mainContent == contents[0];

            // book
            var book = BookHub.Current.GetCurrentBook();
            string bookName = LoosePath.GetDisplayName(book?.Path);
            _replaceString.Set("$Book", bookName);

            // page
            var pageMax = book != null ? book.Pages.Count : 0;
            _replaceString.Set("$PageMax", pageMax.ToString(CultureInfo.InvariantCulture));


            string pageNum0 = GetPageNum(contents.ElementAtOrDefault(0));
            string pageNum1 = GetPageNum(contents.ElementAtOrDefault(1));
            _replaceString.Set("$Page", isMainContent0 ? pageNum0 : pageNum1);
            _replaceString.Set("$PageL", pageNum0);
            _replaceString.Set("$PageR", pageNum1);

            string GetPageNum(ViewContent? content)
            {
                if (content is null) return "0";
                var pageElement = content.Element; 
                return (pageElement.PageRange.PartSize == 2) ? (pageElement.Page.Index + 1).ToString(CultureInfo.InvariantCulture) : (pageElement.Page.Index + 1).ToString(CultureInfo.InvariantCulture) + (pageElement.PageRange.Min.Part == 1 ? ".5" : ".0");
            }

            string fullPath0 = GetFullPath(contents.ElementAtOrDefault(0));
            string fullPath1 = GetFullPath(contents.ElementAtOrDefault(1));
            _replaceString.Set("$FullPath", isMainContent0 ? fullPath0 : fullPath1);
            _replaceString.Set("$FullPathL", fullPath0);
            _replaceString.Set("$FullPathR", fullPath1);

            string GetFullPath(ViewContent? content)
            {
                if (content is null) return "";
                var pageElement = content.Element;
                var fullPath = pageElement.Page.EntryFullName;
                return fullPath + GetPartString(content);
            }

            string path0 = GetFullName(contents.ElementAtOrDefault(0));
            string path1 = GetFullName(contents.ElementAtOrDefault(1));
            _replaceString.Set("$FullName", isMainContent0 ? path0 : path1);
            _replaceString.Set("$FullNameL", path0);
            _replaceString.Set("$FullNameR", path1);

            string GetFullName(ViewContent? content)
            {
                if (content is null) return "";
                var pageElement = content.Element;
                var fullPath = pageElement.Page.EntryName;
                return fullPath.Replace("/", " > ", StringComparison.Ordinal).Replace("\\", " > ", StringComparison.Ordinal) + GetPartString(content);
            }

            string name0 = GetName(contents.ElementAtOrDefault(0));
            string name1 = GetName(contents.ElementAtOrDefault(1));
            _replaceString.Set("$Name", isMainContent0 ? name0 : name1);
            _replaceString.Set("$NameL", name0);
            _replaceString.Set("$NameR", name1);

            string GetName(ViewContent? content)
            {
                if (content is null) return "";
                var pageElement = content.Element;
                var fullPath = pageElement.Page.EntryName;
                return LoosePath.GetFileName(fullPath) + GetPartString(content);
            }

            string GetPartString(ViewContent? content)
            {
                if (content is null) return "";
                var pageElement = content.Element;
                var direction = frameContent?.ViewContentsDirection ?? 1;
                if (pageElement.PageRange.PartSize == 1)
                {
                    var part = pageElement.PageRange.Min.Part;
                    part = direction < 0 ? 1 - part : part;
                    return part == 0 ? "(L)" : "(R)";
                }
                else
                {
                    return "";
                }
            }

            var bitmapContent0 = contents.ElementAtOrDefault(0)?.Element.Page.Content; //as BitmapPageContent;
            var bitmapContent1 = contents.ElementAtOrDefault(1)?.Element.Page.Content; // as BitmapPageContent;
            var pictureInfo0 = bitmapContent0?.PictureInfo;
            var pictureInfo1 = bitmapContent1?.PictureInfo;
            string bpp0 = GetSizeEx(pictureInfo0);
            string bpp1 = GetSizeEx(pictureInfo1);
            _replaceString.Set("$SizeEx", isMainContent0 ? bpp0 : bpp1);
            _replaceString.Set("$SizeExL", bpp0);
            _replaceString.Set("$SizeExR", bpp1);

            string GetSizeEx(PictureInfo? pictureInfo)
            {
                return pictureInfo != null ? GetSize(pictureInfo) + "×" + pictureInfo.BitsPerPixel.ToString(CultureInfo.InvariantCulture) : "";
            }

            string size0 = GetSize(pictureInfo0);
            string size1 = GetSize(pictureInfo1);
            _replaceString.Set("$Size", isMainContent0 ? size0 : size1);
            _replaceString.Set("$SizeL", size0);
            _replaceString.Set("$SizeR", size1);

            string GetSize(PictureInfo? pictureInfo)
            {
                return pictureInfo != null ? $"{pictureInfo.OriginalSize.Width}×{pictureInfo.OriginalSize.Height}" : "";
            }

            // view scale
            _replaceString.Set("$ViewScale", $"{(int)(viewScale * 100 + 0.1)}%");

            // scale
            var dpiScaleX = _dpiScaleProvider.GetDpiScale().ToFixedScale().DpiScaleX;
            string scale0 = $"{(int)(viewScale * GetOriginalScale(contents.ElementAtOrDefault(0)) * dpiScaleX * 100 + 0.1)}%";
            string scale1 = $"{(int)(viewScale * GetOriginalScale(contents.ElementAtOrDefault(1)) * dpiScaleX * 100 + 0.1)}%";
            _replaceString.Set("$Scale", isMainContent0 ? scale0 : scale1);
            _replaceString.Set("$ScaleL", scale0);
            _replaceString.Set("$ScaleR", scale1);

            double GetOriginalScale(ViewContent? content)
            {
                if (content is null) return 1.0;
                var pageElement = content.Element;
                return (frameContent?.PageFrame.Scale ?? 1.0) * pageElement.Scale;
            }

            if (_changedCount > 0)
            {
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }
    }

}
