using CommunityToolkit.Mvvm.ComponentModel;
using NeeView.Windows.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView.Windows.Controls
{
    /// <summary>
    /// ColorPicker.xaml の相互作用ロジック
    /// </summary>
    [INotifyPropertyChanged]
    public partial class ColorPicker : UserControl
    {
        private bool _isPropertyLocked;
        private Color _rgb = Colors.Black;
        private HSVColor _hsv = Colors.Black.ToHSV();
        private DisablePopupBackgroundWheel? _disablePopupBackgroundWheel;
        private UIElement? _popupClosedFocusElement;
        private bool _isDraggingDropper;


        public ColorPicker()
        {
            InitializeComponent();

            this.Root.DataContext = this;
        }


        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(ColorPicker), new FrameworkPropertyMetadata(Colors.Black, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ColorProperty_Changed));

        private static void ColorProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorPicker control)
            {
                control.Flush();
            }
        }


        public Color? DefaultColor
        {
            get { return (Color?)GetValue(DefaultColorProperty); }
            set { SetValue(DefaultColorProperty, value); }
        }

        public static readonly DependencyProperty DefaultColorProperty =
            DependencyProperty.Register(nameof(DefaultColor), typeof(Color?), typeof(ColorPicker), new PropertyMetadata(null));


        public ColorMode ColorMode
        {
            get { return (ColorMode)GetValue(ColorModeProperty); }
            set { SetValue(ColorModeProperty, value); }
        }

        public static readonly DependencyProperty ColorModeProperty =
            DependencyProperty.Register(nameof(ColorMode), typeof(ColorMode), typeof(ColorPicker), new FrameworkPropertyMetadata(ColorMode.RGB, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        public List<ColorMode> ColorModeList { get; } = Enum.GetValues(typeof(ColorMode)).Cast<ColorMode>().ToList();


        public byte R
        {
            get { return _rgb.R; }
            set { UpdateColor(Color.FromArgb(_rgb.A, value, _rgb.G, _rgb.B)); }
        }

        public byte G
        {
            get { return _rgb.G; }
            set { UpdateColor(Color.FromArgb(_rgb.A, _rgb.R, value, _rgb.B)); }
        }

        public byte B
        {
            get { return _rgb.B; }
            set { UpdateColor(Color.FromArgb(_rgb.A, _rgb.R, _rgb.G, value)); }
        }

        public byte A
        {
            get { return _rgb.A; }
            set { UpdateColor(Color.FromArgb(value, _rgb.R, _rgb.G, _rgb.B)); }
        }

        public int H
        {
            get { return (int)_hsv.H; }
            set { UpdateColor(HSVColor.FromHSV(_hsv.A, value, _hsv.S, _hsv.V)); OnPropertyChanged(); }
        }

        public double S
        {
            get { return _hsv.S; }
            set { UpdateColor(HSVColor.FromHSV(_hsv.A, _hsv.H, value, _hsv.V)); OnPropertyChanged(); }
        }

        public double V
        {
            get { return _hsv.V; }
            set { UpdateColor(HSVColor.FromHSV(_hsv.A, _hsv.H, _hsv.S, value)); OnPropertyChanged(); }
        }

        public double HsvA
        {
            get { return _hsv.A; }
            set { UpdateColor(HSVColor.FromHSV(value, _hsv.H, _hsv.S, _hsv.V)); OnPropertyChanged(); }
        }


        private void Flush()
        {
            if (!_isPropertyLocked)
            {
                _rgb = Color;
                _hsv = Color.ToHSV();
            }
            OnPropertyChanged("");
        }

        private void UpdateColor(Color rgb)
        {
            _rgb = rgb;
            _hsv = _rgb.ToHSV();
            UpdateColor();
        }

        private void UpdateColor(HSVColor hsv)
        {
            _hsv = hsv;
            _rgb = _hsv.ToARGB();
            UpdateColor();
        }

        private void UpdateColor()
        {
            _isPropertyLocked = true;
            Color = _rgb;
            _isPropertyLocked = false;
        }

        private bool CanOpenPopup(UIElement popupElement)
        {
            return PopupWatcher.PopupElement != popupElement;
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (CanOpenPopup(this.EditPopup))
            {
                this.EditPopup.IsOpen = true;
            }
        }

        private void DropperButton_Click(object sender, RoutedEventArgs e)
        {
            EnterDropper();
            e.Handled = true;
        }

        private void EditPopup_Opened(object sender, EventArgs e)
        {
            PopupWatcher.SetPopupElement(sender, (UIElement)sender);

            _popupClosedFocusElement = null;

            _disablePopupBackgroundWheel = new DisablePopupBackgroundWheel((UIElement)sender, this.EditPopup);

            this.ColorComboBox.Focus();
        }

        private void EditPopup_Closed(object sender, EventArgs e)
        {
            PopupWatcher.SetPopupElement(sender, null);

            LeaveDropper();

            _disablePopupBackgroundWheel?.Dispose();
            _disablePopupBackgroundWheel = null;

            _popupClosedFocusElement?.Focus();
            _popupClosedFocusElement = null;
        }

        private void EditPopup_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }

        private void EditPopup_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (Mouse.Captured == this.PopupRoot)
                {
                    LeaveDropper();
                }
                else
                {
                    _popupClosedFocusElement = this.ColorButton;
                    this.EditPopup.IsOpen = false;
                }
                e.Handled = true;
            }
        }


        private void EditPopup_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.Captured == this.PopupRoot)
            {
                if (e.LeftButton == MouseButtonState.Pressed && !ColorDropper.IsClickOutsideApp(sender, e))
                {
                    Color = ColorDropper.GetColorUnderCursor();
                    EnterDragDropper();
                }
                e.Handled = true;
            }
        }

        private void EditPopup_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.Captured == this.PopupRoot)
            {
                LeaveDropper();
                e.Handled = true;
            }
        }

        private void EditPopup_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDraggingDropper)
            {
                Color = ColorDropper.GetColorUnderCursor();
            }
        }

        private void EnterDropper()
        {
            Mouse.Capture(this.PopupRoot);
            this.PopupRoot.Cursor = Cursors.Cross;
            _isDraggingDropper = false;
        }

        private void EnterDragDropper()
        {
            Mouse.Capture(this.PopupRoot);
            this.PopupRoot.Cursor = Cursors.Cross;
            _isDraggingDropper = true;
        }

        private void LeaveDropper()
        {
            if (Mouse.Captured == this.PopupRoot)
            {
                Mouse.Capture(null);
            }
            this.PopupRoot.Cursor = null;
            _isDraggingDropper = false;
        }
    }

}
