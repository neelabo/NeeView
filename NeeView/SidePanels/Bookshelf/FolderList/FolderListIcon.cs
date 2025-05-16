using System;
using System.Collections.Generic;
using System.Globalization;
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
    public partial class FolderListIcon : Control
    {
        static FolderListIcon()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FolderListIcon), new FrameworkPropertyMetadata(typeof(FolderListIcon)));
        }

        public FolderItem? FolderItem
        {
            get { return (FolderItem)GetValue(FolderItemProperty); }
            set { SetValue(FolderItemProperty, value); }
        }

        public static readonly DependencyProperty FolderItemProperty =
            DependencyProperty.Register("FolderItem", typeof(FolderItem), typeof(FolderListIcon), new PropertyMetadata(null));


        public bool IsKeepArea
        {
            get { return (bool)GetValue(IsKeepAreaProperty); }
            set { SetValue(IsKeepAreaProperty, value); }
        }

        public static readonly DependencyProperty IsKeepAreaProperty =
            DependencyProperty.Register("IsKeepArea", typeof(bool), typeof(FolderListIcon), new PropertyMetadata(false));


        public ImageSource? FolderIcon
        {
            get { return (ImageSource)GetValue(FolderIconProperty); }
            set { SetValue(FolderIconProperty, value); }
        }

        public static readonly DependencyProperty FolderIconProperty =
            DependencyProperty.Register("FolderIcon", typeof(ImageSource), typeof(FolderListIcon), new PropertyMetadata(null));


        public ImageSource? LinkIcon
        {
            get { return (ImageSource)GetValue(LinkIconProperty); }
            set { SetValue(LinkIconProperty, value); }
        }

        public static readonly DependencyProperty LinkIconProperty =
            DependencyProperty.Register("LinkIcon", typeof(ImageSource), typeof(FolderListIcon), new PropertyMetadata(null));


        public ImageSource? PlaylistIcon
        {
            get { return (ImageSource)GetValue(PlaylistIconProperty); }
            set { SetValue(PlaylistIconProperty, value); }
        }

        public static readonly DependencyProperty PlaylistIconProperty =
            DependencyProperty.Register("PlaylistIcon", typeof(ImageSource), typeof(FolderListIcon), new PropertyMetadata(null));
    }


    [ValueConversion(typeof(FolderItemIconOverlay), typeof(Visibility))]
    public class FolderItemIconOverlayToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FolderItemIconOverlay overlay)
            {
                if (overlay != FolderItemIconOverlay.None && overlay != FolderItemIconOverlay.Uninitialized)
                {
                    return Visibility.Visible;
                }
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(FolderItemIconOverlay), typeof(ImageSource))]
    public class FolderItemIconOverlayToImageSourceConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FolderItemIconOverlay overlay)
            {
                switch (overlay)
                {
                    case FolderItemIconOverlay.Checked:
                        return MainWindow.Current.Resources["ic_done_24px"];
                    case FolderItemIconOverlay.Star:
                        return MainWindow.Current.Resources["ic_grade_24px"];
                    case FolderItemIconOverlay.Disable:
                        return App.Current.Resources["ic_clear_24px"];
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
