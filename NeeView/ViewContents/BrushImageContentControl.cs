using NeeView.Media.Imaging;
using NeeView.PageFrames;
using System.Diagnostics;
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
            rectangle.Fill = BrushImageContentTools.CreateImageBrush(imageSource, viewbox, Stretch.Fill);
            rectangle.HorizontalAlignment = HorizontalAlignment.Stretch;
            rectangle.VerticalAlignment = VerticalAlignment.Stretch;

            return new TargetElement(rectangle, rectangle);
        }
    }


    /// <summary>
    /// BrushImageContentControl 用のツール
    /// </summary>
    public static class BrushImageContentTools
    {
        public static ImageBrush CreateImageBrush(ImageSource imageSource, Rect viewbox, Stretch stretch)
        {
            var brush = new ImageBrush();
            brush.ImageSource = imageSource;
            brush.AlignmentX = AlignmentX.Left;
            brush.AlignmentY = AlignmentY.Top;
            brush.Stretch = stretch;
            brush.TileMode = TileMode.None;
            brush.Viewbox = viewbox;
            brush.ViewboxUnits = BrushMappingMode.RelativeToBoundingBox;

            if (brush.CanFreeze)
            {
                brush.Freeze();
            }

            return brush;
        }

        public static Size GetImageSourcePixelSize(UIElement element)
        {
            if (element is not Rectangle rectangle)
            {
                return Size.Empty;
            }
        
            return GetImageSourcePixelSize(rectangle);
        }

        public static Size GetImageSourcePixelSize(Rectangle rectangle)
        {
            if (rectangle.Fill is not ImageBrush imageBrush)
            {
                return Size.Empty;
            }

            return new Size(imageBrush.ImageSource.GetPixelWidth(), imageBrush.ImageSource.GetPixelHeight());
        }


        public static Rect GetViewBox(UIElement element)
        {
            if (element is not Rectangle rectangle)
            {
                return Rect.Empty;
            }

            return GetViewBox(rectangle);
        }

        public static Rect GetViewBox(Rectangle rectangle)
        {
            if (rectangle.Fill is not ImageBrush imageBrush)
            {
                return Rect.Empty;
            }

            Debug.Assert(imageBrush.ViewboxUnits == BrushMappingMode.RelativeToBoundingBox);
            return imageBrush.Viewbox;
        }
    }
}
