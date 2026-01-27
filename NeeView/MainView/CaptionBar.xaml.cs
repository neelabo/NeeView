using NeeView.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// CaptionBar.xaml の相互作用ロジック
    /// </summary>
    public partial class CaptionBar : UserControl
    {
        MainWindowCaptionEmulator? _windowCaptionEmulator;
        WindowStateCommands? _windowStateCommands;

        public CaptionBar()
        {
            InitializeComponent();

            this.Loaded += CaptionBar_Loaded;
            this.MouseRightButtonUp += CaptionBar_MouseRightButtonUp;
        }


        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(CaptionBar), new PropertyMetadata(""));


        public WindowStateManager WindowStateManager
        {
            get { return (WindowStateManager)GetValue(WindowStateManagerProperty); }
            set { SetValue(WindowStateManagerProperty, value); }
        }

        public static readonly DependencyProperty WindowStateManagerProperty =
            DependencyProperty.Register("WindowStateManager", typeof(WindowStateManager), typeof(CaptionBar), new PropertyMetadata(null, WindowStateManagerPropertyChanged));

        private static void WindowStateManagerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as CaptionBar)?.UpdateCaptionEmulator();
        }


        public bool IsMinimizeEnabled
        {
            get { return (bool)GetValue(IsMinimizeEnabledProperty); }
            set { SetValue(IsMinimizeEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsMinimizeEnabledProperty =
            DependencyProperty.Register("IsMinimizeEnabled", typeof(bool), typeof(CaptionBar), new PropertyMetadata(true, IsMinimizeEnabledPropertyChanged));

        private static void IsMinimizeEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as CaptionBar)?.UpdateSystemMenu();
        }


        public bool IsMaximizeEnabled
        {
            get { return (bool)GetValue(IsMaximizeEnabledProperty); }
            set { SetValue(IsMaximizeEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsMaximizeEnabledProperty =
            DependencyProperty.Register("IsMaximizeEnabled", typeof(bool), typeof(CaptionBar), new PropertyMetadata(true, IsMaximizeEnabledPropertyChanged));

        private static void IsMaximizeEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as CaptionBar)?.UpdateSystemMenu();
        }


        public bool IsMaximizeButtonMouseOver
        {
            get { return (bool)GetValue(IsMaximizeButtonMouseOverProperty); }
            set { SetValue(IsMaximizeButtonMouseOverProperty, value); }
        }

        public static readonly DependencyProperty IsMaximizeButtonMouseOverProperty =
            DependencyProperty.Register("IsMaximizeButtonMouseOver", typeof(bool), typeof(CaptionBar), new PropertyMetadata(false));


        public bool IsSystemMenuEnabled
        {
            get { return (bool)GetValue(IsSystemMenuEnabledProperty); }
            set { SetValue(IsSystemMenuEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsSystemMenuEnabledProperty =
            DependencyProperty.Register("IsSystemMenuEnabled", typeof(bool), typeof(CaptionBar), new PropertyMetadata(false));



        private void CaptionBar_Loaded(object? sender, RoutedEventArgs e)
        {
            UpdateCaptionEmulator();
            UpdateWindowStateCommands();
            UpdateSystemMenu();
        }

        private void CaptionBar_MouseRightButtonUp(object? sender, MouseButtonEventArgs e)
        {
            if (e.Handled) return;
            if (!IsSystemMenuEnabled) return;

            var window = Window.GetWindow(this);
            if (window is null) return;

            WindowTools.ShowSystemMenu(window);
            e.Handled = true;
        }


        private void UpdateCaptionEmulator()
        {
            var window = Window.GetWindow(this);
            if (window is null) return;

            if (_windowCaptionEmulator is null)
            {
                _windowCaptionEmulator = new MainWindowCaptionEmulator(window, this);
                _windowCaptionEmulator.IsMaximizeEnabled = IsMaximizeEnabled;
                _windowCaptionEmulator.IsEnabled = true;
            }

            _windowCaptionEmulator.WindowStateManager = this.WindowStateManager;
        }

        private void UpdateWindowStateCommands()
        {
            var window = Window.GetWindow(this);
            if (window is null) return;

            if (_windowStateCommands is null)
            {
                _windowStateCommands = new WindowStateCommands(window);
                _windowStateCommands.Bind();
            }
        }

        private void UpdateSystemMenu()
        {
            var window = Window.GetWindow(this);
            if (window is null) return;

            var disableFlags = (IsMinimizeEnabled ? WindowTools.WindowStyle.None : WindowTools.WindowStyle.MinimizeBox)
                | (IsMaximizeEnabled ? WindowTools.WindowStyle.None : WindowTools.WindowStyle.MaximizeBox);

            WindowTools.DisableStyle(window, disableFlags);
        }
    }
}
