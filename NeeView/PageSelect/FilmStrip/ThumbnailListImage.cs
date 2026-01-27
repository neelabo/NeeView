using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView
{
    public class ThumbnailListImage : Control
    {
        static ThumbnailListImage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ThumbnailListImage), new FrameworkPropertyMetadata(typeof(ThumbnailListImage)));
        }


        public ImageSource? Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(ThumbnailListImage), new PropertyMetadata(null));
    }
}
