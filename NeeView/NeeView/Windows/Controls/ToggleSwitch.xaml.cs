using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView.Windows.Controls
{
    /// <summary>
    /// ToggleSwitch.xaml の相互作用ロジック
    /// </summary>
    public partial class ToggleSwitch : UserControl
    {
        private readonly Storyboard _onAnimation;
        private readonly Storyboard _offAnimation;

        private bool _pressed;
        private Point _startPos;
        private double _startX;
        private const double _max = 20;


        public ToggleSwitch()
        {
            InitializeComponent();
            this.Root.DataContext = this;

            _onAnimation = (Storyboard)this.Root.Resources["OnAnimation"];
            _offAnimation = (Storyboard)this.Root.Resources["OffAnimation"];

            this.IsEnabledChanged += (s, e) => UpdateBrush();
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
                control.UpdateThumb();
            }
        }


        private void UpdateBrush()
        {
            if (_pressed)
            {
                this.rectangle.Fill = Brushes.Gray;
                this.rectangle.Stroke = Brushes.Gray;
                this.ellipse.Fill = Brushes.White;
            }
            else if (this.IsChecked)
            {
                this.rectangle.Fill = IsEnabled ? this.CheckedBrush : this.DisableBrush;
                this.rectangle.Stroke = IsEnabled ? this.CheckedBrush : this.DisableBrush;
                this.ellipse.Fill = this.CheckedThumbBrush;
            }
            else
            {
                this.rectangle.Fill = this.IsMouseOver ? this.SelectBrush : this.Fill;
                this.rectangle.Stroke = IsEnabled ? this.Stroke : this.DisableBrush;
                this.ellipse.Fill = IsEnabled ? this.Stroke : this.DisableBrush;
            }
        }

        private void UpdateThumb()
        {
            if (this.IsLoaded && SystemParameters.MenuAnimation)
            {
                if (this.IsChecked)
                {
                    this.Root.BeginStoryboard(_onAnimation);
                }
                else
                {
                    this.Root.BeginStoryboard(_offAnimation);
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
            this.Focus();

            MouseInputHelper.CaptureMouse(this, this.Root);

            _startPos = e.GetPosition(this.Root);
            _pressed = true;
            _startX = this.IsChecked ? _max : 0.0;

            UpdateBrush();
        }

        private void BaseGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MouseInputHelper.ReleaseMouseCapture(this, this.Root);

            _pressed = false;

            var pos = e.GetPosition(this.Root);
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
            if (!_pressed) return;

            var pos = e.GetPosition(this.Root);
            var dx = _startX + pos.X - _startPos.X;
            if (dx < 0.0) dx = 0.0;
            if (dx > _max) dx = _max;

            this.thumbTranslate.X = dx;
        }

        private void OnAnimation_Completed(object sender, EventArgs e)
        {
            this.thumbTranslate.X = _max;
        }

        private void OffAnimation_Completed(object sender, EventArgs e)
        {
            this.thumbTranslate.X = 0.0;
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
            return (bool)value ? Properties.TextResources.GetString("Word.On") : Properties.TextResources.GetString("Word.Off");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
