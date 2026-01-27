using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// FilePageControl.xaml の相互作用ロジック
    /// </summary>
    public partial class FilePageControl : UserControl
    {
        public FilePageControl(FileViewData context)
        {
            InitializeComponent();

            this.FileCard.Icon = context.ImageSource;
            this.FileCard.ArchiveEntry = context.Entry;
        }


        public static readonly DependencyProperty DefaultBrushProperty =
            DependencyProperty.Register(
            "DefaultBrush",
            typeof(Brush),
            typeof(FilePageControl),
            new FrameworkPropertyMetadata(Brushes.White, new PropertyChangedCallback(OnDefaultBrushChanged)));

        public Brush DefaultBrush
        {
            get { return (Brush)GetValue(DefaultBrushProperty); }
            set { SetValue(DefaultBrushProperty, value); }
        }

        private static void OnDefaultBrushChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
        }

    }
}
