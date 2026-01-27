using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// ManViewBay.xaml の相互作用ロジック
    /// </summary>
    public partial class MainViewBay : UserControl, IHasDeviceInput
    {
        private readonly MouseInput _mouseInput;
        private readonly TouchInput _touchInput;
        private SlideShowInput? _slideShowInput;

        public MouseInput MouseInput => _mouseInput;
        public TouchInput TouchInput => _touchInput;


        public MainViewBay()
        {
            InitializeComponent();

            this.DataContext = this;

            // mouse / touch command gesture binding
            var mouseGestureCommandCollection = MouseGestureCommandCollection.Current;
            _touchInput = new TouchInput(new TouchInputContext(this, null, mouseGestureCommandCollection, null, null, null, null, null));
            _mouseInput = new MouseInput(new MouseInputContext(this, null, mouseGestureCommandCollection, null, null, null, null, null));
            RoutedCommandTable.Current.AddMouseInput(_mouseInput);
            RoutedCommandTable.Current.AddTouchInput(_touchInput);

            // Drag&Drop設定
            this.AllowDrop = true;
            ContentDropManager.Current.SetDragDropEvent(this);

            this.Loaded += MainViewBay_Loaded;
            this.Unloaded += MainViewBay_Unloaded;
            this.PreviewMouseLeftButtonDown += MainViewBay_PreviewMouseLeftButtonDown;
        }


        private void MainViewBay_Loaded(object sender, RoutedEventArgs e)
        {
            _slideShowInput?.Dispose();
            _slideShowInput = new SlideShowInput(this, SlideShow.Current);
        }

        private void MainViewBay_Unloaded(object sender, RoutedEventArgs e)
        {
            _slideShowInput?.Dispose();
        }

        private void MainViewBay_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainViewManager.Current.RecoveryFloating();
        }
    }
}
