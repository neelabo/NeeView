using NeeView.Windows;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// MenuBar : View
    /// </summary>
    public partial class MenuBarView : UserControl
    {
        private MenuBarViewModel? _vm;


        public MenuBarView()
        {
            InitializeComponent();

            this.Watermark.Visibility = Environment.Watermark ? Visibility.Visible : Visibility.Collapsed;

            if (Environment.IsCanaryPackage)
            {
                this.Watermark.Background = new SolidColorBrush(Color.FromRgb(0xF3, 0xBC, 0x2D));
                this.WatermarkText.Foreground = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20));
                this.WatermarkText.Text = "Canary" + Environment.DateVersion;
            }
            else if (Environment.IsBetaPackage)
            {
                this.Watermark.Background = new SolidColorBrush(Color.FromRgb(0x2E, 0x69, 0xD1));
                this.WatermarkText.Foreground = Brushes.White;
                this.WatermarkText.Text = "Beta" + Environment.DateVersion;
            }
            else
            {
                this.Watermark.Background = Brushes.DimGray;
                this.WatermarkText.Foreground = Brushes.White;
                this.WatermarkText.Text = Environment.PackageType;
            }

            this.WindowCaptionButtons.MouseRightButtonUp += (s, e) => e.Handled = true;
            this.MainMenuJoint.MouseRightButtonUp += (s, e) => e.Handled = true;
            this.MouseRightButtonUp += MenuBarView_MouseRightButtonUp;
        }


        public MenuBar Source
        {
            get { return (MenuBar)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(MenuBar), typeof(MenuBarView), new PropertyMetadata(null, SourcePropertyChanged));

        private static void SourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as MenuBarView)?.Initialize();
        }


        public void Initialize()
        {
            _vm = new MenuBarViewModel(this.Source, this);
            this.Root.DataContext = _vm;
        }

        // 単キーのショートカット無効
        private void Control_KeyDown_IgnoreSingleKeyGesture(object sender, KeyEventArgs e)
        {
            KeyExGesture.AddFilter(KeyExGestureFilter.All);
        }

        // システムメニュー表示
        private void MenuBarView_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_vm is null) return;

            if (_vm.IsCaptionEnabled)
            {
                WindowTools.ShowSystemMenu(Window.GetWindow(this));
                e.Handled = true;
            }
        }
    }
}
