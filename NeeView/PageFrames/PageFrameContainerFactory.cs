using System;

namespace NeeView.PageFrames
{

    public class PageFrameContainerFactory : IDisposable
    {
        // TODO: container recycle ... ここではないな

        private readonly PageFrameContext _context;
        private readonly PageFrameTransformMap _transformMap;
        private readonly LoupeTransformContext _loupeContext;
        private readonly ViewContentFactory _viewContentFactory;
        private readonly BaseScaleTransform _baseScaleTransform;
        private readonly ContentSizeCalculator _calculator;
        private bool _disposedValue;


        public PageFrameContainerFactory(PageFrameContext context, PageFrameTransformMap transformMap, ViewSourceMap viewSourceMap, LoupeTransformContext loupeContext, BaseScaleTransform baseScaleTransform, ContentSizeCalculator calculator)
        {
            _context = context;
            _transformMap = transformMap;
            _loupeContext = loupeContext;
            _viewContentFactory = new ViewContentFactory(_context, viewSourceMap);
            _baseScaleTransform = baseScaleTransform;
            _calculator = calculator;
        }


        public PageFrameContainer Create(PageFrame frame, Page? nextPage)
        {
            //Debug.WriteLine($"PageFrameContainer.Create: {frame}, {nextPage?.Index}");

            var activity = new PageFrameActivity();
            var key = PageFrameTransformTool.CreateKey(frame);

            var rawTransform = _transformMap.ElementAt(key);
            if (!_context.ViewConfig.IsKeepPageTransform)
            {
                rawTransform.Clear();
            }

            var transform = new PageFrameTransformAccessor(_transformMap, rawTransform);
            var content = new PageFrameContent(_viewContentFactory, _context, frame, nextPage, activity, transform, _loupeContext, _baseScaleTransform, _calculator);
            var container = new PageFrameContainer(content, activity, _context.ViewScrollContext);
            return container;
        }

        public void Update(PageFrameContainer container, PageFrame frame, Page? nextPage)
        {
            //Debug.WriteLine($"PageFrameContainer.Update: {frame}, {nextPage?.Index}");

            if (container.Content is PageFrameContent frameContent && frameContent.PageFrame.IsMatch(frame) && container.DirtyLevel < PageFrameDirtyLevel.Replace)
            {
                frameContent.SetSource(frame);
                container.UpdateFrame();
            }
            else
            {
                var activity = container.Activity;
                var key = PageFrameTransformTool.CreateKey(frame);
                var transform = new PageFrameTransformAccessor(_transformMap, _transformMap.ElementAt(key));
                var content = new PageFrameContent(_viewContentFactory, _context, frame, nextPage, activity, transform, _loupeContext, _baseScaleTransform, _calculator);
                container.Content = content;
                container.UpdateFrame();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _viewContentFactory.Dispose();
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
