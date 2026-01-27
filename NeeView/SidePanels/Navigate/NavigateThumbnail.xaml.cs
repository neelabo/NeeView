using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// NavigateThumbnail.xaml の相互作用ロジック
    /// </summary>
    public partial class NavigateThumbnail : UserControl
    {
        private readonly NavigateThumbnailViewModel _vm;

        public NavigateThumbnail()
        {
            InitializeComponent();

            _vm = new NavigateThumbnailViewModel(MainViewComponent.Current);
            this.Root.DataContext = _vm;

            this.IsVisibleChanged += NavigateThumbnail_IsVisibleChanged;

            this.MouseLeftButtonDown += ThumbnailGrid_MouseLeftButtonDown;
            this.PreviewMouseLeftButtonUp += ThumbnailGrid_PreviewMouseLeftButtonUp;
            this.MouseMove += ThumbnailGrid_MouseMove;
            this.SizeChanged += NavigateThumbnail_SizeChanged;
        }

        private void NavigateThumbnail_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            _vm.SetCanvasSize(e.NewSize);
        }

        private void NavigateThumbnail_IsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            _vm.IsEnabled = this.IsVisible;
        }

        private void ThumbnailGrid_MouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
        {
            this.CaptureMouse();
            this.Cursor = Cursors.Hand;

            _vm.LookAt(e.GetPosition(this.ThumbnailGrid));
        }

        private void ThumbnailGrid_PreviewMouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
        {
            this.ReleaseMouseCapture();
            this.Cursor = null;
        }

        private void ThumbnailGrid_MouseMove(object? sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            _vm.LookAt(e.GetPosition(this.ThumbnailGrid));
        }
    }
}
