using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
