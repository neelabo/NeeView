using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// PageSliderView.xaml の相互作用ロジック
    /// </summary>
    public partial class PageSliderView : UserControl
    {
        private PageSliderViewModel? _vm;


        public PageSlider Source
        {
            get { return (PageSlider)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(PageSlider), typeof(PageSliderView), new PropertyMetadata(null, Source_Changed));

        private static void Source_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PageSliderView control)
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
            DependencyProperty.Register("IsBackgroundOpacityEnabled", typeof(bool), typeof(PageSliderView), new PropertyMetadata(false));


        public bool IsBorderVisible
        {
            get { return (bool)GetValue(IsBorderVisibleProperty); }
            set { SetValue(IsBorderVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsBorderVisibleProperty =
            DependencyProperty.Register("IsBorderVisible", typeof(bool), typeof(PageSliderView), new PropertyMetadata(false));


        public PageSliderView()
        {
            InitializeComponent();
        }


        public void Initialize()
        {
            if (this.Source == null) return;

            _vm = new PageSliderViewModel(this.Source);
            this.Root.DataContext = _vm;

            // マーカー初期化
            this.PageMarkersView.Source = this.Source.PageMarkers;

            // 
            _vm.Model.AddPropertyChanged(nameof(PageSlider.IsSliderDirectionReversed), Model_IsSliderDirectionReversedChanged);
        }


        // スライダーの方向切替反映
        public void Model_IsSliderDirectionReversedChanged(object? sender, PropertyChangedEventArgs e)
        {
            // nop.
        }


        /// <summary>
        /// スライダーエリアでのマウスホイール操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SliderArea_MouseWheel(object? sender, MouseWheelEventArgs e)
        {
            if (_vm is null) return;

            _vm.MouseWheel(sender, e);
        }

        private void PageSlider_PreviewMouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
        {
            // 操作するときはメインビューにフォーカスを移動する
            MainWindowModel.Current.FocusMainView();
        }

        private void PageSlider_PreviewMouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
        {
            if (_vm is null) return;

            _vm.Jump(false);
        }

        private void PageSliderTextBox_ValueChanged(object? sender, EventArgs e)
        {
            if (_vm is null) return;

            _vm.Jump(true);
        }

        // テキストボックス入力時に単キーのショートカットを無効にする
        private void PageSliderTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            // 単キーのショートカット無効
            KeyExGesture.AddFilter(KeyExGestureFilter.All);
            //e.Handled = true;
        }

    }
}

