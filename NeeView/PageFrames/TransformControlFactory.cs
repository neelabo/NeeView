using NeeView.Maths;
using System.Windows;

namespace NeeView.PageFrames
{
    public class TransformControlFactory
    {
        private readonly PageFrameContext _context;
        private readonly ViewTransformContext _viewContext;
        private readonly LoupeTransformContext _loupeContext;
        private readonly ScrollLock _scrollLock;


        public TransformControlFactory(PageFrameContext context, ViewTransformContext viewContext, LoupeTransformContext loupeContext, ScrollLock scrollLock)
        {
            _context = context;
            _viewContext = viewContext;
            _loupeContext = loupeContext;
            _scrollLock = scrollLock;
        }

        public ITransformControl Create(PageFrameContainer container)
        {
            if (_context.IsStaticFrame)
            {
                var containerRect = container.Rect.Size.ToRect();
                return new ContentTransformControl(_context, container, containerRect, _scrollLock);
            }
            else
            {
                return new ViewTransformControl(_context, container, _viewContext, _scrollLock);
            }
        }

        public ITransformControl CreateLoupe()
        {
            return new LoupeTransformControl(_loupeContext);
        }

        public Rect CreateContentRect(PageFrameContainer container)
        {
            if (_context.IsStaticFrame)
            {
                return container.GetContentRect();
            }
            else
            {
                return container.Rect;
            }
        }

        public Rect CreateViewRect(Rect viewRect)
        {
            if (_context.IsStaticFrame)
            {
                // TODO: Container から求めるべきでは？
                return viewRect.Size.ToRect();
            }
            else
            {
                return viewRect;
            }
        }
    }





}
