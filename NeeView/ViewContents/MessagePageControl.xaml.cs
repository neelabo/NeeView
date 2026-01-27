using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// MessagePageControl.xaml の相互作用ロジック
    /// </summary>
    public partial class MessagePageControl : UserControl
    {
        public MessagePageControl(FileViewData context)
        {
            InitializeComponent();

            this.FileCard.Icon = context.ImageSource;
            this.FileCard.ArchiveEntry = context.Entry;

            this.MessageTextBlock.Text = context.Message;
        }

        public static readonly DependencyProperty DefaultBrushProperty =
            DependencyProperty.Register(
            "DefaultBrush",
            typeof(Brush),
            typeof(MessagePageControl),
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
