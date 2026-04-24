using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView
{
    public class FilmStripImage : Control
    {
        static FilmStripImage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FilmStripImage), new FrameworkPropertyMetadata(typeof(FilmStripImage)));
        }


        public ImageSource? Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(FilmStripImage), new PropertyMetadata(null));
    }
}
