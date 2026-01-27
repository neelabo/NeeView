using NeeView.PageFrames;

namespace NeeView
{
    public class ViewContentFactory
    {
        private readonly PageFrameContext _context;
        private readonly ViewSourceMap _viewSourceMap;
        private readonly PageBackgroundSource _backgroundSource;

        public ViewContentFactory(PageFrameContext context, ViewSourceMap viewSourceMap)
        {
            _context = context;
            _viewSourceMap = viewSourceMap;
            _backgroundSource = new PageBackgroundSource();
        }

        public ViewContent Create(PageFrameElement element, PageFrameElementScale scale, PageFrameActivity activity, int index)
        {
            var viewSource = _viewSourceMap.Get(element.Page, element.PagePart, element.PageDataSource);
            return new ViewContent(_context, element, scale, viewSource, activity, _backgroundSource, index);
        }
    }
}
