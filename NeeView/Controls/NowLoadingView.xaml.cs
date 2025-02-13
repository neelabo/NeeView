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
    /// <summary>
    /// NowLoading.xaml の相互作用ロジック
    /// </summary>
    public partial class NowLoadingView : UserControl
    {
        public NowLoading Source
        {
            get { return (NowLoading)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(NowLoading), typeof(NowLoadingView), new PropertyMetadata(null, Source_Changed));

        private static void Source_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as NowLoadingView)?.Initialize();
        }

        //
        public NowLoadingView()
        {
            InitializeComponent();
            this.NowLoading.Opacity = 0.0;
        }

        //
        private NowLoadingViewModel? _vm;

        //
        public void Initialize()
        {
            _vm = new NowLoadingViewModel(this.Source);
            this.Root.DataContext = _vm;

            _vm.AddPropertyChanged(nameof(_vm.IsDisplayNowLoading),
                (s, e) => DisplayNowLoading(_vm.IsDisplayNowLoading));
        }


        /// <summary>
        /// NowLoadingの表示/非表示
        /// </summary>
        /// <param name="isDisplay"></param>
        private void DisplayNowLoading(bool isDisplay)
        {
            if (isDisplay && Config.Current.Notice.NowLoadingShowMessageStyle != ShowMessageStyle.None)
            {
                if (Config.Current.Notice.NowLoadingShowMessageStyle == ShowMessageStyle.Normal)
                {
                    this.NowLoadingNormal.Visibility = Visibility.Visible;
                    this.NowLoadingTiny.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.NowLoadingNormal.Visibility = Visibility.Collapsed;
                    this.NowLoadingTiny.Visibility = Visibility.Visible;
                }

                var ani = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5)) { BeginTime = TimeSpan.FromSeconds(0.5) };
                this.NowLoading.BeginAnimation(UIElement.OpacityProperty, ani, HandoffBehavior.SnapshotAndReplace);

                this.ProgressRing.IsActive = true;
            }
            else
            {
                var ani = new DoubleAnimation(0, TimeSpan.FromSeconds(0.25));
                this.NowLoading.BeginAnimation(UIElement.OpacityProperty, ani, HandoffBehavior.SnapshotAndReplace);

                this.ProgressRing.IsActive = false;
            }
        }
    }

}
