using NeeView.PageFrames;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NeeView
{
    public class BrushImageContentControl : ImageContentControl
    {
        public BrushImageContentControl(PageFrameElement source, ImageSource image, ViewContentSize contentSize, PageBackgroundSource backgroundSource)
            : base(source, image, contentSize, backgroundSource)
        {
        }

        protected override TargetElement CreateTarget(ImageSource imageSource, Rect viewbox)
        {
            var rectangle = new Rectangle();
            rectangle.Fill = CreatePageImageBrush(imageSource, viewbox, true);
            rectangle.HorizontalAlignment = HorizontalAlignment.Stretch;
            rectangle.VerticalAlignment = VerticalAlignment.Stretch;

            return new TargetElement(rectangle, rectangle);
        }

        private static ImageBrush CreatePageImageBrush(ImageSource imageSource, Rect viewbox, bool isStretch)
        {
            var brush = new ImageBrush();
            brush.ImageSource = imageSource;
            brush.AlignmentX = AlignmentX.Left;
            brush.AlignmentY = AlignmentY.Top;
            brush.Stretch = isStretch ? Stretch.Fill : Stretch.None;
            brush.TileMode = TileMode.None;
            brush.Viewbox = viewbox;
            if (brush.CanFreeze)
            {
                brush.Freeze();
            }

            return brush;
        }
    }
}
