using System;
using System.Windows;


namespace NeeView.PageFrames
{
    public interface IPageFrameContent : IDisposable, INotifyTransformChanged
    {
        public event EventHandler<FrameViewContentChangedEventArgs>? ViewContentChanged;
        public event EventHandler? ContentSizeChanged;
        FrameworkElement? Content { get; }
        IPageFrameTransform Transform { get; }

        PageFrameActivity Activity { get; }
        bool IsLocked { get; }
        PageRange FrameRange { get; }
        bool IsFirstFrame { get; }
        bool IsLastFrame { get; }
        PageFrameDirtyLevel DirtyLevel { get; set; }
        bool IsStaticFrame { get; }

        void OnAttached();
        void OnDetached();
        Rect GetContentRect();
        Size GetFrameSize();
    }


    public class DummyPageFrameContent : IPageFrameContent
    {
        public DummyPageFrameContent(PageFrameActivity activity)
        {
            Activity = activity;
        }

#pragma warning disable CS0067
        public event TransformChangedEventHandler? TransformChanged;
        public event EventHandler<FrameViewContentChangedEventArgs>? ViewContentChanged;
        public event EventHandler? ContentSizeChanged;
#pragma warning restore CS0067

        public virtual PageRange FrameRange => PageRange.Empty;

        public IPageFrameTransform Transform => new DummyPageFrameTransform();

        public virtual FrameworkElement? Content => null;

        public PageFrameActivity Activity { get; }

        public virtual bool IsLocked => false;

        public bool IsFirstFrame => false;

        public bool IsLastFrame => false;

        public PageFrameDirtyLevel DirtyLevel
        {
            get { return PageFrameDirtyLevel.Clean; }
            set { }
        }

        public bool IsStaticFrame => false;


        public void Dispose()
        {
        }

        public static void SetDirty(PageFrameDirtyLevel level)
        {
        }

        public Rect GetContentRect()
        {
            return new Rect();
        }

        public Size GetFrameSize()
        {
            return new Size();
        }

        public override string ToString()
        {
            return "Dummy";
        }

        public void OnAttached()
        {
        }

        public void OnDetached()
        {
        }
    }

}
