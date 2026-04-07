using NeeView.PageFrames;
using System.Collections.Generic;
using System.Linq;


namespace NeeView
{
    /// <summary>
    /// タイトル文字列生成に必要な環境情報
    /// </summary>
    public class TitleSource
    {
        public TitleSource(Book? book, MainViewComponent mainViewComponent)
        {
            Book = book;
            FrameContent = mainViewComponent.PageFrameBoxPresenter.GetSelectedPageFrameContent();
            ViewContentDirection = FrameContent?.ViewContentsDirection ?? +1;
            Contents = (FrameContent?.ViewContents ?? new List<ViewContent>()).Where(e => !e.Element.IsDummy).ToList();
            ViewScale = FrameContent?.Transform.Scale ?? 1.0;
            DpiScale = mainViewComponent.MainView.DpiProvider.GetDpiScale().ToFixedScale().DpiScaleX;
        }

        public Book? Book { get; }
        public PageFrameContent? FrameContent { get; }
        public int ViewContentDirection { get; }
        public List<ViewContent> Contents { get; }
        public double ViewScale { get; }
        public double DpiScale { get; }
    }
}
