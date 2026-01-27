using NeeLaboratory;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// VideoControl.xaml の相互作用ロジック
    /// </summary>
    public partial class MediaControlView : UserControl
    {
        private MediaControlViewModel? _vm;


        public MediaControlView()
        {
            InitializeComponent();
        }


        public MediaControl Source
        {
            get { return (MediaControl)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(MediaControl), typeof(MediaControlView), new PropertyMetadata(null, Source_Changed));

        private static void Source_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MediaControlView control)
            {
                control.Initialize();
            }
        }

        public bool IsBackgroundOpacityEnabled
        {
            get { return (bool)GetValue(IsBackgroundOpacityEnabledProperty); }
            set { SetValue(IsBackgroundOpacityEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsBackgroundOpacityEnabledProperty =
            DependencyProperty.Register("IsBackgroundOpacityEnabled", typeof(bool), typeof(MediaControlView), new PropertyMetadata(false));


        public bool IsMainViewFocusEnabled
        {
            get { return (bool)GetValue(IsMainViewFocusEnabledProperty); }
            set { SetValue(IsMainViewFocusEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsMainViewFocusEnabledProperty =
            DependencyProperty.Register("IsMainViewFocusEnabled", typeof(bool), typeof(MediaControlView), new PropertyMetadata(false));



        public void Initialize()
        {
            if (Source == null) return;

            _vm = new MediaControlViewModel(Source);
            this.Root.DataContext = _vm;
        }

        private void VideoSlider_DragStarted(object? sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _vm?.SetScrubbing(true);
        }

        private void VideoSlider_DragCompleted(object? sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _vm?.SetScrubbing(false);
        }

        private void VideoSlider_PreviewMouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
        {
            if (IsMainViewFocusEnabled)
            {
                MainWindowModel.Current.FocusMainView();
            }
        }

        private void TimeTextBlock_MouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
        {
            _vm?.ToggleTimeFormat();
        }

        private void Root_MouseWheel(object? sender, MouseWheelEventArgs e)
        {
            _vm?.MouseWheel(sender, e);
            e.Handled = true;
        }

        private void Volume_PreviewMouseDown(object? sender, MouseButtonEventArgs e)
        {
            this.VolumeSlider.Focus();
        }

        private void Volume_MouseWheel(object? sender, MouseWheelEventArgs e)
        {
            _vm?.MouseWheelVolume(sender, e);
            e.Handled = true;
        }

        private void Volume_KeyDown(object? sender, KeyEventArgs e)
        {
            if (_vm is null) return;

            e.Handled = _vm.KeyVolume(e.Key);
        }
    }


    public class EnableValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)values[1] ? values[0] : 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[]
            {
                value
            };
        }
    }

    public class RootWidthToVolumeSlierWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rootWidth = (double)value;
            var defaultWidth = double.Parse((string)parameter, CultureInfo.InvariantCulture);

            var rate = MathUtility.Clamp((rootWidth - 300.0) / 200.0, 0.0, 1.0);
            return (defaultWidth * 0.5) * (1.0 + rate);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
