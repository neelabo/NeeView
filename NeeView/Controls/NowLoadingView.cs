using System;
using System.Collections.Generic;
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

namespace NeeView
{
    public class NowLoadingView : Control
    {
        static NowLoadingView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NowLoadingView), new FrameworkPropertyMetadata(typeof(NowLoadingView)));
        }


        public NowLoading Source
        {
            get { return (NowLoading)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(NowLoading), typeof(NowLoadingView), new PropertyMetadata(null, Source_Changed));

        private static void Source_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as NowLoadingView)?.Initialize();
        }


        private NowLoadingViewModel? _vm;
        private Grid? _root;
        private Grid? _nowLoading;
        private Grid? _nowLoadingNormal;
        private TextBlock? _nowLoadingNormalText;
        private ProgressRing? _progressRing;
        private StackPanel? _nowLoadingTiny;
        private TextBlock? _nowLoadingTextTiny;
        private ProgressRing? _progressRingTiny;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _root = this.GetTemplateChild("PART_Root") as Grid ?? throw new InvalidOperationException();
            _nowLoading = this.GetTemplateChild("PART_NowLoading") as Grid ?? throw new InvalidOperationException();
            _nowLoadingNormal = this.GetTemplateChild("PART_NowLoadingNormal") as Grid ?? throw new InvalidOperationException();
            _nowLoadingNormalText = this.GetTemplateChild("PART_NowLoadingText") as TextBlock ?? throw new InvalidOperationException();
            _progressRing = this.GetTemplateChild("PART_ProgressRing") as ProgressRing ?? throw new InvalidOperationException();
            _nowLoadingTiny = this.GetTemplateChild("PART_NowLoadingTiny") as StackPanel ?? throw new InvalidOperationException();
            _nowLoadingTextTiny = this.GetTemplateChild("PART_NowLoadingTextTiny") as TextBlock ?? throw new InvalidOperationException();
            _progressRingTiny = this.GetTemplateChild("PART_ProgressRingTiny") as ProgressRing ?? throw new InvalidOperationException();

            if (this.Source is not null)
            {
                Initialize();
            }
        }


        public void Initialize()
        {
            if (_root is null) return;

            _vm = new NowLoadingViewModel(this.Source);
            _root.DataContext = _vm;

            _vm.AddPropertyChanged(nameof(_vm.IsDisplayNowLoading),
                (s, e) => DisplayNowLoading(_vm.IsDisplayNowLoading));
        }


        /// <summary>
        /// NowLoadingの表示/非表示
        /// </summary>
        /// <param name="isDisplay"></param>
        private void DisplayNowLoading(bool isDisplay)
        {
            if (_nowLoading is null) return;
            if (_nowLoadingNormal is null) return;
            if (_nowLoadingNormalText is null) return;
            if (_progressRing is null) return;
            if (_nowLoadingTiny is null) return;
            if (_nowLoadingTextTiny is null) return;
            if (_progressRingTiny is null) return;

            if (isDisplay && Config.Current.Notice.NowLoadingShowMessageStyle != ShowMessageStyle.None)
            {
                var ani = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5)) { BeginTime = TimeSpan.FromSeconds(0.5) };
                _nowLoading.BeginAnimation(UIElement.OpacityProperty, ani, HandoffBehavior.SnapshotAndReplace);

                switch (Config.Current.Notice.NowLoadingShowMessageStyle)
                {
                    case ShowMessageStyle.Normal:
                        _nowLoadingNormal.Visibility = Visibility.Visible;
                        _nowLoadingTiny.Visibility = Visibility.Collapsed;
                        _nowLoadingNormalText.Visibility = Visibility.Visible;
                        _progressRing.IsActive = true;
                        _progressRingTiny.IsActive = false;
                        break;

                    case ShowMessageStyle.NormalIconOnly:
                        _nowLoadingNormal.Visibility = Visibility.Visible;
                        _nowLoadingTiny.Visibility = Visibility.Collapsed;
                        _nowLoadingNormalText.Visibility = Visibility.Collapsed;
                        _progressRing.IsActive = true;
                        _progressRingTiny.IsActive = false;
                        break;

                    case ShowMessageStyle.Tiny:
                        _nowLoadingNormal.Visibility = Visibility.Collapsed;
                        _nowLoadingTiny.Visibility = Visibility.Visible;
                        _nowLoadingTextTiny.Visibility = Visibility.Visible;
                        _nowLoadingTiny.Background = new SolidColorBrush(Color.FromArgb(0x88, 0x00, 0x00, 0x00));
                        _progressRingTiny.Foreground = Brushes.White;
                        _progressRing.IsActive = false;
                        _progressRingTiny.IsActive = true;
                        break;

                    case ShowMessageStyle.TinyIconOnly:
                        _nowLoadingNormal.Visibility = Visibility.Collapsed;
                        _nowLoadingTiny.Visibility = Visibility.Visible;
                        _nowLoadingTextTiny.Visibility = Visibility.Collapsed;
                        _nowLoadingTiny.Background = Brushes.Transparent;
                        _progressRingTiny.Foreground = App.Current.Resources["Control.Accent"] as Brush ?? Brushes.Gray;
                        _progressRing.IsActive = false;
                        _progressRingTiny.IsActive = true;
                        break;

                    default:
                        throw new InvalidOperationException("Unknown ShowMessageStyle: " + Config.Current.Notice.NowLoadingShowMessageStyle);
                }
            }
            else
            {
                var ani = new DoubleAnimation(0, TimeSpan.FromSeconds(0.25));
                _nowLoading.BeginAnimation(UIElement.OpacityProperty, ani, HandoffBehavior.SnapshotAndReplace);
                _progressRing.IsActive = false;
                _progressRingTiny.IsActive = false;
            }
        }
    }

}
