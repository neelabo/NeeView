using NeeView.PageFrames;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView
{
    public class CropImageContentControl : ImageContentControl
    {
        public CropImageContentControl(PageFrameElement source, ImageSource image, ViewContentSize contentSize, PageBackgroundSource backgroundSource)
            : base(source, image, contentSize, backgroundSource)
        {
        }

        protected override TargetElement CreateTarget(ImageSource imageSource, Rect viewbox)
        {
            var image = new Image()
            {
                Source = imageSource
            };

            var cropControl = new CropControl();
            cropControl.Target = image;
            cropControl.Viewbox = viewbox;

            return new TargetElement(cropControl, image);
        }
    }
}
