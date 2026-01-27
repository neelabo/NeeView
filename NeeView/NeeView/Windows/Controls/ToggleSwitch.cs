using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace NeeView.Windows.Controls
{
    /// <summary>
    /// ToggleSwitch.xaml の相互作用ロジック
    /// </summary>
    public partial class ToggleSwitch : Control
    {
        static ToggleSwitch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleSwitch), new FrameworkPropertyMetadata(typeof(ToggleSwitch)));
        }

        private Grid? _root;
        private Canvas? _canvas;
        private Rectangle? _rectangle;
        private Ellipse? _ellipse;
        private ScaleTransform? _thumbScale;
        private TranslateTransform? _thumbTranslate;

        private Storyboard? _onAnimation;
        private Storyboard? _offAnimation;

        private bool _pressed;
        private Point _startPos;
        private double _startX;
        private const double _max = 20;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _onAnimation = this.FindResource("OnAnimation") as Storyboard ?? throw new InvalidOperationException();
            _offAnimation = this.FindResource("OffAnimation") as Storyboard ?? throw new InvalidOperationException();

            _root = this.GetTemplateChild("PART_Root") as Grid ?? throw new InvalidOperationException();
            _canvas = this.GetTemplateChild("PART_Canvas") as Canvas ?? throw new InvalidOperationException();
            _rectangle = this.GetTemplateChild("PART_Rectangle") as Rectangle ?? throw new InvalidOperationException();
            _ellipse = this.GetTemplateChild("PART_Ellipse") as Ellipse ?? throw new InvalidOperationException();
            _thumbScale = this.GetTemplateChild("PART_ThumbScale") as ScaleTransform ?? throw new InvalidOperationException();
            _thumbTranslate = this.GetTemplateChild("PART_ThumbTranslate") as TranslateTransform ?? throw new InvalidOperationException();

            this.IsEnabledChanged += (s, e) => UpdateBrush();

            this.KeyDown += ToggleSwitch_KeyDown;
            _root.MouseEnter += BaseGrid_MouseEnter;
            _root.MouseLeave += BaseGrid_MouseLeave;
            _root.MouseLeftButtonDown += BaseGrid_MouseLeftButtonDown;
            _root.MouseLeftButtonUp += BaseGrid_MouseLeftButtonUp;
            _root.MouseMove += BaseGrid_MouseMove;

            UpdateBrush();
            UpdateThumb(true);
        }

        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register("Stroke", typeof(Brush), typeof(ToggleSwitch), new PropertyMetadata(Brushes.Black, BrushProperty_Changed));


        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(Brush), typeof(ToggleSwitch), new PropertyMetadata(Brushes.White, BrushProperty_Changed));


        public Brush DisableBrush
        {
            get { return (Brush)GetValue(DisableBrushProperty); }
            set { SetValue(DisableBrushProperty, value); }
        }

        public static readonly DependencyProperty DisableBrushProperty =
            DependencyProperty.Register("DisableBrush", typeof(Brush), typeof(ToggleSwitch), new PropertyMetadata(Brushes.Gray, BrushProperty_Changed));


        public Brush SelectBrush
        {
            get { return (Brush)GetValue(SelectBrushProperty); }
            set { SetValue(SelectBrushProperty, value); }
        }

        public static readonly DependencyProperty SelectBrushProperty =
            DependencyProperty.Register("SelectBrush", typeof(Brush), typeof(ToggleSwitch), new PropertyMetadata(Brushes.White, BrushProperty_Changed));


        public Brush CheckedBrush
        {
            get { return (Brush)GetValue(CheckedBrushProperty); }
            set { SetValue(CheckedBrushProperty, value); }
        }

        public static readonly DependencyProperty CheckedBrushProperty =
            DependencyProperty.Register("CheckedBrush", typeof(Brush), typeof(ToggleSwitch), new PropertyMetadata(Brushes.SteelBlue, BrushProperty_Changed));


        public Brush CheckedThumbBrush
        {
            get { return (Brush)GetValue(CheckedThumbBrushProperty); }
            set { SetValue(CheckedThumbBrushProperty, value); }
        }

        public static readonly DependencyProperty CheckedThumbBrushProperty =
            DependencyProperty.Register("CheckedThumbBrush", typeof(Brush), typeof(ToggleSwitch), new PropertyMetadata(Brushes.White, BrushProperty_Changed));

        private static void BrushProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ToggleSwitch)?.UpdateBrush();
        }


        public bool ShowState
        {
            get { return (bool)GetValue(ShowStateProperty); }
            set { SetValue(ShowStateProperty, value); }
        }

        public static readonly DependencyProperty ShowStateProperty =
            DependencyProperty.Register("ShowState", typeof(bool), typeof(ToggleSwitch), new PropertyMetadata(false));


        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(ToggleSwitch), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, IsCheckedProperty_Changed));

        private static void IsCheckedProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ToggleSwitch control)
            {
                control.UpdateBrush();
                control.UpdateThumb(false);
            }
        }


        private void UpdateBrush()
        {
            if (_rectangle is null) return;
            if (_ellipse is null) return;

            if (_pressed)
            {
                _rectangle.Fill = Brushes.Gray;
                _rectangle.Stroke = Brushes.Gray;
                _ellipse.Fill = Brushes.White;
            }
            else if (this.IsChecked)
            {
                _rectangle.Fill = IsEnabled ? this.CheckedBrush : this.DisableBrush;
                _rectangle.Stroke = IsEnabled ? this.CheckedBrush : this.DisableBrush;
                _ellipse.Fill = this.CheckedThumbBrush;
            }
            else
            {
                _rectangle.Fill = this.IsMouseOver ? this.SelectBrush : this.Fill;
                _rectangle.Stroke = IsEnabled ? this.Stroke : this.DisableBrush;
                _ellipse.Fill = IsEnabled ? this.Stroke : this.DisableBrush;
            }
        }

        private void UpdateThumb(bool flush)
        {
            if (_root is null) return;
            if (_onAnimation is null) return;
            if (_offAnimation is null) return;

            if (!flush && this.IsLoaded && SystemParameters.MenuAnimation)
            {
                if (this.IsChecked)
                {
                    _root.BeginStoryboard(_onAnimation);
                }
                else
                {
                    _root.BeginStoryboard(_offAnimation);
                }
            }
            else
            {
                if (this.IsChecked)
                {
                    OnAnimation_Completed(this, EventArgs.Empty);
                }
                else
                {
                    OffAnimation_Completed(this, EventArgs.Empty);
                }
            }
        }

        private void BaseGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_root is null) return;

            this.Focus();

            MouseInputHelper.CaptureMouse(this, _root);

            _thumbTranslate?.BeginAnimation(TranslateTransform.XProperty, null, HandoffBehavior.SnapshotAndReplace);

            _startPos = e.GetPosition(_root);
            _pressed = true;
            _startX = this.IsChecked ? _max : 0.0;

            UpdateBrush();

            BaseGrid_MouseMove(sender, e);
        }

        private void BaseGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_root is null) return;

            MouseInputHelper.ReleaseMouseCapture(this, _root);

            _pressed = false;

            var pos = e.GetPosition(_root);
            var dx = pos.X - _startPos.X;

            if (Math.Abs(dx) > SystemParameters.MinimumHorizontalDragDistance)
            {
                this.IsChecked = dx > 0;
            }
            else
            {
                this.IsChecked = !this.IsChecked;
            }

            UpdateBrush();
        }

        private void BaseGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (_thumbTranslate is null) return;
            if (!_pressed) return;

            var pos = e.GetPosition(_root);
            var dx = _startX + pos.X - _startPos.X;
            if (dx < 0.0) dx = 0.0;
            if (dx > _max) dx = _max;

            _thumbTranslate.X = dx;
        }

        private void OnAnimation_Completed(object? sender, EventArgs e)
        {
            if (_thumbTranslate is null) return;

            _thumbTranslate.X = _max;
        }

        private void OffAnimation_Completed(object? sender, EventArgs e)
        {
            if (_thumbTranslate is null) return;

            _thumbTranslate.X = 0.0;
        }

        private void ToggleSwitch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.Enter)
            {
                IsChecked = !IsChecked;
                e.Handled = true;
            }
        }

        private void BaseGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            UpdateBrush();
        }

        private void BaseGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            UpdateBrush();
        }
    }


    public class BooleanToSwitchStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? TextResources.GetString("Word.On") : TextResources.GetString("Word.Off");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
