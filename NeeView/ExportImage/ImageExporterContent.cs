using System.Windows;

namespace NeeView
{
    public class ImageExporterContent
    {
        public ImageExporterContent(FrameworkElement view, Size size)
        {
            View = view;
            Size = size;
        }

        public FrameworkElement View { get; set; }

        public Size Size { get; set; }
    }
}
