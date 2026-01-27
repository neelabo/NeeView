using NeeView.ComponentModel;
using NeeView.PageFrames;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// 表示座標系操作のリソース
    /// </summary>
    public abstract class ContentDragTransformContext : DragTransformContext
    {
        public ContentDragTransformContext(FrameworkElement sender, ITransformControl transform, ViewConfig viewConfig, MouseConfig mouseConfig)
            : base(sender, transform, viewConfig, mouseConfig)
        {
        }

        public Rect ContentRect { get; protected set; }
        public Point ContentCenter => ContentRect.Center();

        public Point RotateCenter { get; protected set; }
        public Point ScaleCenter { get; protected set; }
        public Point FlipCenter { get; protected set; }


        public virtual PageFrameContent? GetPageFrameContent()
        {
            return null;
        }

        public virtual void UpdateRect()
        {
        }
    }

}
